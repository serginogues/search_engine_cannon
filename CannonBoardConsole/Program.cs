using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CannonModel;
using SearchEngine;

namespace CannonBoardConsole
{
    class Program
    {
        static BoardState myBoard;

        static void Main() 
        {
            PlayGame();
        }

        /// <param name="AIblack">if true, AI plays for dark pieces</param>
        static void PlayGame(bool aiDark = true)
        {
            // root node
            myBoard = new BoardState();
            myBoard.root_init();
            CannonUtils.printBoard(myBoard);

            // init users
            ManualUser user = new ManualUser();
            myBoard.AddTown(5, myBoard.Friend);
            AISearchEngine ai = new AISearchEngine();
            myBoard.AddTown(4, myBoard.Friend); 

            // Play game
            for (int two_turns = 0; two_turns < 1000; two_turns++)
            {
                // dark user leads
                myBoard = aiDark ? ai.Search(myBoard, aiDark, 5) : user.MakeMove(myBoard);

                // light user follows
                myBoard = aiDark ? user.MakeMove(myBoard) : ai.Search(myBoard, aiDark, 5);
            }
        }

    }
}
