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
        /// <summary>
        /// killerMoves[ply][slot]
        /// https://stackoverflow.com/questions/17692867/implementing-killer-heuristic-in-alpha-beta-search-in-chess
        /// </summary>
        //private Move[,] killerMoves { get; set; }
        private int myColor { get; set; }
        public static Random rand { get; set; }
        public AIUtils.IEval eval_f { get; set; }
        private bool isMultiCut { get; set; }

        // private List<EvaluatedNode> moveEvaluationList { get; set; } 

        #region counters
        private int nodesEvaluated { get; set; }
        private int prunnings { get; set; }
        private int multi_cut_prunnings { get; set; }
        #endregion

        public AISearchEngine(AIUtils.IEval function, int color, bool is_multi_cut = true)
        {
            eval_f = function;
            myColor = color;
            isMultiCut = is_multi_cut;
        }

        public BoardState Search(BoardState root, int depth)
        {
            rand = new Random();
            Console.WriteLine();
            myTT = new TranspositionTable(root);
            //killerMoves = new Move[100, 2]; // make big enough and save best two killer moves per ply

            if (myColor == 1) { Console.WriteLine("DARK AI's TURN"); }
            else { Console.WriteLine("LIGHT AI's TURN"); }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int[] values = new int[2];
            // Iterative-deepening
            for (int d = 1; d <= depth; d++)
            {
                nodesEvaluated = 0;
                prunnings = 0;
                multi_cut_prunnings = 0;
                values = AlphaBetaWithTT(root, -1000000, 100000, d, myColor, 0);

                var elapsed = stopWatch.ElapsedMilliseconds;
                Console.WriteLine("Depth = " + d +
                    ", Nodes Evaluated = " + nodesEvaluated +
                    " in " + elapsed.ToString() + "[ms]" +
                    ", ab prunings = " + prunnings +
                    ", multi-cut prunings = " + multi_cut_prunnings +
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
            CannonUtils.printMove(root.legalMoves[bestMove], bestMove);
            BoardState new_s = root.Successor(bestMove);

            return new_s;
        }

        /// <summary>
        /// Decision algorithm to find the best move given current state of the board
        /// Call: AlphaBetaWithTT(s, -inf, inf, depth)
        /// </summary>
        private int[] AlphaBetaWithTT(BoardState s, int alpha, int beta, int depth, int color, int ply)
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

            // Terminal or Leaf node
            if (depth == 0 || s.terminalState != CannonUtils.INode.leaf) { return new int[] { Evaluate(s) * color, 0 }; }

            // We could not cut-off with TT entry so we need to investigate deeper
            int bestValue = -100000000;
            int bestMove = 0;
            int n_moves = s.legalMoves.Count;
            List<int> successor_list = Enumerable.Range(0, n_moves).ToList();

            #region multi-cut
            if (isMultiCut)
            {
                // before regular ab search
                int c = 0;
                int m = 0;
                int C = 3;
                int M = 10;
                int R = 2;
                while (m < M && m < n_moves)
                {
                    int new_depth = depth - 1 - R;
                    if (new_depth < 0) { new_depth = 0; }
                    int result = -AlphaBetaWithTT(s.Successor(m), -beta, -alpha, new_depth, -color, ply+1)[0];
                    if (result > bestValue)
                    {
                        bestValue = result;
                        bestMove = m;
                    }
                    if (result >= beta)
                    {
                        c++;
                        if (c >= C)
                        {
                            multi_cut_prunnings++;
                            return new int[] { beta, 0 };
                        }
                    }
                    m++;
                }
                successor_list.Insert(0, bestMove);
                successor_list.RemoveAt(bestMove);
            }
            // start searching all children again
            #endregion

            bestValue = -100000000;
            bestMove = 0;

            #region Killer Heuristics
            // killer Move after TT bestMove
            //for (int slot = 0; slot < 2; slot++)
            //{
            //    Move killerMove = killerMoves[ply,slot];

            //    for (int i = 0; i < s.legalMoves.Count; i++)
            //        if (CannonUtils.movesAreEqual(s.legalMoves[i], killerMove))
            //        {
            //            // moves[i] is a killer move so move it up the list
            //            successor_list.Insert(0, i);
            //            successor_list.RemoveAt(i);
            //            break;
            //        }
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
                BoardState newState = s.Successor(child);
                int result = -AlphaBetaWithTT(newState, -beta, -alpha, depth - 1, -color, ply + 1)[0];
                //scoreList.Add(new EvaluatedNode() {depth=depth, move=s.FriendLegalMoves[child], value=result });
                if (result > bestValue)
                {
                    bestValue = result;
                    bestMove = child;
                }
                if (bestValue > alpha) { alpha = bestValue; }
                if (bestValue >= beta)
                {
                    // Killer move ordering and insert new move
                    //killerMoves[ply,1] = killerMoves[ply,0];
                    //killerMoves[ply, 0] = newState.legalMoves[bestMove];

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
            if (s.terminalState != CannonUtils.INode.leaf)
            {
                // terminal node
                int score = 100000;
                return s.terminalState == CannonUtils.INode.light_wins? -score : score;
            }
            else
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
        }

        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.myFriend);
            myBoard.AddTown(4, myBoard.myFriend);
        }
    }

    public class EvaluatedNode
    {
        public Move move { get; set; }
        public int value { get; set; }
        public int depth { get; set; }
    }
}
