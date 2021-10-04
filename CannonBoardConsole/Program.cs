using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CannonModel;
using SearchEngine;

namespace CannonBoardConsole
{
    class Program
    {
        static BoardState myBoard;

        static void Main() 
        {
            PlayGame();
        }

        static void PlayGame(bool manual=false)
        {
            myBoard = new BoardState();

            if (manual)
            {
                // Set Towns
                ManualUser user = new ManualUser();
                user.SetTowns(myBoard);

                // Play game
                for (int i = 0; i < 100; i++)
                {
                    user.MakeMove(myBoard);
                }
            }
            else
            {
                AIUser user = new AIUser();
                user.SetTowns(myBoard);

                // Play game
                for (int i = 0; i < 100; i++)
                {
                    user.MakeMove(myBoard);
                }
            }
        }

        static void TTTest()
        {
            Node a = new Node(new BoardState());
            TranspositionTable TT = new TranspositionTable(a);
            //a.State.printBoard();
            //Console.WriteLine("Long key a = "+TT.Hash(a));
            //Console.WriteLine("Key a = " + Zobrist.long2doubleInt(TT.Hash(a))[0]);
            //Node b = a.Successor(0);
            ////b.State.printBoard();
            //Console.WriteLine("Long key b                = " + TT.Hash(b));
            //Console.WriteLine("Key b = " + Zobrist.long2doubleInt(TT.Hash(b))[0]);
            //Console.WriteLine("Long key b by operation   = " + TT.HashByOperation(b));
            //Console.WriteLine("Key b by operation = " + Zobrist.long2doubleInt(TT.HashByOperation(b))[0]);
            //TT.Store(a, 0, 5, AIUtils.ITTEntryFlag.exact_value, 1);
            //TT.Store(b, 0, 5, AIUtils.ITTEntryFlag.exact_value, 1);
            //Entry n = TT.Retrieve(b);
            //Console.WriteLine(n.EntryKey == (int)Zobrist.long2doubleInt(TT.Hash(b))[0]);
            Console.ReadLine();
        }

        static void BitArrayTest()
        {
            BitArray a = new BitArray(new int[] { 14 });
            BitArray b = new BitArray(new int[] { 11 });
            BitArray c = a.Xor(b); // 5
            BitArray d = b.Xor(a); // 14
            Console.WriteLine(AIUtils.getIntFromBitArray(c));
            Console.WriteLine(AIUtils.getIntFromBitArray(d));
            Console.WriteLine(d == b); // True
            Console.ReadLine();
        }
    }
}
