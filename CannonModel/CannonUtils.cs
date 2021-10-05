using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CannonModel
{
    public static class CannonUtils
    {
        public static IBounds BoundsState(Cell cell)
        {
            if (cell.Row == 9)
            {
                if (cell.Column == 0) { return IBounds.LeftUpperC; }
                else if (cell.Column == 9) { return IBounds.RightUpperC; }
                else { return IBounds.upperBound; }
            }
            else if (cell.Row == 0)
            {
                if (cell.Column == 0) { return IBounds.LeftDownC; }
                else if (cell.Column == 9) { return IBounds.RightDownC; }
                else { return IBounds.lowerBound; }
            }
            else
            {
                if (cell.Column == 0) { return IBounds.leftBound; }
                else if (cell.Column == 9) { return IBounds.rightBound; }
                else { return IBounds.middle; }
            }
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

        public enum IBounds
        {
            middle,
            upperBound,
            lowerBound,
            leftBound,
            rightBound,
            LeftUpperC,
            LeftDownC,
            RightUpperC,
            RightDownC
        }

        public enum IMoves
        {
            none,
            step,
            retreat,
            capture,
            slideCannon,
            shootCannon
        }

        public static int Size = 10; // Board size
        public static readonly int[] ColumnLightSoldiers = { 1, 3, 5, 7, 9 }; // Initial row and column values foreach soldier color
        public static readonly int[] RowLightSoldiers = { 6, 7, 8 };
        public static readonly int[] ColumnDarkSoldiers = { 0, 2, 4, 6, 8 };
        public static readonly int[] RowDarkSoldiers = { 1, 2, 3 };

        public static string ColumnInt_To_Char(int num)
        {
            switch (num)
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
            return "A";
        }

        #region printMethods
        public static void printBoard(BoardState s)
        {
            string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            Console.WriteLine("================================= Counter:" + s.TurnCounter);
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
                    Cell c = s.Grid[i, j];
                    if (CannonUtils.IsOdd(s.TurnCounter))
                    {
                        // Light soldiers turn (p2)
                        if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                        else if (c.Piece == CannonUtils.ISoldiers.light_soldier) { Console.Write(counter); counter++; }
                        else if (c.Piece == CannonUtils.ISoldiers.dark_town || c.Piece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.Piece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }

                    }
                    else
                    {
                        if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write(counter); counter++; }
                        else if (c.Piece == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                        else if (c.Piece == CannonUtils.ISoldiers.dark_town || c.Piece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.Piece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
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
            Console.WriteLine("=================================");

        }

        public static void printBoardWithMoves(BoardState s, Cell chosen)
        {
            string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            Console.WriteLine("================================= Counter:" + s.TurnCounter);
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();

            List<Move> list = s.LegalMoves.Where(x => x.OldCell.Row == chosen.Row && x.OldCell.Column == chosen.Column).ToList();
            for (int i = 10 - 1; i >= 0; i--)
            {
                Console.Write(i + 1);
                Console.Write(" ");
                if (i != 9) { Console.Write(" "); }
                for (int j = 0; j < 10; j++)
                {
                    Cell c = s.Grid[i, j];
                    Move mm = list.Where(x => x.NewCell.Row == c.Row && x.NewCell.Column == c.Column).FirstOrDefault();
                    if (c == chosen) { Console.Write("S"); }
                    else if (mm != null)
                    {
                        if (mm.Type == CannonUtils.IMoves.step) { Console.Write("m"); }
                        else if (mm.Type == CannonUtils.IMoves.retreat) { Console.Write("r"); }
                        else if (mm.Type == CannonUtils.IMoves.capture) { Console.Write("k"); }
                        else if (mm.Type == CannonUtils.IMoves.slideCannon) { Console.Write("s"); }
                    }
                    else if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                    else if (c.Piece == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                    else if (c.Piece == CannonUtils.ISoldiers.dark_town || c.Piece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                    else if (c.Piece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
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

        public static void printSoldiers(BoardState state)
        {
            Console.WriteLine("List of soldiers: ");
            int count = 0;
            foreach (Cell s in state.Grid)
            {
                if (s.Piece == CannonUtils.ISoldiers.dark_soldier ||
                    s.Piece == CannonUtils.ISoldiers.light_soldier ||
                    s.Piece == CannonUtils.ISoldiers.dark_town ||
                    s.Piece == CannonUtils.ISoldiers.light_town)
                {
                    Console.WriteLine(count + " - Soldier at: (" + s.Row + "," + s.Column + ")");
                    count++;
                }
            }
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

        private static void printMove(Move move, int count)
        {
            switch (move.Type)
            {
                case CannonUtils.IMoves.step:
                    Console.WriteLine(count + "     - Soldier steps from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
                case CannonUtils.IMoves.capture:
                    Console.WriteLine(count + "     - Soldier captures from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
                case CannonUtils.IMoves.retreat:
                    Console.WriteLine(count + "     - Soldier retreats from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
                case CannonUtils.IMoves.slideCannon:
                    Console.WriteLine(count + "     - Soldier slides from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
                case CannonUtils.IMoves.shootCannon:
                    Console.WriteLine(count + "     - Shoot soldier at cell (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
            }
        }

        public static void printNextPlayer(BoardState s)
        {
            if (CannonUtils.IsOdd(s.TurnCounter)) { Console.WriteLine("Light soldiers turn (p2)"); }
            else { Console.WriteLine("Dark soldiers turn (p1)"); }
        }
        #endregion
    }
}
