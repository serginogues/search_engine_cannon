using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SearchEngine
{
    public static class Zobrist
    {
        
        public static ushort hashFunctionDeprecated(ulong zobristHashKey) 
        {
            // hash it, and get the corresponding entry: integer between 0 and 2 ^ 16 - 1
            var index = zobristHashKey & 0xFFFF;
            ushort vOut = Convert.ToUInt16(index);
            // byte[] intt = BitConverter.GetBytes(vOut);
            // Console.WriteLine(index);
            return vOut;
        }

        public static int[] long2doubleInt(ulong a)
        {
            int a1 = (int)(a & uint.MaxValue);
            int a2 = (int)(a >> 32);
            return new int[] { a1, a2 };
        }

        public static ulong doubleInt2long(int a1, int a2)
        {
            ulong b = (ulong)a2;
            b = b << 32;
            b = b | (uint)a1;
            return b;
        }

        public static ulong uLongRandom(Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            ulong longRand = BitConverter.ToUInt64(buf, 0);
            //while (longRand <= 0)
            //{
            //    buf = new byte[8];
            //    rand.NextBytes(buf);
            //    longRand = BitConverter.ToUInt64(buf, 0);
            //}

            return longRand;
        }

        public static byte[] Bytes8Random(Random rand) 
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            return buf;
        }

        public static void Test()
        {
            Random random = new Random();
            byte[] buff = Zobrist.Bytes8Random(random);

            

            BitArray bits = new BitArray(buff);

            ulong a = 12345678910111213;
            int[] al = long2doubleInt(a);
            Console.WriteLine(al[0]);
            BitArray b = new BitArray(new int[] { al[0] });
            Console.WriteLine(al[1]);
            Console.WriteLine(b.Count);
            ulong ap = doubleInt2long(al[0], al[1]);
            Console.WriteLine(ap);
            Console.ReadKey();
        }
    }
}
