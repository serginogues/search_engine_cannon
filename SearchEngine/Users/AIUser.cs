﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using CannonModel;

namespace SearchEngine
{
    public class AIUser
    {
        public SearchEngine searchEngine { get; set; }
        public int Depth = 6;

        public void MakeMove(BoardState s)
        {
            Node root = new Node(s);
            searchEngine = new SearchEngine(root);
            int OptimalValue = IterativeDeepening(root);
        }

        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.Friend);
            myBoard.AddTown(4, myBoard.Friend);
        }

        private int IterativeDeepening(Node s)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int optimalValue = 0;
            // Iterative-deepening
            for (int d = 1; d <= Depth; d++)
            {
                // Before searching to depth d-1, order the moves at the root based on the scores returned from depth d
                // AlphaBetaWith(s, -inf, inf, depth)
                optimalValue = searchEngine.AlphaBetaWithTT(s, -1000000, 100000, d);

                var elapsed = stopWatch.ElapsedMilliseconds;
                Console.WriteLine("Depth = " + d + ", Nodes Evaluated = " + Node.NodesEvaluated +", Prunnings = "+ searchEngine.Prunnings+" in " + elapsed.ToString() + "[ms]");
            }
            return optimalValue;
        }
    }
}
