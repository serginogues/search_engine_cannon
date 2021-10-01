using System;
using System.Collections.Generic;
using System.Text;

namespace CannonModel
{
    public static class ManualUser
    {
        public static void SetManualTowns(BoardState myBoard, bool auto = true)
        {
            if (auto)
            {
                myBoard.AddTown(5, myBoard.CurrentFriend);
                myBoard.AddTown(4, myBoard.CurrentFriend);
            }
            else
            {
                Console.WriteLine("On their first turns both players put a special piece called 'Town'");
                Console.WriteLine("anywhere on the rows closest to them (1st for the Dark player and 10th for the Light player) excluding corners:");

                // p1
                myBoard.printNextPlayer();
                Console.WriteLine(" - Write COLUMN number (i.e: '4' means column E):");
                int col = int.Parse(Console.ReadLine());
                myBoard.AddTown(col, myBoard.CurrentFriend);

                // p2
                myBoard.printNextPlayer();
                Console.WriteLine(" - Write COLUMN number (i.e: '4' means column E):");
                col = int.Parse(Console.ReadLine());
                myBoard.AddTown(col, myBoard.CurrentFriend);

            }
        }

        public static BoardState MakeMove(BoardState myBoard)
        {
            myBoard.A_SetLegalMoves();

            Console.Clear();
            // A -> GUI
            myBoard.printBoard(false);
            myBoard.printNextPlayer();
            myBoard.printAvailableShoots();
            Console.WriteLine();
            Console.WriteLine("You can either shoot (if there is available shoot cells) OR choose a soldier to move.");
            int[] cell = readRowColumn();

            if (cell[0] >= 0 && cell[0] <= 9 && cell[1] >= 0 && cell[1] <= 9)
            {
                if (myBoard.Grid[cell[0], cell[1]].CurrentValue == myBoard.CurrentFriend)
                {
                    // GUI -> B
                    myBoard.StateManager(CannonUtils.IUserActions.click_soldier, cell[0], cell[1]);

                    // B -> GUI
                    Console.Clear();
                    myBoard.printBoard(true);
                    Console.WriteLine("You have chosen soldier at cell: [" + myBoard.ChosenSoldier.Row + ", " + myBoard.ChosenSoldier.Column + "]");
                    Console.WriteLine("You can either choose another soldier by typing any cell value out of bounds (i.e: [100, 20])");
                    Console.WriteLine("OR");
                    Console.WriteLine("Choose a cell from available moves given current soldier.");
                    cell = readRowColumn();

                    if (cell[0] >= 0 && cell[0] <= 9 && cell[1] >= 0 && cell[1] <= 9)
                    {
                        if (myBoard.Grid[cell[0], cell[1]].Action == CannonUtils.IMoves.step ||
                            myBoard.Grid[cell[0], cell[1]].Action == CannonUtils.IMoves.capture ||
                            myBoard.Grid[cell[0], cell[1]].Action == CannonUtils.IMoves.retreat ||
                            myBoard.Grid[cell[0], cell[1]].Action == CannonUtils.IMoves.slideCannon)
                        {
                            myBoard.StateManager(CannonUtils.IUserActions.click_moves, cell[0], cell[1]);
                        }
                    }
                    else
                    {
                        myBoard.StateManager(CannonUtils.IUserActions.reset_available_moves, 0, 0);
                    }
                }
                else if (myBoard.Grid[cell[0], cell[1]].Action == CannonUtils.IMoves.shootCannon)
                {
                    // GUI -> E
                    myBoard.StateManager(CannonUtils.IUserActions.click_shoot, cell[0], cell[1]);
                }
            }
            return myBoard;
        }
        private static int[] readRowColumn()
        {
            Console.WriteLine(" - Write ROW number:");
            int row = int.Parse(Console.ReadLine());
            Console.WriteLine(" - Write COLUMN number (i.e: '4' means column E):");
            int col = int.Parse(Console.ReadLine());
            return new int[] { row, col };
        }
    }
}
