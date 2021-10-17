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
        ///  - your  program  has  to  finish  the  whole  game  within  10minutes own time.
        ///  - OPPONENT (MANUAL USER): Moves of the opponent are entered manually  by  the  operator.
        ///  - YOU (AI): Moves  by  your  program  are  communicated  to  your  opponent.
        /// GAME LOGIC:
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

        public const int Depth = 30;

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
            AISearchEngine ai = new AISearchEngine(AIUtils.IEval.dist2EnemyTown, 1);
            myBoard.AddTown(4, myBoard.Friend);

            Console.WriteLine("Evaluation root node = "+ ai.Evaluate(myBoard));

            // Play game
            for (int two_turns = 0; two_turns < 1000; two_turns++)
            {
                // dark user leads
                myBoard = color == 1 ? ai.Search(myBoard, Depth) : user.MakeMove(myBoard);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                    break;
                }

                // light user follows
                myBoard = color == 1 ? user.MakeMove(myBoard) : ai.Search(myBoard, Depth);

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
            AISearchEngine ai_dark = new AISearchEngine(AIUtils.IEval.dist2EnemyTown, 1);
            AISearchEngine ai_light = new AISearchEngine(AIUtils.IEval.safeMobility, -1);

            // add towns
            myBoard.AddTown(9, myBoard.Friend);
            myBoard.AddTown(0, myBoard.Friend);

            Console.WriteLine("Evaluation root node = " + ai_dark.Evaluate(myBoard));

            // Play game
            for (int two_turns = 0; two_turns < 1000; two_turns++)
            {
                // dark user leads
                CannonUtils.printBoard(myBoard, false);
                Console.WriteLine("================================= Turn for Dark");
                myBoard = ai_dark.Search(myBoard, Depth);

                if (myBoard.TerminalState != CannonUtils.INode.leaf)
                {
                    string winner = myBoard.TerminalState == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                    Console.WriteLine(winner);
                    break;
                }

                // light user follows
                CannonUtils.printBoard(myBoard, false);
                Console.WriteLine("================================= Turn for Light");
                myBoard = ai_light.Search(myBoard, Depth);

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
