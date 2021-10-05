using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CannonModel;

namespace SearchEngine
{
    public class AIUser
    {
        public SearchEngine searchEngine { get; set; }
        public int Depth = 7;

        public void MakeMove(BoardState root)
        {
            searchEngine = new SearchEngine(root);
            int OptimalValue = IterativeDeepening(root);
        }

        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.Friend);
            myBoard.AddTown(4, myBoard.Friend);
        }

        private int IterativeDeepening(BoardState s)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int optimalValue = 0;
            // Iterative-deepening
            for (int d = 1; d <= Depth; d++)
            {
                optimalValue = searchEngine.AlphaBetaWithTT(s, -1000000, 100000, d);
                //searchEngine.myTT.ResetAllAncientFlags();

                var elapsed = stopWatch.ElapsedMilliseconds;
                Console.WriteLine("Depth = " + d + 
                    ", Nodes Evaluated = " + searchEngine.nodesEvaluated +
                    " in " + elapsed.ToString() + "[ms]"+ 
                    ", prunnings = "+searchEngine.prunnings+ 
                    ", type-1 errors = "+ searchEngine.type1e+
                    ", TT entries = "+ searchEngine.myTT.TT.ToList().Where(x => x != null).ToList().Count);
            }
            return optimalValue;
        }
    }
}
