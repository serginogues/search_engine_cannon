using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CannonModel
{
    public static class CannonUtils
    {

        public static bool movesAreEqual(Move a, Move b)
        {
            if(a.Type == b.Type && 
                a.OldCell.myRow == b.OldCell.myRow && a.OldCell.myColumn == b.OldCell.myColumn &&
                a.NewCell.myRow == b.NewCell.myRow && a.NewCell.myColumn == b.NewCell.myColumn)
            {
                return true;
            }
            else { return false; }
        }
        public static int ChebyshevDistance(Cell soldier, Cell town)
        {
            return Math.Abs(soldier.myRow - soldier.myRow) + Math.Abs(soldier.myColumn - soldier.myColumn);
        }

        public static bool compareMoves(Move a, Move b)
        {
            if (a != null && b != null &&
                a.OldCell.myRow == b.OldCell.myRow &&
                a.OldCell.myColumn == b.OldCell.myColumn &&
                a.NewCell.myRow == b.NewCell.myRow &&
                a.NewCell.myColumn == b.NewCell.myColumn &&
                a.Type == b.Type)
            {
                return true;
            }
            else { return false; }
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
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

        public enum ISoldiers
        {
            empty,
            dark_soldier,
            light_soldier,
            dark_town,
            light_town
        }

        public enum IMoves
        {
            none,
            slideCannon,
            step,
            retreat,
            capture,
            shootCannon
        }

        public enum INode 
        {
            dark_wins,
            light_wins,
            leaf
        }

        public static int Size = 10; // Board size
        public static readonly int[] ColumnLightSoldiers = { 1, 3, 5, 7, 9 }; // Initial row and column values foreach soldier color
        public static readonly int[] RowLightSoldiers = { 6, 7, 8 };
        public static readonly int[] ColumnDarkSoldiers = { 0, 2, 4, 6, 8 };
        public static readonly int[] RowDarkSoldiers = { 1, 2, 3 };

        #region printMethods
        public static void printBoard(BoardState s, bool printNumbers = true)
        {
            string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            Console.WriteLine("Counter:" + s.turnCounter);
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();
            int counter = 0;
            for (int i = 10 - 1; i >= 0; i--)
            {
                Console.Write(i + 1);
                Console.Write(" ");
                if (i != 9) { Console.Write(" "); }
                for (int j = 0; j < 10; j++)
                {
                    Cell c = s.myGrid[i, j];
                    if (CannonUtils.IsOdd(s.turnCounter))
                    {
                        // Light soldiers turn (p2)
                        if (c.myPiece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                        else if (c.myPiece == CannonUtils.ISoldiers.light_soldier) 
                        {
                            if (printNumbers) { Console.Write(counter); counter++; }
                            else { Console.Write("O"); }
                            
                        
                        }
                        else if (c.myPiece == CannonUtils.ISoldiers.dark_town || c.myPiece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.myPiece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }

                    }
                    else
                    {
                        if (c.myPiece == CannonUtils.ISoldiers.dark_soldier) 
                        {
                            if (printNumbers) { Console.Write(counter); counter++; }
                            else { Console.Write("X"); }
                             
                        }
                        else if (c.myPiece == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                        else if (c.myPiece == CannonUtils.ISoldiers.dark_town || c.myPiece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.myPiece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
                    }
                    if (j != 9) { Console.Write(" - "); }

                }
                Console.WriteLine();
                if (i != 0)
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

        public static void printBoardWithMoves(BoardState s, Cell chosen)
        {
            string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            Console.WriteLine("================================= Counter:" + s.turnCounter);
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();

            List<Move> list = s.legalMoves.Where(x => x.OldCell.myRow == chosen.myRow && x.OldCell.myColumn == chosen.myColumn).ToList();
            for (int i = 10 - 1; i >= 0; i--)
            {
                Console.Write(i + 1);
                Console.Write(" ");
                if (i != 9) { Console.Write(" "); }
                for (int j = 0; j < 10; j++)
                {
                    Cell c = s.myGrid[i, j];
                    Move mm = list.Where(x => x.NewCell.myRow == c.myRow && x.NewCell.myColumn == c.myColumn).FirstOrDefault();
                    if (c == chosen) { Console.Write("S"); }
                    else if (mm != null)
                    {
                        if (mm.Type == CannonUtils.IMoves.step) { Console.Write("m"); }
                        else if (mm.Type == CannonUtils.IMoves.retreat) { Console.Write("r"); }
                        else if (mm.Type == CannonUtils.IMoves.capture) { Console.Write("k"); }
                        else if (mm.Type == CannonUtils.IMoves.slideCannon) { Console.Write("s"); }
                    }
                    else if (c.myPiece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                    else if (c.myPiece == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                    else if (c.myPiece == CannonUtils.ISoldiers.dark_town || c.myPiece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                    else if (c.myPiece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
                    if (j != 9) { Console.Write(" - "); }

                }
                Console.WriteLine();
                if (i != 0)
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
            Console.WriteLine("=================================");

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
            string oldrow = (move.OldCell.myRow + 1).ToString();
            string newrow = (move.NewCell.myRow + 1).ToString();
            string oldcol = intToColumn(move.OldCell.myColumn);
            string newcol = intToColumn(move.NewCell.myColumn);
            string oldc = oldcol + oldrow;
            string newc = newcol + newrow;
            string sentence = oldc + " to " + newc;
            switch (move.Type)
            {
                case CannonUtils.IMoves.step:
                    Console.WriteLine(count + "     - STEP " + sentence);
                    break;
                case CannonUtils.IMoves.capture:
                    Console.WriteLine(count + "     - CAPTURE " + sentence);
                    break;
                case CannonUtils.IMoves.retreat:
                    Console.WriteLine(count + "     - RETREAT " + sentence);
                    break;
                case CannonUtils.IMoves.slideCannon:
                    Console.WriteLine(count + "     - SLIDE " + sentence);
                    break;
                case CannonUtils.IMoves.shootCannon:
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
