using System;
using CannonModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

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
    public class SearchEngine
    {
        /// <summary>
        /// Transposition table
        /// </summary>
        public TranspositionTable myTT { get; set; }
        public int nodesEvaluated { get; set; }
        public int prunnings { get; set; }
        public int type1e { get; set; }

        public SearchEngine(BoardState root) 
        {
            myTT = new TranspositionTable(root);
            nodesEvaluated = 0;
            prunnings = 0;
            type1e = 0;
        }

        /// <summary>
        /// Decision algorithm to find the best move given current state of the board
        /// Call: AlphaBetaWithTT(s, -inf, inf, depth)
        /// </summary>
        public int AlphaBetaWithTT(BoardState s, int alpha, int beta, int depth)
        {
            // save original alpha value
            int olda = alpha;
            nodesEvaluated++;

            // Transposition-table lookup
            Entry n = myTT.TableLookup(s);

            if (n.Depth >= depth)
            {
                // [n] is deeper than current depth or the same (means it is more interesting than current depth)

                if (n.Flag == AIUtils.ITTEntryFlag.exact_value)  { return n.Score;  }
                else if (n.Flag == AIUtils.ITTEntryFlag.lower_bound) { alpha = n.Score > alpha ? n.Score : alpha; }
                else if (n.Flag == AIUtils.ITTEntryFlag.upper_bound) { beta = n.Score < beta ? n.Score : beta; }
                if (alpha >= beta) 
                {
                    prunnings++;
                    return n.Score; 
                }                
            }

            // Check if terminal node
            if (depth == 0 || s.TerminalState()) { return s.Evaluate(); }

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
                    //child_list.RemoveAt(bestMove);
                    //child_list.Insert(0, bestMove);
                    result = -AlphaBetaWithTT(s.Successor(bestMove), -beta, -alpha, depth - 1);
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
                            result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1);
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
                    int result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1);
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

            return bestValue;           
        }

        public int AlphaBeta(BoardState s, int alpha, int beta, int depth)
        {

            // leaf node?
            if (depth == 0 || s.TerminalState())
            {
                return s.Evaluate();
            }

            int bestValue = -100000000;
            int bestMove = 0;
            for (int child = 0; child < s.LegalMoves.Count; child++)
            {
                int result = -AlphaBeta(s.Successor(child), -beta, -alpha, depth - 1);
                if (result > bestValue)
                {
                    bestValue = result;
                    bestMove = child;
                }
                if (bestValue > alpha) { alpha = bestValue; }
                if (bestValue >= beta) { break; }
            }
            return bestValue;
        }
    }
}
