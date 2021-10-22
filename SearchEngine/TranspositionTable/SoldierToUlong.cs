using CannonModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchEngine
{
    /// a = RandomNumbersMatrix [1][15]
    /// b = RandomNumbersMatrix [0][58]
    /// c = RandomNumbersMatrix [0][42]
    ///    0 1 2 3 4 5 6 7 8 9
    /// 90 · · · · · · · · · ·
    /// 80 · · · · · · · · · ·
    /// 70 · · · · · · · · · ·
    /// 60 · · · · · · · · · ·
    /// 50 · · · · · · · · b ·
    /// 40 · · c · · · · · · ·
    /// 30 · · · · · · · · · ·
    /// 20 · · · · · · · · · ·
    /// 10 · · · · · a · · · ·
    /// 00 · · · · · · · · · ·
    public class SoldierToUlong
    {
        public static ulong[,] table { get; set; }
        public SoldierToUlong()
        {
            init_Zobrist();
        }

        /// <summary>
        /// [m] different types of pieces in total (Go m=1+1=2)(Chess m=6+6=12)(Cannon m=2+2=4 if town is considered) 
        /// [n] squares (Go n=19x19=361)(Chess n=8x8=64)(Cannon n=10x10=100)
        /// [mxn] random numbers needed
        /// https://stackoverflow.com/questions/6651554/random-number-in-long-range-is-this-the-way
        /// https://stackoverflow.com/questions/7201972/convert-datatype-long-to-byte-array
        /// </summary>
        private void init_Zobrist()
        {
            // fill a table of random numbers/bitstrings
            int n_squares = 100;
            int m_pieces = 4;
            table = new ulong[n_squares, m_pieces];
            Random random = new Random();
            for (int i = 0; i < n_squares; i++)
            {
                for (int j = 0; j < m_pieces; j++)
                {
                    //byte[] buff = Zobrist.Bytes8Random(random);
                    table[i, j] = uLongRandom(random);
                }
            }
        }

        public ulong SoldierToBitArray(int boardIndex, CannonUtils.eSoldiers type)
        {
            int dim1 = (int)type - 1;
            return table[boardIndex, dim1];
        }

        static ulong uLongRandom(Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            ulong longRand = BitConverter.ToUInt64(buf, 0);

            return longRand;
        }
    }
}
