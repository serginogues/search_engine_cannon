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

        static void PlayGame(bool manual=true)
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
            a.State.printBoard();
            Console.WriteLine("Key a = "+TT.Hash(a));
            Node b = a.Successor(0);
            b.State.printBoard();
            Console.WriteLine("Key b                = " + TT.Hash(b));
            Console.WriteLine("Key b by operation   = " + TT.HashByOperation(b));
            TT.Store(a, 0, 5, AIUtils.ITTEntryFlag.exact_value, 1, new List<int>()); ;
            TT.Store(b, 0, 5, AIUtils.ITTEntryFlag.exact_value, 1, new List<int>());
            Entry n = TT.Retrieve(b);
            Console.ReadLine();
        }

        static void HashingTest()
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
