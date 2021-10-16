using System;
using CannonModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace SearchEngine
{
    /// <summary>
    /// Reference search speed values:
    /// - With move ordering alpha beta: 
    /// Depth 4 -> Nodes evaluated: 73678. In 1252 [ms]
    /// </summary>
    public class AISearchEngine
    {
        /// <summary>
        /// Transposition table
        /// </summary>
        private TranspositionTable myTT { get; set; }
        private int[] KillerMoveList { get; set; }
        private int Color { get; set; }
        private int Depth { get; set; }
        public static Random rand { get; set; }
        public AIUtils.IEval eval_f { get; set; }
        //private List<EvaluatedNode> moveEvaluationList { get; set; } 


        #region counters
        private int nodesEvaluated { get; set; }
        private int prunnings { get; set; }
        #endregion

        public AISearchEngine(AIUtils.IEval function, int color)
        {
            eval_f = function;
            Color = color;
        }

        public BoardState Search(BoardState root, int depth)
        {
            rand = new Random();
            Console.WriteLine();

            myTT = new TranspositionTable(root);
            // KillerMoves = new int[Depth];
            
            Depth = depth;
            
            if (Color == 1) { Console.WriteLine("DARK AI's TURN"); }
            else { Console.WriteLine("LIGHT AI's TURN"); }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int[] values = new int[2];
            // Iterative-deepening
            for (int d = 1; d <= Depth; d++)
            {
                nodesEvaluated = 0;
                prunnings = 0;
                values = AlphaBetaWithTT(root, -1000000, 100000, d, Color);

                //searchEngine.myTT.ResetAllAncientFlags();

                var elapsed = stopWatch.ElapsedMilliseconds;
                Console.WriteLine("Depth = " + d +
                    ", Nodes Evaluated = " + nodesEvaluated +
                    " in " + elapsed.ToString() + "[ms]" +
                    ", prunnings = " + prunnings +
                    ", TT entries = " + myTT.TT.ToList().Where(x => x != null).ToList().Count);

                if (elapsed > 2000)
                {
                    break;
                }
            }
            int bestValue = values[0];
            int bestMove = values[1];
            // DELETE
            //List<EvaluatedNode> newEvaluatedL = moveEvaluationList.OrderByDescending(x => x.value).ToList();
            //CannonUtils.printLegalMovesWithScore(newEvaluatedL.Select(x => x.move).ToList(), newEvaluatedL.Select(x => x.value).ToList());
            //Console.WriteLine("AI optimal move:");
            //--------
            CannonUtils.printMove(root.FriendLegalMoves[bestMove], bestMove);
            BoardState new_s = root.Successor(bestMove);

            return new_s;
        }

        /// <summary>
        /// Decision algorithm to find the best move given current state of the board
        /// Call: AlphaBetaWithTT(s, -inf, inf, depth)
        /// </summary>
        private int[] AlphaBetaWithTT(BoardState s, int alpha, int beta, int depth, int color)
        {
            // save original alpha value
            int olda = alpha;
            nodesEvaluated++;

            //moveEvaluationList = new List<EvaluatedNode>();

            #region Transposition-table lookup
            Entry n = myTT.TableLookup(s);
            if (n.Depth >= depth)
            {
                // [n] is deeper than current depth or the same (means it is more interesting than current depth)
                if (n.Flag == AIUtils.ITTEntryFlag.exact_value) { return new int[] { n.Score, n.BestMove }; }
                else if (n.Flag == AIUtils.ITTEntryFlag.lower_bound) { alpha = n.Score > alpha ? n.Score : alpha; }
                else if (n.Flag == AIUtils.ITTEntryFlag.upper_bound) { beta = n.Score < beta ? n.Score : beta; }
                if (alpha >= beta)
                {
                    prunnings++;
                    return new int[] { n.Score, n.BestMove };
                }
            }
            #endregion

            #region Terminal or Leaf node
            if (s.TerminalState != CannonUtils.INode.leaf)
            {
                // terminal node
                int score = 100000;
                if(s.TerminalState == CannonUtils.INode.light_wins) { score = -score; }
                return new int[] { score * color, 0 };
            }
            else if (depth == 0)
            {
                // leaf node
                return new int[] { Evaluate(s) * color, 0 };
            }
            #endregion

            // We could not cut-off with TT entry so we need to investigate deeper

            int bestValue = -100000000;
            int bestMove = 0;

            int n_moves = s.FriendLegalMoves.Count;
            List<int> successor_list = Enumerable.Range(0, n_moves).ToList();

            #region Killer Heuristics
            // killer Move after TT bestMove
            //int killer = KillerMoves[depth];
            //if (KillerMoves[depth] != null && killer < n_moves)
            //{
            //    successor_list.Insert(0, killer);
            //    successor_list.RemoveAt(killer);
            //}
            #endregion

            // TT move ordering
            if (n.Depth != -1 && bestMove < n_moves)
            {
                // if the TT does not give a cutoff, we play the best move as first
                successor_list.Insert(0, bestMove);
                successor_list.RemoveAt(bestMove);
            }

            // if position is not found, n.depth will be -1
            // Regular alpha-beta search algorithm
            //List<EvaluatedNode> scoreList = new List<EvaluatedNode>();
            foreach (int child in successor_list)
            {
                int result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1, -color)[0];
                //scoreList.Add(new EvaluatedNode() {depth=depth, move=s.FriendLegalMoves[child], value=result });
                if (result > bestValue)
                {
                    bestValue = result;
                    bestMove = child;
                }
                if (bestValue > alpha) { alpha = bestValue; }
                if (bestValue >= beta)
                {
                    // KillerMoves[depth] = bestValue;
                    prunnings++;
                    break;
                }                
            }
            //moveEvaluationList = scoreList;
            // Traditional transposition table storing of bounds
            AIUtils.ITTEntryFlag flag = AIUtils.ITTEntryFlag.exact_value;
            // Fail-low result implies an upper bound
            if (bestValue <= olda) { flag = AIUtils.ITTEntryFlag.upper_bound; }
            // Fail-high result implies a lower bound
            else if (bestValue >= beta) { flag = AIUtils.ITTEntryFlag.lower_bound; }

            // store information in the TT
            myTT.Store(s, bestMove, bestValue, flag, depth);            

            return new int[] { bestValue, bestMove };
        }

        /// <summary>
        /// Evaluate given Board position
        /// Must return a score relative to the side to being evaluated
        /// </summary>
        public int Evaluate(BoardState s)
        {
            switch (eval_f)
            {
                case AIUtils.IEval.color:
                    return Evaluation.byColor(s);
                case AIUtils.IEval.dist2EnemyTown:
                    return Evaluation.evalByTypeAndDistanceToTown(s);
                case AIUtils.IEval.mobility:
                    return Evaluation.mobility(s);
                case AIUtils.IEval.safeMobility:
                    return Evaluation.safe_mobility(s);
            }
            return 0;
        }

        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.Friend);
            myBoard.AddTown(4, myBoard.Friend);
        }
    }

    public class EvaluatedNode
    {
        public Move move { get; set; }
        public int value { get; set; }
        public int depth { get; set; }
    }
}
