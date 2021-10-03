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
        TranspositionTable myTT { get; set; }
        public int Prunnings { get; set; }

        public SearchEngine(Node root) 
        {
            myTT = new TranspositionTable(root);
            Prunnings = 0;
        }

        /// <summary>
        /// Decision algorithm to find the best move given current state of the board
        /// Call: AlphaBetaWithTT(s, -inf, inf, depth)
        /// </summary>
        public int AlphaBetaWithTT(Node s, int alpha, int beta, int depth)
        {
            // save original alpha value
            int olda = alpha;

            // Transposition-table lookup
            Entry n = myTT.Retrieve(s);

            if (n.Depth >= depth)
            {
                // TTEntry is deeper than current depth -> more interesting
                if (n.Flag == AIUtils.ITTEntryFlag.exact_value)  { return n.Value;  }
                else if (n.Flag == AIUtils.ITTEntryFlag.lower_bound) { alpha = n.Value > alpha ? n.Value : alpha; }
                else if (n.Flag == AIUtils.ITTEntryFlag.upper_bound) { beta = n.Value < beta ? n.Value : beta; }
                if (alpha >= beta) { return n.Value; }
            }

            // leaf node?
            if (depth == 0 || s.TerminalState()) 
            {
                return n.Depth == -1 ? s.Evaluate() : n.Value;
            }

            // if position is not found, depth will be -1
            if (n.Depth != -1)
            {
                // entry found
                int bestValue = n.Value;
                int bestMove = n.BestMove;
                List<int> indexes = new List<int>();
                foreach (int child in n.ChildList)
                {
                    int result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1);
                    indexes.Add(result);
                    if (result > bestValue)
                    {
                        bestValue = result;
                        bestMove = child;
                    }
                    if (bestValue > alpha) { alpha = bestValue; }
                    if (bestValue >= beta) 
                    {
                        Prunnings++;
                        break;
                    }
                }
                indexes = indexes.Select((x, i) => new KeyValuePair<int, int>(x, i)).OrderByDescending(x => x.Key).ToList().Select(x => x.Value).ToList();

                // Traditional transposition table storing of bounds
                AIUtils.ITTEntryFlag flag = AIUtils.ITTEntryFlag.exact_value;

                // Fail-low result implies an upper bound
                if (bestValue <= olda) { flag = AIUtils.ITTEntryFlag.upper_bound; }
                // Fail-high result implies a lower bound
                else if (bestValue >= beta) { flag = AIUtils.ITTEntryFlag.lower_bound; }

                // store information in the TT
                myTT.Update(bestMove, bestValue, flag, depth, n.EntryKey, indexes);

                return bestValue;

            }
            else
            {
                // entry not found
                int bestValue = -100000000;
                int bestMove = 0;
                List<int> indexes = new List<int>();
                for (int child = 0; child < s.State.LegalMoves.Count; child++)
                {
                    int result = -AlphaBetaWithTT(s.Successor(child), -beta, -alpha, depth - 1);
                    indexes.Add(result);
                    if (result > bestValue)
                    {
                        bestValue = result;
                        bestMove = child;
                    }
                    if (bestValue > alpha) { alpha = bestValue; }
                    if (bestValue >= beta) 
                    {
                        Prunnings++;
                        break; 
                    }
                }
                indexes = indexes.Select((x, i) => new KeyValuePair<int, int>(x, i)).OrderByDescending(x => x.Key).ToList().Select(x => x.Value).ToList();

                // Traditional transposition table storing of bounds
                AIUtils.ITTEntryFlag flag = AIUtils.ITTEntryFlag.exact_value;

                // Fail-low result implies an upper bound
                if (bestValue <= olda) { flag = AIUtils.ITTEntryFlag.upper_bound; }
                // Fail-high result implies a lower bound
                else if (bestValue >= beta) { flag = AIUtils.ITTEntryFlag.lower_bound; }

                long key = myTT.Store(s, bestMove, bestValue, flag, depth, indexes);
                //Tree.Add(new NodeKeyPair() { Key = key, node = s });

                return bestValue;
            }            
        }

        public int AlphaBeta(Node s, int alpha, int beta, int depth)
        {

            // leaf node?
            if (depth == 0 || s.TerminalState())
            {
                return s.Evaluate();
            }

            int bestValue = -100000000;
            int bestMove = 0;
            for (int child = 0; child < s.State.LegalMoves.Count; child++)
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
