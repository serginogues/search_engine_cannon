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

        public AIUtils.IEval eval_f { get; set; }

        #region counters
        private int nodesEvaluated { get; set; }
        private int prunnings { get; set; }
        #endregion

        public BoardState Search(BoardState root, bool isDarkSoldiers, int depth, AIUtils.IEval f)
        {
            rand = new Random();
            Console.WriteLine();

            myTT = new TranspositionTable(root);
            IsDarkSoldiers = isDarkSoldiers;
            Depth = depth;
            eval_f = f;
            if (IsDarkSoldiers) { Console.WriteLine("DARK AI's TURN"); }
            else { Console.WriteLine("LIGHT AI's TURN"); }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int[] values = new int[2];
            // Iterative-deepening
            for (int d = 1; d <= Depth; d++)
            {
                nodesEvaluated = 0;
                prunnings = 0;
                values = AlphaBetaWithTT(root, -1000000, 100000, d);

                //searchEngine.myTT.ResetAllAncientFlags();

                var elapsed = stopWatch.ElapsedMilliseconds;
                //Console.WriteLine("Depth = " + d +
                //    ", Nodes Evaluated = " + nodesEvaluated +
                //    " in " + elapsed.ToString() + "[ms]" +
                //    ", prunnings = " + prunnings +
                //    ", TT entries = " + myTT.TT.ToList().Where(x => x != null).ToList().Count);
            }
            int bestValue = values[0];
            int bestMove = values[1];

            CannonUtils.printMove(root.LegalMoves[bestMove], bestMove);
            BoardState new_s = root.Successor(bestMove);

            return new_s;
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
            int evaluation = 0;
            // if(IsDarkSoldiers) then Friend is dark and Enemy is light

            foreach (Cell soldier in s.SoldierList)
            {
                switch(eval_f)
                {
                    case AIUtils.IEval.byTypeAndRow:
                        evaluation = evaluation + evalByTypeAndRow(soldier);
                        break;
                    case AIUtils.IEval.byType:
                        evaluation = evaluation + evalByType(soldier);
                        break;
                }
                
            }
            return -evaluation;
        }

        /// <summary>
        /// evaluate soldier based on its color
        /// </summary>
        private int evalByType(Cell soldier)
        {
            int score = 10;
            if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
            {
                return IsDarkSoldiers ? score : -score;
            }
            else if (soldier.Piece == CannonUtils.ISoldiers.light_soldier)
            {
                return IsDarkSoldiers ? -score : score;
            }
            else { return score; }
        }

        /// <summary>
        /// evaluate soldier based on its current row distance from enemy town
        /// </summary>
        private int evalByTypeAndRow(Cell soldier)
        {
            int w_2 = 100; // pieces that are 2 rows away from enemy town
            int w_4 = 70; // pieces that are 4 rows away from enemy town
            int w_6 = 50; // pieces that are 6 rows away from enemy town
            int w_9 = 10; // pieces that are 8 to 10 rows away from enemy town

            int score = 0;
            if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
            {
                if(soldier.Row > 6) { score = w_2; }
                else if (soldier.Row > 4) { score = w_4; }
                else if (soldier.Row > 2) { score = w_6; }
                else { score = w_9; }
                return IsDarkSoldiers ? score : -score;
            }
            else if (soldier.Piece == CannonUtils.ISoldiers.light_soldier)
            {
                if (soldier.Row < 3) { score = w_2; }
                else if (soldier.Row < 5) { score = w_4; }
                else if (soldier.Row < 7) { score = w_6; }
                else { score = w_9; }

                return IsDarkSoldiers ? -score : score;
            }
            else
            {
                return w_9;
            } 
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
            // leaf node
            if (depth == 0) { return new int[] { Evaluate(s), 0 }; }
            // terminal node - TODO
            else if (s.TerminalState != CannonUtils.INode.leaf) 
            {
                return new int[] { terminalNodeScore(s.TerminalState), 0 };
            }

            // We could not cut-off so we need to investigate deeper

            int bestValue = -100000000;
            int bestMove = 0;
            int result = bestValue;
            //List<int> child_list = Enumerable.Range(0, s.LegalMoves.Count).ToList();

            if (n.Depth != -1)
            {
                // if the TT does not give a cutoff, we play the best move as first
                bestValue = n.Score;
                bestMove = n.BestMove;

                // Do move ordering with child list
                // First iteration with bestMove
                // If type-1 error ignore it
                try { result = -AlphaBetaWithTT(s.Successor(bestMove), -beta, -alpha, depth - 1)[0]; }
                catch (Exception) { }
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
                                //if (depth == Depth)
                                //{
                                //    CannonUtils.printMove(s.LegalMoves[child], child);
                                //    Console.WriteLine("                                             value = " + result);
                                //}
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
                    result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1)[0];
                    if (result > bestValue)
                    {
                        bestValue = result;
                        bestMove = child;
                        //if (depth == Depth)
                        //{
                        //    CannonUtils.printMove(s.LegalMoves[child], child);
                        //    Console.WriteLine("                                             value = " + result);
                        //}
                    }
                    if (bestValue > alpha) { alpha = bestValue; }
                    if (bestValue >= beta)
                    {
                        prunnings++;
                        break;
                    }   
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

        private int terminalNodeScore(CannonUtils.INode nodeType)
        {
            int score = 1000000;
            if ((nodeType == CannonUtils.INode.dark_wins && IsDarkSoldiers) ||
                (nodeType == CannonUtils.INode.light_wins && !IsDarkSoldiers))
            {
                // you win
                return -score;
            }
            else if ((nodeType == CannonUtils.INode.light_wins && IsDarkSoldiers) || 
                (nodeType == CannonUtils.INode.dark_wins && !IsDarkSoldiers))
            {
                // you loose
                return score;
            }
            else { return score; }
        }

        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.Friend);
            myBoard.AddTown(4, myBoard.Friend);
        }
    }
}
