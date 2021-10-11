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
    /// - without move ordering: 
    /// Depth = 1, Nodes evaluated: 42. In 7 [ms].
    /// Depth = 2, Nodes evaluated: 1782. In 19 [ms].
    /// Depth = 3, Nodes evaluated: 69737. In 726 [ms].
    /// Depth = 4, Nodes evaluated: 2885028. In 26914 [ms]
    /// </summary>
    public class AISearchEngine
    {
        /// <summary>
        /// Transposition table
        /// </summary>
        private TranspositionTable myTT { get; set; }
        private bool IsDarkSoldiers { get; set; }
        private int Depth { get; set; }

        public static Random rand { get; set; }

        #region counters
        private int nodesEvaluated { get; set; }
        private int prunnings { get; set; }
        private int type1e { get; set; }
        #endregion

        public BoardState Search(BoardState root, bool isDarkSoldiers, int depth)
        {
            rand = new Random();
            Console.WriteLine();
            Console.WriteLine("SEARCH ENGINE TURN");

            myTT = new TranspositionTable(root);
            IsDarkSoldiers = isDarkSoldiers;
            Depth = depth;
            nodesEvaluated = 0;
            prunnings = 0;
            type1e = 0;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int[] values = new int[2];
            // Iterative-deepening
            for (int d = 1; d <= Depth; d++)
            {
                values = AlphaBetaWithTT(root, -1000000, 100000, d);

                //searchEngine.myTT.ResetAllAncientFlags();

                var elapsed = stopWatch.ElapsedMilliseconds;
                Console.WriteLine("Depth = " + d +
                    ", Nodes Evaluated = " + nodesEvaluated +
                    " in " + elapsed.ToString() + "[ms]" +
                    ", prunnings = " + prunnings +
                    ", type-1 errors = " + type1e +
                    ", TT entries = " + myTT.TT.ToList().Where(x => x != null).ToList().Count);
            }
            int bestValue = values[0];
            int bestMove = values[1];

            CannonUtils.printMove(root.LegalMoves[bestMove], bestMove);

            return root.Successor(bestMove);
        }

        /// <summary>
        /// Evaluate State
        /// Material:   value = w1*f1(s) + w2*f2(s) + ....
        ///             where e.g. w3 = 9 and f3(s) = # soldiers (friend) 3 steps from oponent town - soldiers (enemy) 3 steps from oponent town
        /// Mobility:   value = # moves (friend) - # moves (enemy)
        /// EVALUATE RETURNS [this.Value] which is calculated at every 
        /// </summary>
        public int Evaluate(BoardState s)
        {
            int n_friends = 0;
            int n_enemies = 0;
            foreach (Cell soldier in s.SoldierList)
            {
                if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
                {
                    // Friend is dark
                    // Enemy is light
                    if(IsDarkSoldiers) { n_friends++; }
                    else { n_enemies++; }
                }
                else if (soldier.Piece == CannonUtils.ISoldiers.light_soldier)
                {
                    // Friend is light
                    // Enemy is dark
                    if (IsDarkSoldiers) { n_enemies++; }
                    else { n_friends++; }
                }
            }
            int evaluation = (n_friends - n_enemies) * 100;
            return -evaluation;
        }

        /// <summary>
        /// Decision algorithm to find the best move given current state of the board
        /// Call: AlphaBetaWithTT(s, -inf, inf, depth)
        /// </summary>
        private int[] AlphaBetaWithTT(BoardState s, int alpha, int beta, int depth)
        {
            // save original alpha value
            int olda = alpha;
            nodesEvaluated++;

            // Transposition-table lookup
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

            // Check if terminal node
            if (depth == 0)
            {
                // leaf node
                return new int[] { Evaluate(s), 0 };
            }
            else if (s.TerminalState != CannonUtils.INode.leaf)
            {
                // terminal node
                // TODO
                return new int[] { Evaluate(s), 0 };
            }

            // We could not cut-off so we need to investigate deeper

            int bestValue = -100000000;
            int bestMove = 0;
            //List<int> child_list = Enumerable.Range(0, s.LegalMoves.Count).ToList();

            if (n.Depth >= depth)
            {
                // if the TT does not give a cutoff, we play the best move as first
                bestValue = n.Score;
                bestMove = n.BestMove;
                int result = 0;
                // Do move ordering with child list
                try
                {
                    // First iteration with bestMove
                    result = -AlphaBetaWithTT(s.Successor(bestMove), -beta, -alpha, depth - 1)[0];

                }
                catch (Exception)
                {
                    // Type-1 error 
                    // ignore it
                    type1e++;
                }

                if (result > bestValue) { bestValue = result; }
                if (bestValue > alpha) { alpha = bestValue; }
                if (bestValue >= beta) { prunnings++; }
                else
                {
                    // Regular alpha-beta search algorithm
                    for (int child = 0; child < s.LegalMoves.Count; child++)
                    {
                        if (child != bestMove)
                        {
                            result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1)[0];
                            if (result > bestValue)
                            {
                                bestValue = result;
                                bestMove = child;
                            }
                            if (bestValue > alpha) { alpha = bestValue; }
                            if (bestValue >= beta)
                            {
                                prunnings++;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // if position is not found, n.depth will be -1
                // Regular alpha-beta search algorithm
                for (int child = 0; child < s.LegalMoves.Count; child++)
                {
                    int result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1)[0];
                    if (result > bestValue)
                    {
                        bestValue = result;
                        bestMove = child;
                    }
                    if (bestValue > alpha) { alpha = bestValue; }
                    if (bestValue >= beta)
                    {
                        prunnings++;
                        break;
                    }
                    //if (depth == Depth)
                    //{
                    //    CannonUtils.printMove(s.LegalMoves[child], child);
                    //    Console.WriteLine("                                             value = " + result);
                    //}
                }
            }

            // Traditional transposition table storing of bounds
            AIUtils.ITTEntryFlag flag = AIUtils.ITTEntryFlag.exact_value;
            // Fail-low result implies an upper bound
            if (bestValue <= olda) { flag = AIUtils.ITTEntryFlag.upper_bound; }
            // Fail-high result implies a lower bound
            else if (bestValue >= beta) { flag = AIUtils.ITTEntryFlag.lower_bound; }

            if (n.Depth == -1)
            {
                // store information in the TT
                myTT.Store(s, bestMove, bestValue, flag, depth);
            }
            else if (n.Depth >= depth)
            {
                // If current state has already been stored in TT but it was less deep than current evaluation, update bestMove
                myTT.Update(bestMove, bestValue, flag, depth, n.zobristHashKey);
            }

            return new int[] { bestValue, bestMove };
        }

        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.Friend);
            myBoard.AddTown(4, myBoard.Friend);
        }
    }
}
