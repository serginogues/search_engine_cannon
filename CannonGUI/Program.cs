using CannonModel;
using SearchEngine;
using System;
using System.Diagnostics;

namespace CannonGUI
{
    class Program
    {
        static BoardState Root;
        static Player DarkPlayer;
        static Player LightPlayer;
        static int searchTimeMiliSec;
        static bool darkIsAi;
        static bool lightIsAi;
        static bool isMidGame;

        /// <summary>
        ///  color = 1 -> dark
        /// </summary>
        static void Main()
        {
            bool isProgram = true;
            while (isProgram)
            {
                Console.ResetColor();

                // Board
                Console.Clear();
                boardMenu();
                Console.Clear();
                CannonUtils.printBoard(Root, false);

                playerMenu();

                // search time allowed
                secondsAllowedMenu();
                // players
                DarkPlayer = new Player(new AISearchEngine(AIUtils.eEval.safeMobility, 1, searchTimeMiliSec));
                LightPlayer = new Player(new AISearchEngine(AIUtils.eEval.safeMobility, -1, searchTimeMiliSec));

                if (undoMenu()) continue;

                Console.Clear();

                // Town placement
                if (!isMidGame)
                {
                    Random rand = new Random();
                    DarkPlayer.addTown(darkIsAi, Root, rand);
                    LightPlayer.addTown(lightIsAi, Root, rand);
                }

                // Play game
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                bool isGame = true;
                while (isGame)
                {
                    isGame = playTurn(stopWatch);
                }
            }
        }

        static void boardMenu()
        {
            // root node
            BoardState newGame = new BoardState();
            newGame.root_init();

            BoardState midGame = CannonUtils.midGameBoard();

            bool isMenu = true;
            while (isMenu)
            {
                try
                {
                    // choose board
                    Console.WriteLine("Welcome to Cannon game.");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" - Type '1' for new game.");
                    Console.WriteLine(" - Type '2' for mid-position game.");
                    Console.ResetColor();

                    int num = int.Parse(Console.ReadLine());
                    if (num == 1)
                    {
                        Root = newGame;
                        isMidGame = false;
                        Console.Clear();
                        Console.WriteLine("Board loaded");
                        isMenu = false;
                    }
                    else if (num == 2)
                    {
                        Root = midGame;
                        isMidGame = true;
                        Console.Clear();
                        Console.WriteLine("Board loaded");
                        isMenu = false;
                    }
                    else
                    {
                        Console.WriteLine("Input Error");
                    }
                }

                catch (Exception)
                {
                    Console.WriteLine("Input Error");
                }
            }
        }

        static bool playTurn(Stopwatch stopWatch)
        {
            BoardState stateCopy = Root.DeepCopy();
            stateCopy.generateLegalMoves();
            bool isGame = true;
            try
            {
                Console.WriteLine();
                if (!CannonUtils.IsOdd(Root.turnCounter))
                {
                    // dark turn
                    Console.WriteLine("================================= Turn for Dark");
                    Root = DarkPlayer.Move(darkIsAi, Root);
                    if (!darkIsAi) isGame = midGameOptions(stateCopy);
                }
                else
                {
                    // light turn
                    Console.WriteLine("================================= Turn for Light");
                    Root = LightPlayer.Move(lightIsAi, Root);
                    if (!lightIsAi) isGame = midGameOptions(stateCopy);

                }
                if (isTerminal(Root)) { return false; }
                else
                {
                    if (isGame)
                    {
                        var elapsed = stopWatch.ElapsedMilliseconds;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine();
                        Console.WriteLine("Time elapsed: " + elapsed / (1000 * 60) + "[min]");
                        if (elapsed > (60 * 10 * 1000 * 2))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Time limit: Draw!!!");
                            Console.ResetColor();
                            Console.ReadLine();
                            return false;
                        }
                        Console.ResetColor();
                        return isGame;
                    }
                    else return isGame;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("****************Error****************");
                Console.WriteLine("Try again");
            }

            return true;
        }

