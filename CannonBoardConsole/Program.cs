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
        static BoardState root;

        /// <summary>
        ///  color = 1 -> dark
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
            // ai_dark vs ai_light = 0
            // ai_dark vs manual_light = 1
            // manual_dark vs ai_light = 2 
            bool darkIsAi = true;
            bool lightIsAi = false;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // root node
            root = new BoardState();
            root.root_init();            

            // Dark player
            MoveManager darkP = new MoveManager(new AISearchEngine(AIUtils.IEval.safeMobility, 1));
            MoveManager lightP = new MoveManager(new AISearchEngine(AIUtils.IEval.safeMobility, -1));
            Random rand = new Random();
            darkP.addTown(darkIsAi, root, rand);
            lightP.addTown(lightIsAi, root, rand);

            // Play game
            for (int turn = 0; turn < 1000; turn++)
            {
                BoardState stateCopy = root.DeepCopy();
                stateCopy.generateLegalMoves();
                try
                {
                    Console.WriteLine();
                    if (!CannonUtils.IsOdd(root.turnCounter))
                    {
                        // dark turn
                        Console.WriteLine("================================= Turn for Dark");
                        root = darkP.Move(darkIsAi, root);
                        root = UndoMove() ? stateCopy : root;
                    }
                    else
                    {
                        // light turn
                        Console.WriteLine("================================= Turn for Light");
                        root = lightP.Move(lightIsAi, root);
                        root = UndoMove() ? stateCopy : root;

                    }
                    if (isTerminal(root)) { break; }

                    #region check time
                    var elapsed = stopWatch.ElapsedMilliseconds;
                    Console.WriteLine();
                    Console.WriteLine("Time elapsed: " + elapsed / 1000 + "[s]");
                    //if (elapsed > 60 * 10 * 1000)
                    //{
                    //    Console.WriteLine("Draw!!!");
                    //    break;
                    //}
                    #endregion
                }
                catch (Exception)
                {
                    Console.WriteLine("****************Error****************");
                    Console.WriteLine("Try again");
                }
            }
            CannonUtils.printBoard(root, false);
            Console.ReadLine();
        }

        static bool UndoMove()
        {
            Console.WriteLine("Write 500 if you want to UNDO move");
            int num = int.Parse(Console.ReadLine());
            if(num == 500)
            {
                Console.WriteLine("UNDO");
                return true;
            }
            else
            {
                return false;
            }

            Console.WriteLine();
        }

        static bool isTerminal(BoardState myBoard)
        {
            if (myBoard.terminalFlag != CannonUtils.INode.leaf)
            {
                string winner = myBoard.terminalFlag == CannonUtils.INode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                Console.WriteLine(winner);
                return true;
            }
            else { return false; }
        }
    }
}
