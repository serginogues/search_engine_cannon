using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;
using System.Linq;
using System.Collections;

namespace SearchEngine
{
    public static class AIUtils
    {
        public enum IType
        {
            MAX,
            MIN
        }

        public enum ITTEntryFlag
        {
            exact_value,
            lower_bound,
            upper_bound
        }

        public static int getIntFromBitArray(BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }

        public static long random64Bit(Random rand)
        {
            long num = -1000000000000000000L;
            while (num < 0)
            {
                num = (long)(rand.Next(1, 100000) * 1000000000000000000L);
            }
            return num;
        }
    }
}
