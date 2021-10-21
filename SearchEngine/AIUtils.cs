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
        public static int ChebyshevDistance(int soldier, int town)
        {
            int s_row = soldier / 10;
            int s_column = soldier - s_row * 10;

            int town_row = town / 10;
            int town_column = town - town_row * 10;

            return Math.Abs(s_row - town_row) + Math.Abs(s_column - town_column);
        }

        /// <summary>
        /// Evaluation function type
        /// </summary>
        public enum IEval
        {
            colorAndRow,
            color,
            mobility,
            safeMobility,
            dist2EnemyTown,
            random
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
            if (depth == 0 || s.terminalFlag != CannonUtils.INode.leaf)
            {
                return 0;
            }

            int bestValue = -100000000;
            int bestMove = 0;
            for (int child = 0; child < s.legalMoves.Count; child++)
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
