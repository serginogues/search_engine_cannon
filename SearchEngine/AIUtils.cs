using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;
using System.Linq;
using System.Collections;

namespace SearchEngine
{
    public static class AIUtils
    {
        /// <summary>
        /// Evaluation function type
        /// </summary>
        public enum IEval
        {
            byTypeAndRow,
            byType
        }

        public enum ITTEntryFlag
        {
            exact_value,
            lower_bound,
            upper_bound
        }

        public static int AlphaBeta(BoardState s, int alpha, int beta, int depth)
        {

            // leaf node?
            if (depth == 0 || s.TerminalState != CannonUtils.INode.leaf)
            {
                return 0;
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
