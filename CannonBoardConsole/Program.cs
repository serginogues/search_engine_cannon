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

        /// <summary>
        /// TODO:
        /// - Add null move (iterative deepening) and multi cut
        /// - Evitar que la AI faci 2 vegades seguides el mateix moviment ([ai moves], enemy moves, ai moves back, enemy moves, [ai moves again])
        /// </summary>
        static void Main() 
        {
            //PlayAIvsManual();
            PlayAIvsAI();
        }

        /// <param name="AIblack">if true, AI plays for dark pieces</param>
        static void PlayAIvsManual(bool aiDark = false)
        {
            // root node
            myBoard = new BoardState();
            myBoard.root_init();
            CannonUtils.printBoard(myBoard, false);

            // init users
            ManualUser user = new ManualUser();
            myBoard.AddTown(4, myBoard.Friend);
            AISearchEngine ai = new AISearchEngine();
            myBoard.AddTown(4, myBoard.Friend);
            int depth = 6;

            Console.WriteLine("Evaluation root node = "+ ai.Evaluate(myBoard));

            // Play game
            for (int two_turns = 0; two_turns < 1000; two_turns++)
            {
                // dark user leads
                myBoard = aiDark ? ai.Search(myBoard, aiDark, depth, AIUtils.IEval.byType) : user.MakeMove(myBoard);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                    break;
                }

                // light user follows
                myBoard = aiDark ? user.MakeMove(myBoard) : ai.Search(myBoard, aiDark, depth, AIUtils.IEval.byType);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                    break;
                }
            }
        }

        /// <summary>
        /// To compare evaluation functions and search times
        /// </summary>
        static void PlayAIvsAI()
        {
            // root node
            myBoard = new BoardState();
            myBoard.root_init();
            CannonUtils.printBoard(myBoard, false);

            // init users
            AISearchEngine ai_light = new AISearchEngine();
            myBoard.AddTown(4, myBoard.Friend);
            AISearchEngine ai_dark = new AISearchEngine();
            myBoard.AddTown(4, myBoard.Friend);
            int depth = 6;

            Console.WriteLine("Evaluation root node = " + ai_dark.Evaluate(myBoard));

            // Play game
            for (int two_turns = 0; two_turns < 1000; two_turns++)
            {
                // dark user leads
                myBoard = ai_dark.Search(myBoard, true, depth, AIUtils.IEval.byType);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                    break;
                }

                // light user follows
                myBoard = ai_light.Search(myBoard, false, depth, AIUtils.IEval.byTypeAndRow);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                     break;
                }

                CannonUtils.printBoard(myBoard, false);
            }
            CannonUtils.printBoard(myBoard, false);
            Console.ReadLine();
        }
    }
}
