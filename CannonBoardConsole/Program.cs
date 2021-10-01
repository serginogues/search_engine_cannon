using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CannonModel;

namespace CannonBoardConsole
{
    class Program
    {
        static BoardState myBoard;

        static void Main() { PlayGame(); }

        static void PlayGame(bool manual=true)
        {
            myBoard = new BoardState();

            if (manual)
            {
                // Set Towns
                ManualUser.SetManualTowns(myBoard);

                // Play game
                for (int i = 0; i < 100; i++)
                {
                    myBoard = ManualUser.MakeMove(myBoard);
                }
            }
        }
    }
}
