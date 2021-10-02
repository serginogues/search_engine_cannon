using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public class AIUser
    {
        public SearchEngine searchEngine { get; set; }
        public int Depth = 6;
        public Node Root { get; set; }
        public AIUser()
        {
            searchEngine = new SearchEngine();
        }
        public void MakeMove(BoardState myBoard)
        {
            Root = new Node(myBoard);
            int OptimalValue = IterativeDeepening(Root);
        }

        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            myBoard.AddTown(5, myBoard.Friend);
            myBoard.AddTown(4, myBoard.Friend);
        }

        private int IterativeDeepening(Node s)
        {
            int optimalValue = 0;
            // Iterative-deepening
            for (int d = Depth; d >= 0; d--)
            {
                // Before searching to depth d-1, order the moves at the root based on the scores returned from depth d
                // AlphaBetaWith(s, -inf, inf, depth)
                optimalValue = searchEngine.AlphaBeta(s, -1000000, 100000, d);
            }
            return optimalValue;
        }
    }
}
