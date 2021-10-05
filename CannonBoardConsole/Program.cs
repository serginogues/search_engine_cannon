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
            BoardState a = new BoardState();
            TranspositionTable TT = new TranspositionTable(a);
            //a.State.printBoard();
            Console.WriteLine("Key a = " + TT.hashFunction(TT.zobristHashWithOperations(a)));
            BoardState b = a.Successor(0);
            //b.State.printBoard();
            Console.WriteLine("Key b = " + TT.hashFunction(TT.zobristHashWithOperations(b)));
            Console.WriteLine("Key b by operation = " + TT.hashFunction(TT.zobristHash(b)));
            TT.Store(a, 0, 5, AIUtils.ITTEntryFlag.exact_value, 1);
            TT.Store(b, 0, 5, AIUtils.ITTEntryFlag.exact_value, 1);
            Entry n = TT.TableLookup(b);
            Console.WriteLine(TT.hashFunction(n.zobristHashKey));
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
