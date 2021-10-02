using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;
using System.Linq;

namespace SearchEngine
{
    public static class AIUtils
    {
        public enum IType
        {
            MAX,
            MIN
        }

        public enum ITTEntryFlag
        {
            exact_value,
            lower_bound,
            upper_bound
        }

        //static int MiniMax(Node s, int depth_, AIUtils.IType type_)
        //{
        //    if (TerminalState(s) || depth_ == 0) { return Evaluate(s); }
        //    int score = type_ == AIUtils.IType.MAX ? -1000000 : 1000000;

        //    if (type_ == AIUtils.IType.MAX)
        //    {
        //        for (int child = 1; child < s.ChildList.Count; child++)
        //        {
        //            int value = MiniMax(s.ChildList[child], depth_ - 1, AIUtils.IType.MIN);
        //            if (value > score) { score = value; }
        //        }
        //    }
        //    else
        //    {
        //        for (int child = 1; child < s.ChildList.Count; child++)
        //        {
        //            int value = MiniMax(s.ChildList[child], depth_ - 1, AIUtils.IType.MAX);
        //            if (value < score) { score = value; }
        //        }
        //    }
        //    return score;
        //}
    }
}
