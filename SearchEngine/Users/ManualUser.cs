using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEngine;

namespace CannonModel
{
    public class ManualUser
    {
        public BoardState MakeMove(BoardState myBoard)
        {
            Console.WriteLine();
            Console.WriteLine("MANUAL USER TURN");
            CannonUtils.printNextPlayer(myBoard);
            //myBoard.printSoldiers();
            CannonUtils.printBoard(myBoard);
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
                                CannonUtils.printBoardWithMoves(myBoard, s);
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
                                CannonUtils.printBoardWithMoves(myBoard, s);
                                cell = s;
                            }
                            count++;
                        }
                    }
                }
            }
            List<Move> moves = myBoard.LegalMoves.Where(x => x.OldCell.Row == cell.Row && x.OldCell.Column == cell.Column).ToList();
            CannonUtils.printLegalMoves(moves);

            Console.WriteLine("Choose a move");
            id = readActionId();
            id = myBoard.LegalMoves.IndexOf(moves.ElementAt(id));
            BoardState newB = myBoard.Successor(id);
            CannonUtils.printBoard(newB);

            return newB;
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
