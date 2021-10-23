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
        #region properties
        private TranspositionTable transpositionTable;
        /// <summary>
        /// killerMoves[ply][slot]
        /// https://stackoverflow.com/questions/17692867/implementing-killer-heuristic-in-alpha-beta-search-in-chess
        /// </summary>
        private Move[,] killerMoves { get; set; }
        private int[,] darkHistory { get; set; }
        private int[,] lightHistory { get; set; }
        private int optimalMove { get; set; }
        #endregion

        #region Settings
        private readonly int myColor;
        private readonly AIUtils.eEval evaluationF;
        private readonly bool isTT = true;
        private readonly bool isMultiCut = false;
        private readonly bool isKillerHeuristics = true;
        private readonly bool isHistoryHeuristics = true;
        private readonly bool isAspirationalSearch = true;
        private readonly bool isNullMove = false;
        private const int searchDepth = 30;
        private readonly bool printEvaluatedMoves = false;
        #endregion

        #region counters
        private int nodesEvaluated { get; set; }
        private int prunnings { get; set; }
        private int tt_prunings { get; set; }
        private List<EvaluatedNode> moveEvaluationList { get; set; }
        #endregion

        public AISearchEngine(AIUtils.eEval function, int color)
        {
            evaluationF = function;
            myColor = color;
        }

        public BoardState search(BoardState root)
        {
            // print current state of the board
            CannonUtils.printBoard(root, false);
            Console.WriteLine();

            // initialise enhancements
            transpositionTable = new TranspositionTable(root);
            killerMoves = new Move[100,2]; // make big enough and save best two killer moves per ply
            darkHistory = new int[100, 100]; // all possible moves
            lightHistory = new int[100, 100];

            // initialise timer
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // initialise counters
            nodesEvaluated = 0;
            prunnings = 0;
            tt_prunings = 0;

            // initialise alpha beta values
            int alpha = -50000;
            int beta = 50000;

            // Iterative-deepening
            for (int d = 1; d <= searchDepth; d++)
            {
                moveEvaluationList = new List<EvaluatedNode>();

                int bestValue = AlphaBeta(root, alpha, beta, d, myColor, 0, isMultiCut);
                var elapsed = stopWatch.ElapsedMilliseconds;
                Console.WriteLine("Depth = " + d +
                    ", Nodes Evaluated = " + nodesEvaluated +
                    " after " + elapsed.ToString() + "[ms]");

                if (elapsed > 60000)
                {
                    break;
                }

                if (isAspirationalSearch)
                {
                    if((bestValue <= alpha) || (bestValue >=beta))
                    {
                        alpha = -50000;
                        beta = 50000;
                        continue;
                    }

                    alpha = bestValue - 50;
                    beta = bestValue + 50;
                }
            }

            if (printEvaluatedMoves)
            {
                List<EvaluatedNode> newEvaluatedL = moveEvaluationList.OrderByDescending(x => x.value).ToList();
                CannonUtils.printLegalMovesWithScore(newEvaluatedL.Select(x => x.move).ToList(), newEvaluatedL.Select(x => x.value).ToList());
            }
            Console.WriteLine();
            Console.WriteLine("AI optimal move:");
            CannonUtils.printMove(root.legalMoves[optimalMove], optimalMove);
            BoardState new_s = root.Successor(optimalMove);

            return new_s;
        }

        /// <summary>
        /// Decision algorithm to find the best move given current state of the board
        /// Call: AlphaBetaWithTT(s, -inf, inf, depth)
        /// </summary>
        private int AlphaBeta(BoardState s, int alpha, int beta, int depth, int color, int ply, bool cut)
        {
            // increment node counter
            nodesEvaluated++;

            #region TT lookup and pruning
            // save original alpha value for TT flag
            int olda = alpha;

            // TT lookup
            Entry n = transpositionTable.TableLookup(s);

            // we can only use [n] when the entry depth is less deep than current [depth]
            if (n.Depth >= depth)
            {
                // [n] is deeper than current depth or the same (means it is more interesting than current depth)
                if (n.Flag == AIUtils.eTTEntryFlag.exact_value) 
                {
                    tt_prunings++;
                    if (ply == 0) optimalMove = n.BestMove;
                    return n.Score; 
                }
                else if (n.Flag == AIUtils.eTTEntryFlag.lower_bound) { alpha = n.Score > alpha ? n.Score : alpha; }
                else if (n.Flag == AIUtils.eTTEntryFlag.upper_bound) { beta = n.Score < beta ? n.Score : beta; }
                if (alpha >= beta)
                {
                    tt_prunings++;
                    if (ply == 0) optimalMove = n.BestMove;
                    return n.Score;
                }
            }
            #endregion

            // Terminal or Leaf node
            if (depth == 0 || s.terminalFlag != CannonUtils.eNode.leaf) { return Evaluation.Evaluate(s, evaluationF) * color; }

            // initialize alpha-beta parameters
            int bestValue = -100000000;
            int bestMove = 0; 

            #region null move pruning
            if(isNullMove && depth >= 3 && ply > 0)
            {
                // reduced depth factor
                int R = 2;

                // save board position
                BoardState copyBoard = s.DeepCopy();

                // change side
                copyBoard.turnCounter++;

                // generate moves for new position
                copyBoard.generateLegalMoves();

                // search with reduced depth
                int result = -AlphaBeta(copyBoard, -beta, -alpha, depth - 1 - R, -color, ply + 1, false);

                if(result >= beta) return beta;
            }

            #endregion

            List<int> successor_list = sortMoves(s, n, ply);

            #region multi-cut pruning
            if (cut && depth >= 3)
            {
                // multi-cut parameters
                int c = 0;
                int m = 0;
                int C = 3;
                int M = 10;
                int R = 2;

                while (m < M && m < successor_list.Count)
                {
                    // search with reduced depth
                    int result = -AlphaBeta(s.Successor(m), -beta, -alpha, depth - 1 - R, -color, ply + 1, false);
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
                            if (ply == 0) optimalMove = bestMove;
                            return beta;
                        }
                    }
                    m++;
                }
                successor_list.Insert(0, bestMove);
                successor_list.RemoveAt(bestMove);

                // re-initialise alpha-beta parameters after multi-cut
                bestValue = -100000000;
                bestMove = 0;
            }
            // start searching all children again
            #endregion

            foreach (int child in successor_list)
            {
                int result = -AlphaBeta(s.Successor(child), -beta, -alpha, depth - 1, -color, ply + 1, false);
                if(ply == 0) moveEvaluationList.Add(new EvaluatedNode() {depth=depth, move=s.legalMoves[child], value=result });

                if (result > bestValue)
                {
                    bestValue = result;
                    bestMove = child;
                    if (ply == 0) optimalMove = bestMove;
                }
                if (bestValue > alpha) { alpha = bestValue; }
                if (bestValue >= beta)
                {
                    // Killer move ordering and insert new move
                    killerMoves[ply,1] = killerMoves[ply,0];
                    killerMoves[ply,0] = s.legalMoves[bestMove];
                    prunnings++;
                    break;
                }                
            }

            #region HH increase bestMove weight
            if (isHistoryHeuristics && ply > 1)
            {
                Move move = s.legalMoves[bestMove];
                if(s.friendSoldier == CannonUtils.eSoldiers.dark_soldier)
                {
                    darkHistory[move.startIndex, move.targetIndex] += depth*depth;
                }
                else
                {
                    lightHistory[move.startIndex, move.targetIndex] += depth*depth; 
                }
            }
            #endregion

            #region TT (cont.) store position
            if (isTT)
            {
                // Traditional transposition table storing of bounds
                AIUtils.eTTEntryFlag flag;

                // Fail-low result implies an upper bound
                if (bestValue <= olda) { flag = AIUtils.eTTEntryFlag.upper_bound; }

                // Fail-high result implies a lower bound
                else if (bestValue >= beta) { flag = AIUtils.eTTEntryFlag.lower_bound; }
                else { flag = AIUtils.eTTEntryFlag.exact_value; }

                // store information in the TT
                transpositionTable.Store(s, bestMove, bestValue, flag, depth);
            }
            #endregion

            return bestValue;
        }

        /// <summary>
        /// Move ordering scheme:
        /// 1 - hash moves
        /// 2 - killer moves
        /// 3 - history heuristic
        /// 4 - domain knowledge heuristic
        /// </summary>
        private List<int> sortMoves(BoardState s, Entry n, int ply)
        {
            // generate list of move indices
            int n_moves = s.legalMoves.Count;
            List<int> successor_list = Enumerable.Range(0, n_moves).ToList();

            // if history heuristics
            if (isHistoryHeuristics)
            {
                // we need to order the list of legal moves [s.legalMoves] as HH dictates
                List<int> moveScores = new List<int>();
                int[,] HHTable = s.friendSoldier == CannonUtils.eSoldiers.dark_soldier ? darkHistory : lightHistory;
                foreach (Move move in s.legalMoves)
                {
                    moveScores.Add(HHTable[move.startIndex, move.targetIndex]);
                }

                if (moveScores.Any(v => v != 0)) successor_list = successor_list.OrderByDescending(d => moveScores.IndexOf(d)).ToList();
            }
            
            // if Killer Heuristics, put them at the beginning
            if (isKillerHeuristics)
            {
                for (int slot = 0; slot < 2; slot++)
                {
                    if (killerMoves[ply, slot] != null)
                    {
                        Move killerMove = killerMoves[ply, 0];
                        int killIndex = CannonUtils.getMoveIndex(s, killerMove);

                        if (killIndex != -1)
                        {
                            successor_list.Insert(0, killIndex);
                            successor_list.RemoveAt(killIndex);
                            break;
                        }
                    }
                }
            }

            // if TT entry found, put it at first position
            if (isTT && n.Depth != -1 && n.BestMove < n_moves)
            {
                // if position is not found in TT, n.depth will be -1
                successor_list.Insert(0, n.BestMove);
                successor_list.RemoveAt(n.BestMove);
            }
            return successor_list;
        }

        public void setTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.friendSoldier);
            myBoard.AddTown(4, myBoard.friendSoldier);
        }
    }

    public class EvaluatedNode
    {
        public Move move { get; set; }
        public int value { get; set; }
        public int depth { get; set; }
    }
}
