using System;
using System.Collections.Generic;
using System.Linq;
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
            //myBoard.printSoldiers();
            myBoard.printBoard();
            printShoots(myBoard);
            Console.WriteLine();
            Console.WriteLine("Choose a soldier");
            int id = readActionId();
            int count = 0;
            Cell cell = new Cell(0, 0);
            for (int i = 10 - 1; i >= 0; i--)
            {
                for (int j = 0; j < 10; j++)
                {
                    Cell s = myBoard.Grid[i, j];
                    if (CannonUtils.IsOdd(myBoard.TurnCounter))
                    {
                        // Light soldiers turn (p2)
                        if (s.Piece == CannonUtils.ISoldiers.light_soldier)
                        {
                            if (id == count)
                            {
                                myBoard.printBoardWithMoves(s);
                                cell = s;
                            }
                            count++;
                        }
                    }
                    else
                    {
                        if (s.Piece == CannonUtils.ISoldiers.dark_soldier)
                        {
                            if (id == count)
                            {
                                myBoard.printBoardWithMoves(s);
                                cell = s;
                            }
                            count++;
                        }
                    }
                }
            }
            List<Move> moves = myBoard.LegalMoves.Where(x => x.OldCell.Row == cell.Row && x.OldCell.Column == cell.Column).ToList();
            myBoard.printLegalMoves(moves);
            Console.WriteLine("Choose a move");
            id = readActionId();
            id = myBoard.LegalMoves.IndexOf(moves.ElementAt(id));
            myBoard.ExecuteMove(myBoard.LegalMoves[id]);
        }
        private int readActionId()
        {
            int num = int.Parse(Console.ReadLine());
            return num;
        }

        private void printShoots(BoardState myBoard)
        {
            foreach (Move item in myBoard.LegalMoves)
            {
                if(item.Type == CannonUtils.IMoves.shootCannon)
                {
                    Console.WriteLine("Shoot at cell (" + item.NewCell.Row + "," + item.NewCell.Column + ")");
                }
            }
        }
    }
}