        static void playerMenu()
        {
            bool isMenu = true;
            while (isMenu)
            {
                try
                {
                    // choose board
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    Console.WriteLine("Choose type of game:");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("- Type '0' to see a game AI vs AI.");
                    Console.WriteLine("- Type '1' to play with dark pieces vs AI.");
                    Console.WriteLine("- Type '2' to play with light pieces vs AI.");
                    Console.WriteLine("- Type '3' to play manually with both dark and light pieces.");
                    Console.ResetColor();
                    int num = int.Parse(Console.ReadLine());
                    isMenu = false;
                    if (num == 0)
                    {
                        darkIsAi = true;
                        lightIsAi = true;
                    }
                    else if(num == 1)
                    {
                        darkIsAi = false;
                        lightIsAi = true;
                    }
                    else if(num == 2)
                    {
                        darkIsAi = true;
                        lightIsAi = false;
                    }
                    else if (num == 3)
                    {
                        darkIsAi = false;
                        lightIsAi = false;
                    }
                    else
                    {
                        Console.WriteLine("Input Error");
                        isMenu = true;
                    }
                }

                catch (Exception)
                {
                    Console.WriteLine("Input Error");
                }
            }
        }

        static void secondsAllowedMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            bool isMenu = true;
            while (isMenu)
            {
                try
                {
                    // choose board
                    Console.WriteLine();
                    Console.WriteLine("Type the number of seconds the AI is allowed to search (once reached this threshold, it will perform a last search with depth++)");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("(i.e. '2')");
                    Console.ResetColor();
                    isMenu = false;
                    int num = int.Parse(Console.ReadLine());
                    if (num > 0 && num < 1000)
                    {
                        searchTimeMiliSec = num * 1000;
                    }
                    else
                    {
                        isMenu = true;
                        Console.WriteLine("Number out of bounds");
                    }
                }

                catch (Exception)
                {
                    Console.WriteLine("Input Error");
                }
            }
        }

        static bool undoMenu()
        {
            Console.WriteLine("- To Undo type '500'.");
            int num = int.Parse(Console.ReadLine());
            if (num == 500) return true;
            else return false;
        }

        static bool midGameOptions(BoardState undoState)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("- To Undo type '500'.");
            Console.WriteLine("- To go back to main menu type '600'.");
            Console.WriteLine("- To save current position type '700'.");
            Console.WriteLine("- To continue type any other key:");
            Console.ResetColor();
            try
            {
                int num = int.Parse(Console.ReadLine());
                if (num == 500)
                {
                    Console.WriteLine("UNDO");
                    Root = undoState;
                    return true;
                }
                else if (num == 600)
                {
                    Console.Clear();
                    return false;
                }
                else if (num == 700)
                {
                    CannonUtils.saveBoard(Root);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception) { return true; }
        }

        static bool isTerminal(BoardState myBoard)
        {
            if (myBoard.terminalFlag != CannonUtils.eNode.leaf)
            {
                CannonUtils.printBoard(Root, false);
                Console.ForegroundColor = ConsoleColor.Red;
                string winner = myBoard.terminalFlag == CannonUtils.eNode.dark_wins ? "Dark wins!!!" : "Light wins !!!";
                Console.WriteLine(winner);
                Console.ResetColor();
                return true;
            }
            else { return false; }
        }

        static void showAllColors()
        {
            // Demonstrate all colors and backgrounds.
            Type type = typeof(ConsoleColor);
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var name in Enum.GetNames(type))
            {
                Console.BackgroundColor = (ConsoleColor)Enum.Parse(type, name);
                Console.WriteLine(name);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            foreach (var name in Enum.GetNames(type))
            {
                Console.ForegroundColor = (ConsoleColor)Enum.Parse(type, name);
                Console.WriteLine(name);
            }
            Console.ReadLine();
        }
    }
}
