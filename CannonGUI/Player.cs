using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CannonGUI;
using CannonModel;
using SearchEngine;
namespace CannonGUI
{
    public class Player
    {
        private readonly AISearchEngine searchEngine;

        public Player(AISearchEngine ai)
        {
            searchEngine = ai;
        }

        public BoardState Move(bool isAI, BoardState root)
        {
            if (isAI)
            {
                return searchEngine.Search(root);
            }
            else
            {
                return manualMove(root);
            }
        }

        public BoardState addTown(bool random, BoardState root, Random rand)
        {
            CannonUtils.printBoard(root, false);
            if (random)
            {
                root.AddTown(rand.Next(1, 9), root.friendSoldier);
            }
            else
            {
                bool isTown = true;
                while (isTown)
                {
                    try
                    {
                        Console.WriteLine();
                        Console.WriteLine("Write a column number to place Town:");
                        int column = readActionId();
                        if (column > 0 && column < 10)
                        {
                            root.AddTown(column, root.friendSoldier);
                            isTown = false;
                        }
                        else { Console.WriteLine("Error. Try Again.");  }
                    }
                    catch (Exception) 
                    {
                        Console.WriteLine("Error. Try Again.");
                        isTown = true; 
                    }
                }
            }
            return root;
        }

        #region manual user
        public BoardState manualMove(BoardState s)
        {
            Console.WriteLine();
            Console.WriteLine("MANUAL USER TURN");
            CannonUtils.printNextPlayer(s);
            CannonUtils.printBoard(s);

            CannonUtils.printLegalMoves(s.legalMoves.Where(x => x.moveType == CannonUtils.eMoves.shootCannon).ToList());
            Console.WriteLine();
            Console.WriteLine("- Choose a soldier (if you want to SHOOT, you can do it after chosing a soldier):");
            int id = readActionId();

            int count = 0;
            int cell = 0;
            for (int row = 10 - 1; row >= 0; row--)
            {
                for (int column = 0; column < 10; column++)
                {
                    int boardIndex = row * 10 + column;
                    CannonUtils.eSoldiers piece = s.Board[boardIndex];
                    if (CannonUtils.IsOdd(s.turnCounter))
                    {
                        // Light soldiers turn (p2)
                        if (piece == CannonUtils.eSoldiers.light_soldier)
                        {
                            if (id == count)
                            {
                                CannonUtils.printBoardWithMoves(s, boardIndex);
                                cell = boardIndex;
                            }
                            count++;
                        }
                    }
                    else
                    {
                        if (piece == CannonUtils.eSoldiers.dark_soldier)
                        {
                            if (id == count)
                            {
                                CannonUtils.printBoardWithMoves(s, boardIndex);
                                cell = boardIndex;
                            }
                            count++;
                        }
                    }
                }
            }
            List<Move> moves = s.legalMoves.Where(x => x.startIndex == cell || x.moveType == CannonUtils.eMoves.shootCannon).ToList();
            CannonUtils.printLegalMoves(moves);

            Console.WriteLine("Choose a move");
            id = readActionId();
            id = s.legalMoves.IndexOf(moves.ElementAt(id));
            BoardState newB = s.Successor(id);
            CannonUtils.printBoard(newB);

            return newB;

        }
        private int readActionId()
        {
            int num = int.Parse(Console.ReadLine());
            return num;
        }
        #endregion
    }
}
