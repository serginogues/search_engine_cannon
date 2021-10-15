using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// GAME LOGIC:
        /// - Threefold repetition = draw
        /// - Implement Undo move
        /// ITERATIVE DEEPENING: 
        /// - Add null move (iterative deepening) and multi cut
        /// https://www.codeproject.com/Articles/37024/Simple-AI-for-the-Game-of-Breakthrough
        /// </summary>
        static void Main() 
        {
            //PlayAIvsManual();
            PlayAIvsAI();
        }

        /// <summary>
        /// color = 1 -> dark
        /// </summary>
        static void PlayAIvsManual(int color = 1)
        {
            // root node
            myBoard = new BoardState();
            myBoard.root_init();
            CannonUtils.printBoard(myBoard, false);

            // init users
            ManualUser user = new ManualUser();
            myBoard.AddTown(4, myBoard.Friend);
            AISearchEngine ai = new AISearchEngine(AIUtils.IEval.color, -1);
            myBoard.AddTown(4, myBoard.Friend);
            int depth = 5;

            Console.WriteLine("Evaluation root node = "+ ai.Evaluate(myBoard));

            // Play game
            for (int two_turns = 0; two_turns < 1000; two_turns++)
            {
                // dark user leads
                myBoard = color == 1 ? ai.Search(myBoard, depth) : user.MakeMove(myBoard);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                    break;
                }

                // light user follows
                myBoard = color == 1 ? user.MakeMove(myBoard) : ai.Search(myBoard, depth);

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
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // root node
            myBoard = new BoardState();
            myBoard.root_init();
            CannonUtils.printBoard(myBoard, false);

            // Init Players
            AISearchEngine ai_dark = new AISearchEngine(AIUtils.IEval.color, 1);
            AISearchEngine ai_light = new AISearchEngine(AIUtils.IEval.mobility, -1);

            // add towns
            myBoard.AddTown(9, myBoard.Friend);
            myBoard.AddTown(0, myBoard.Friend);

            // search depth
            int depth = 20;

            Console.WriteLine("Evaluation root node = " + ai_dark.Evaluate(myBoard));

            // Play game
            for (int two_turns = 0; two_turns < 1000; two_turns++)
            {
                // dark user leads
                Console.WriteLine("================================= Turn for Dark");
                CannonUtils.printBoard(myBoard, false);
                myBoard = ai_dark.Search(myBoard, depth);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                    break;
                }

                // light user follows
                Console.WriteLine("================================= Turn for Light");
                CannonUtils.printBoard(myBoard, false);
                myBoard = ai_light.Search(myBoard, depth);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                     break;
                }
                var elapsed = stopWatch.ElapsedMilliseconds;
                Console.WriteLine("Time elapsed: " +  elapsed/ 1000 + "[s]");    
                if(elapsed > 60 * 10 * 1000)
                {
                    Console.WriteLine("Draw!!!");
                    break;
                }
            }
            CannonUtils.printBoard(myBoard, false);
            Console.ReadLine();
        }
    }
}
