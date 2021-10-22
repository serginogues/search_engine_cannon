using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CannonModel
{
    public static class CannonUtils
    {
        private static readonly string savePath = @"D:/UM/ISG/search_engine_cannon/board.txt";

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public static bool saveBoard(BoardState s)
        {
            List<string> Freds = s.Board.Select(tb => ((int)tb + "")).ToList();
            //Freds.Insert(0, 25.ToString());
            //System.IO.File.WriteAllText(savePath, 25.ToString());
            System.IO.File.WriteAllLines(savePath, Freds);
            return true;
        }

        public static BoardState readBoard()
        {
            BoardState s = new BoardState();
            s.root_init();
            int square = 0;
            foreach (string line in System.IO.File.ReadLines(savePath))
            {
                s.Board[square] = (eSoldiers)Int32.Parse(line);
                square ++;
            }
            s.generateLegalMoves();

            return s;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/5132758/words-combinations-without-repetition
        /// </summary>
        public static IEnumerable<IEnumerable<T>> GetPermutationsWithoutRepet<T>(IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetPermutationsWithoutRepet(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }
                ++i;
            }
        }
        /// <summary>
        /// {1, 2, 3, 4} and a length of 2
        /// {1,2} {1,3} {1,4} {2,1} {2,3} {2,4} {3,1} {3,2} {3,4} {4,1} {4,2} {4,3}
        /// </summary>
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// {1, 2, 3, 4} and a length of 2
        /// {1,1} {1,2} {1,3} {1,4} {2,1} {2,2} {2,3} {2,4} {3,1} {3,2} {3,3} {3,4} {4,1} {4,2} {4,3} {4,4}
        /// </summary>
        public static IEnumerable<IEnumerable<T>> GetPermutationsWithRepet<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutationsWithRepet(list, length - 1)
                .SelectMany(t => list,
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public enum eSoldiers
        {
            empty,
            dark_soldier,
            light_soldier,
            dark_town,
            light_town
        }

        public enum eMoves
        {
            none,
            slideCannon,
            step,
            retreat,
            capture,
            shootCannon
        }

        public enum eNode 
        {
            dark_wins,
            light_wins,
            leaf
        }

        public static int Size = 10; // Board size
        public static readonly int[] rootDark = {10, 12, 14, 16, 18,
                                                 20, 22, 24, 26, 28,
                                                 30, 32, 34, 36, 38 };

        public static readonly int[] rootLight = {61, 63, 65, 67, 69,
                                                  71, 73, 75, 77, 79,
                                                  81, 83, 85, 87, 89 };

        public static readonly string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

        #region printMethods
        public static void printBoard(BoardState s, bool printNumbers = true)
        {
            Console.WriteLine();
            Console.WriteLine("- Turn counter = " + s.turnCounter);
            Console.WriteLine("- # Dark pieces = " + s.boardCounter.darkPieceList.Count);
            Console.WriteLine("- # Light pieces = " + s.boardCounter.lightPieceList.Count);

            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();

            int counter = 0;
            for (int row = 10 - 1; row >= 0; row--)
            {
                Console.Write(row + 1);
                Console.Write(" ");
                if (row != 9) { Console.Write(" "); }
                for (int column = 0; column < 10; column++)
                {
                    int boardIndex = row * 10 + column;
                    eSoldiers cell = s.Board[boardIndex];

                    if (IsOdd(s.turnCounter))
                    {
                        // Light soldiers turn (p2)
                        if (cell == eSoldiers.dark_soldier) { Console.Write("X"); }
                        else if (cell == eSoldiers.light_soldier) 
                        {
                            if (printNumbers) { Console.Write(counter); counter++; }
                            else { Console.Write("O"); }
                        }
                        else if (cell == eSoldiers.dark_town || cell == eSoldiers.light_town) { Console.Write("T"); }
                        else if (cell == eSoldiers.empty) { Console.Write("·"); }

                    }
                    else
                    {
                        if (cell == eSoldiers.dark_soldier) 
                        {
                            if (printNumbers) { Console.Write(counter); counter++; }
                            else { Console.Write("X"); }
                             
                        }
                        else if (cell == eSoldiers.light_soldier) { Console.Write("O"); }
                        else if (cell == eSoldiers.dark_town || cell == eSoldiers.light_town) { Console.Write("T"); }
                        else if (cell == eSoldiers.empty) { Console.Write("·"); }
                    }
                    if (column != 9) { Console.Write(" - "); }

                }
                Console.WriteLine();
                if (row != 0)
                {
                    Console.Write(" ");
                    for (int j = 0; j < 10; j++)
                    {
                        Console.Write("  ");
                        Console.Write("| ");
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
            Console.WriteLine("Soldiers legend: X = Dark Soldier, O = Light Soldier, T = Town, · = empty cell");

        }

        public static void printBoardWithMoves(BoardState s, int chosenSquare)
        {
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();

            List<Move> moveList = s.legalMoves.Where(x => x.startIndex == chosenSquare).ToList();

            for (int row = 10 - 1; row >= 0; row--)
            {
                Console.Write(row + 1);
                Console.Write(" ");
                if (row != 9) { Console.Write(" "); }
                for (int column = 0; column < 10; column++)
                {
                    int boardIndex = row * 10 + column;
                    eSoldiers cell = s.Board[boardIndex]; 
                    Move mm = moveList.Where(x => x.targetIndex == boardIndex).FirstOrDefault();

                    if (boardIndex == chosenSquare) { Console.Write("S"); }
                    else if (mm != null)
                    {
                        if (mm.moveType == CannonUtils.eMoves.step) { Console.Write("m"); }
                        else if (mm.moveType == CannonUtils.eMoves.retreat) { Console.Write("r"); }
                        else if (mm.moveType == CannonUtils.eMoves.capture) { Console.Write("k"); }
                        else if (mm.moveType == CannonUtils.eMoves.slideCannon) { Console.Write("s"); }
                    }
                    else if (cell == CannonUtils.eSoldiers.dark_soldier) { Console.Write("X"); }
                    else if (cell == CannonUtils.eSoldiers.light_soldier) { Console.Write("O"); }
                    else if (cell == CannonUtils.eSoldiers.dark_town || cell == CannonUtils.eSoldiers.light_town) { Console.Write("T"); }
                    else if (cell == CannonUtils.eSoldiers.empty) { Console.Write("·"); }
                    if (column != 9) { Console.Write(" - "); }
                }
                Console.WriteLine();
                if (row != 0)
                {
                    Console.Write(" ");
                    for (int j = 0; j < 10; j++)
                    {
                        Console.Write("  ");
                        Console.Write("| ");
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
            Console.WriteLine("Soldiers legend: X = Dark Soldier, O = Light Soldier, T = Town, · = empty cell");
            Console.WriteLine("Available Moves legend: m = step, r = retreat, k = capture, s = slide");

        }

        public static void printLegalMoves(List<Move> moves)
        {
            int count = 0;
            Console.WriteLine("List of legal moves: ");
            foreach (Move move in moves)
            {
                printMove(move, count);
                count++;
            }
        }

        public static void printLegalMovesWithScore(List<Move> moves, List<int> scores)
        {
            int count = 0;
            Console.WriteLine("List of best moves: ");
            foreach (Move move in moves)
            {
                printMove(move, count);
                Console.WriteLine(count + " SCORE = " + scores[count]);
                count++;
            }
        }

        public static void printMove(Move move, int count)
        {
            int old_row = move.startIndex / 10;
            int new_row = move.targetIndex / 10;
            int old_column = move.startIndex - old_row * 10;
            int new_column = move.targetIndex - new_row * 10;

            string oldrow = (old_row + 1).ToString();
            string newrow = ((move.targetIndex / 10) + 1).ToString();
            string oldcol = intToColumn(old_column);
            string newcol = intToColumn(new_column);
            string oldc = oldcol + oldrow;
            string newc = newcol + newrow;
            string sentence = oldc + " to " + newc;
            switch (move.moveType)
            {
                case CannonUtils.eMoves.step:
                    Console.WriteLine(count + "     - STEP " + sentence);
                    break;
                case CannonUtils.eMoves.capture:
                    Console.WriteLine(count + "     - CAPTURE " + sentence);
                    break;
                case CannonUtils.eMoves.retreat:
                    Console.WriteLine(count + "     - RETREAT " + sentence);
                    break;
                case CannonUtils.eMoves.slideCannon:
                    Console.WriteLine(count + "     - SLIDE " + sentence);
                    break;
                case CannonUtils.eMoves.shootCannon:
                    Console.WriteLine(count + "     - SHOOT at cell " + newc);
                    break;
            }
        }

        private static string intToColumn(int column)
        {
            switch (column)
            {
                case 0:
                    return "A";
                case 1:
                    return "B";
                case 2:
                    return "C";
                case 3:
                    return "D";
                case 4:
                    return "E";
                case 5:
                    return "F";
                case 6:
                    return "G";
                case 7:
                    return "H";
                case 8:
                    return "I";
                case 9:
                    return "J";
            }
            return "bug";
        }

        public static void printNextPlayer(BoardState s)
        {
            if (CannonUtils.IsOdd(s.turnCounter)) { Console.WriteLine("Light soldiers turn (p2)"); }
            else { Console.WriteLine("Dark soldiers turn (p1)"); }
        }
        #endregion
    }
}
