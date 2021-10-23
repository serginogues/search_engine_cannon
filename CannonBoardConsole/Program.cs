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
        /// </summary>
        static void Main() 
        {
            // ai_dark vs ai_light = 0
            // ai_dark vs manual_light = 1
            // manual_dark vs ai_light = 2 
            bool darkIsAi = true;
            bool lightIsAi = true;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // root node
            root = new BoardState();
            root.root_init();            

            // Dark player
            MoveManager darkP = new MoveManager(new AISearchEngine(AIUtils.eEval.safeMobility, 1));
            MoveManager lightP = new MoveManager(new AISearchEngine(AIUtils.eEval.safeMobility, -1));
            Random rand = new Random();
            darkP.addTown(darkIsAi, root, rand);
            lightP.addTown(lightIsAi, root, rand);
            //root = CannonUtils.readBoard();

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
                        if(!darkIsAi) root = options(stateCopy, root);
                    }
                    else
                    {
                        // light turn
                        Console.WriteLine("================================= Turn for Light");
                        root = lightP.Move(lightIsAi, root);
                        if(!lightIsAi) root = options(stateCopy, root);

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

        static BoardState options(BoardState undoState, BoardState currentState)
        {
            Console.WriteLine("- To Undo write '500'.");
            Console.WriteLine("- To save current board state write '600'.");
            Console.WriteLine("- To load mid match board state write '700'.");
            Console.WriteLine("- To continue press any other button:");
            int num = int.Parse(Console.ReadLine());
            if(num == 500)
            {
                Console.WriteLine("UNDO");
                return undoState;
            }
            else if (num == 600)
            {
                Console.WriteLine("Board saved");
                CannonUtils.saveBoard(currentState);
                return currentState;
            }
            else if (num == 700)
            {
                Console.Clear();
                Console.WriteLine("Board loaded");
                return CannonUtils.readBoard();
            }
            else
            {
                return currentState;
            }
        }

        static bool isTerminal(BoardState myBoard)
        {
            if (myBoard.terminalFlag != CannonUtils.eNode.leaf)
            {
                string winner = myBoard.terminalFlag == CannonUtils.eNode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                Console.WriteLine(winner);
                return true;
            }
            else { return false; }
        }
    }
}
