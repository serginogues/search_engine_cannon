using System;
using System.Collections.Generic;
using System.Text;
using SearchEngine;

namespace CannonModel
{
    public class ManualUser
    {
        public void SetTowns(BoardState myBoard, bool auto = true)
        {
            if (auto)
            {
                myBoard.AddTown(5, myBoard.Friend);
                myBoard.AddTown(4, myBoard.Friend);
            }
            else
            {
                Console.WriteLine("On their first turns both players put a special piece called 'Town'");
                Console.WriteLine("anywhere on the rows closest to them (1st for the Dark player and 10th for the Light player) excluding corners:");

                // p1
                myBoard.printNextPlayer();
                Console.WriteLine(" - Write COLUMN number (i.e: '4' means column E):");
                int col = int.Parse(Console.ReadLine());
                myBoard.AddTown(col, myBoard.Friend);

                // p2
                myBoard.printNextPlayer();
                Console.WriteLine(" - Write COLUMN number (i.e: '4' means column E):");
                col = int.Parse(Console.ReadLine());
                myBoard.AddTown(col, myBoard.Friend);

            }
        }

        public void MakeMove(BoardState myBoard)
        {

            Console.Clear();
            myBoard.printNextPlayer();
            myBoard.printLegalMoves();
            myBoard.printBoard(false);
            Console.WriteLine();
            Console.WriteLine("Write the number of the action to execute");
            int id = readActionId();
            myBoard.ExecuteMove(myBoard.LegalMoves[id]);
        }
        private int readActionId()
        {
            int num = int.Parse(Console.ReadLine());
            return num;
        }
    }
}
