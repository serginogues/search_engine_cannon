using CannonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchEngine;

namespace CannonBoardConsole
{
    public class MoveManager
    {
        private readonly AISearchEngine searchEngine;

        public MoveManager(AISearchEngine ai)
        {
            searchEngine = ai;
        }

        public BoardState Move(bool isAI, BoardState root)
        {
            if (isAI)
            {
                return searchEngine.search(root);
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
                Console.WriteLine();
                Console.WriteLine("Write a column number to place Town:");
                int column = readActionId();
                root.AddTown(column, root.friendSoldier);
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


            //string num = Console.ReadLine();
            //string[] num_split = num.Split('-');
            //List<string> columnChar = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

            ////// start square
            //char[] startS = num_split[0].ToCharArray();
            //string col_ = startS[0].ToString();
            //int col0 = columnChar.IndexOf(col_);
            //int row0 = int.Parse(startS[1].ToString());
            //int boardIndex = row0 * 10 + col0;
            //CannonUtils.ISoldiers s1 = s.Board[boardIndex];

            //// target square
            //col_ = startS[1].ToString();
            //col0 = columnChar.IndexOf(col_);
            //row0 = int.Parse(startS[1].ToString());
            //int boardIndex2 = row0 * 10 + col0;
            //CannonUtils.ISoldiers s2 = s.Board[boardIndex2];

            //if(s2 == CannonUtils.ISoldiers.empty)
            //{
            //    if()
            //}

            //if(s2 == s.enemySoldier && AIUtils.ChebyshevDistance(s1,s2) > 3)

            //int id = s.legalMoves.IndexOf(x => x.);


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
