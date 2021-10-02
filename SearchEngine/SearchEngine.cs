using System;
using CannonModel;
using System.Linq;
using System.Collections.Generic;

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

        public SearchEngine()
        {
            myTT = new TranspositionTable();
            NodesEvaluated = 0;
        }

        public int NodesEvaluated { get; set; }

        public int AlphaBeta(Node s, int depth, int alpha, int beta)
        {
            if (s.TerminalState() || depth == 0) { return s.Evaluate(); }
            int score = -1000;

            foreach (Child child in s.MoveOrderingList)
            {
                int value = -AlphaBeta(s.Successor(child.Id), depth - 1, -beta, -alpha);

                // Update child value
                s.MoveOrderingList.Where(x => x.Id == child.Id).First().Value = value;

                if (value > score) { score = value; }
                if (score > alpha) { alpha = score; }
                if (score >= beta) { break; }
            }

            #region deprecated
            //for (int child = 1; child < s.State.LegalMoves.Count; child++)
            //{
            //    int value = -AlphaBeta(s.Successor(child), depth - 1, -beta, -alpha);

            //    if (value > score) { score = value; }
            //    if (score > alpha) { alpha = score; }
            //    if (score >= beta) { break; }
            //}
            #endregion

            s.MoveOrderingList = s.MoveOrderingList.OrderBy(o => o.Value).ToList();
            return score;
        }

        /// <summary>
        /// Decision algorithm to find the best move given current state of the board
        /// Call: AlphaBetaWithTT(s, -inf, inf, depth)
        /// </summary>
        //public int AlphaBetaWithTT(Node s, int alpha, int beta, int depth)
        //{
        //    // save original alpha value
        //    int olda = alpha;
        //    NodesEvaluated = NodesEvaluated + 1;

        //    // Transposition-table lookup
        //    TTEntry n = myTT.GetEntryFromState(s);

        //    if (n.Depth >= depth)
        //    {
        //        // TTEntry is deeper than current depth -> more interesting
        //        if (n.Flag == AIUtils.ITTEntryFlag.exact_value) 
        //        { 
        //            return n.Value; 
        //        }
        //        else if (n.Flag == AIUtils.ITTEntryFlag.lower_bound) { alpha = n.Value > alpha ? n.Value : alpha; }
        //        else if (n.Flag == AIUtils.ITTEntryFlag.upper_bound) { beta = n.Value < beta ? n.Value : beta; }
        //        if (alpha >= beta) { return n.Value; }
        //    }

        //    if (depth == 0 || AIUtils.TerminalState(s)) 
        //    {
        //        return AIUtils.Evaluate(s);  // leaf node
        //    } 
            
        //    //ToDo
        //    //if (n.Depth != -1)
        //    //{
        //    //    // Investigate the BestMove stored in [n]
        //    //    AIUtils.Result output = AlphaBetaWithTT(s.ChildList[n.BestMove], -beta, -alpha, depth - 1);
        //    //}
        //    // if position is not found, depth will be -1                 
        //    int bestValue = -100000000;
        //    int bestMove = 0;

        //    for (int child = 0; child < s.LegalMoves.Count; child++)
        //    {
        //        int result = -AlphaBetaWithTT(AIUtils.Successor(child, s), -beta, -alpha, depth - 1);

        //        if (result > bestValue)
        //        {
        //            bestValue = result;
        //            bestMove = child;
        //        }
        //        if (bestValue > alpha) { alpha = bestValue; }
        //        if (bestValue >= beta) { break; }
        //    }

        //    // Traditional transposition table storing of bounds
        //    AIUtils.ITTEntryFlag flag = AIUtils.ITTEntryFlag.exact_value;

        //    // Fail-low result implies an upper bound
        //    if (bestValue <= olda) { flag = AIUtils.ITTEntryFlag.upper_bound; }
        //    // Fail-high result implies a lower bound
        //    else if (bestValue >= beta) { flag = AIUtils.ITTEntryFlag.lower_bound; }

        //    // store information in the TT
        //    myTT.Store(s, bestMove, bestValue, flag, depth);

        //    return bestValue;
        //}
    }
}
