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

        /// <summary>
        /// https://stackoverflow.com/questions/200574/linq-equivalent-of-foreach-for-ienumerablet
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static bool IsSequential(int[] array)
        {
            return array.Zip(array.Skip(1), (a, b) => (a + 1) == b).All(x => x);
        }

        public enum ISoldiers
        {
            empty,
            dark_soldier,
            light_soldier,
            dark_town,
            light_town
        }

        public enum IUserActions
        {
            click_soldier,
            click_shoot,
            reset_available_moves,
            click_moves
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
    }
}
