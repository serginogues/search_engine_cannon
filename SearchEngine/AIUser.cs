using System;
using CannonModel;
using System.Linq;

namespace SearchEngine
{
    public class AIUser
    {
        TranspositionTable myTT { get; set; }

        public AIUser()
        {
            myTT = new TranspositionTable();
        }

        #region a-b search
        int MiniMax(BoardState s, int depth_, AIUtils.IType type_)
        {
            if (s.TerminalState() || depth_ == 0) { return Evaluate(s); }
            int score = type_ == AIUtils.IType.MAX ? -1000000 : 1000000;
            if (type_ == AIUtils.IType.MAX)
            {
                for (int child = 1; child < s.ChildrenList.Count; child++)
                {
                    int value = MiniMax(s.ChildrenList[child], depth_ - 1, AIUtils.IType.MIN);
                    if (value > score) { score = value; }
                }
            }
            else
            {
                for (int child = 1; child < s.ChildrenList.Count; child++)
                {
                    int value = MiniMax(s.ChildrenList[child], depth_ - 1, AIUtils.IType.MAX);
                    if (value < score) { score = value; }
                }
            }
            return score;
        }

        int AlphaBeta(BoardState s, int depth, int alpha, int beta)
        {
            if (s.TerminalState() || depth == 0) { return Evaluate(s); }
            int score = -100000000;
            for (int child = 1; child < s.ChildrenList.Count; child++)
            {
                int value = -AlphaBeta(s.ChildrenList[child], depth - 1, -beta, -alpha);
                if(value > score) { score = value; }
                if(score > alpha) { alpha = score; }
                if(score >= beta) { break; }
            }
            return score;
        }
        #endregion

        public int AlphaBetaWithTT(BoardState s, int alpha, int beta, int depth)
        {
            int olda = alpha; // save original alpha value
            TTEntry n = myTT.GetEntryFromState(s); // Transposition-table lookup

            // if position is not found, depth will be -1
            // Retrieved depth [n.Depth] and search depth [depth]
            // n.Depth >= depth means that TTEntry is deeper than current depth 
            // so it is a good idea to investigate the TTEntry as the first one. 
            if (n.Depth >= depth)
            {
                if (n.Flag == AIUtils.ITTEntryFlag.exact_value) { return n.Value; }
                else if(n.Flag == AIUtils.ITTEntryFlag.lower_bound) { alpha = n.Value > alpha ? n.Value : alpha; }
                else if (n.Flag == AIUtils.ITTEntryFlag.upper_bound) { beta = n.Value < beta ? n.Value : beta; }
                if (alpha >= beta) { return n.Value; }
            }
            
            //ToDo:
            // Investigate the BestMove stored in [n]

            if (depth == 0 || s.TerminalState()) { return Evaluate(s); } // leaf node
            int bestValue = -100000000;
            int bestMove = 0;

            for (int child = 0; child < s.ChildrenList.Count; child++)
            {
                s.ChildrenList[child].A_SetLegalMoves();
                int result = -AlphaBetaWithTT(s.ChildrenList[child], -beta, -alpha, depth - 1);
                if (result > bestValue) 
                { 
                    bestValue = result;
                    bestMove = child;
                }
                if (bestValue > alpha) { alpha = bestValue; }
                if (bestValue >= beta) { break; }
            }

            // Traditional transposition table storing of bounds
            AIUtils.ITTEntryFlag flag = AIUtils.ITTEntryFlag.exact_value;

            // Fail-low result implies an upper bound
            if (bestValue <= olda) { flag = AIUtils.ITTEntryFlag.upper_bound; }
            // Fail-high result implies a lower bound
            else if (bestValue >= beta) { flag = AIUtils.ITTEntryFlag.lower_bound; }

            // store information in the TT
            myTT.Store(s, bestMove, bestValue, flag, depth);

            return bestValue;
        }

        int Evaluate(BoardState s)
        {
            Random rnd = new Random();
            return rnd.Next(-200, 200);
        }
    }
}
