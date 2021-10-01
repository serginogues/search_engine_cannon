using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;
using System.Linq;

namespace SearchEngine
{
    public class TranspositionTable
    {
        public List<TTEntry> TT { get; set; }
        // 1 to 10 is first row and so on
        private const int n_squares = 100;
        // 1=dark_soldier, 2=light_soldier, 3=dark_town, 4=light_town
        private const int m_pieces = 4;
        private List<List<int>> RandomNumbersMatrix { get; set; }

        public TranspositionTable()
        {
            TT = new List<TTEntry>();
            RandomNumbersMatrix = new List<List<int>>();
            GenerateRandomNumbers();
        }

        private List<List<int>> GenerateRandomNumbers()
        {
            Random rnd = new Random();
            for (int m = 0; m < m_pieces; m++)
            {
                for (int n = 0; n < n_squares; n++)
                {
                    RandomNumbersMatrix[m][n] = rnd.Next();
                }
            }
            return RandomNumbersMatrix;
        }

        /// <summary>
        /// [m] different types of pieces in total (Go m=1+1=2)(Chess m=6+6=12)(Cannon m=2+2=4 if town is considered) 
        /// [n] squares (Go n=19x19=361)(Chess n=8x8=64)(Cannon n=10x10=100)
        /// [mxn] random numbers needed + one extra XOR for changing side to move
        /// </summary>
        public int ZobristHashing(BoardState state)
        {
            // pick starting state and generate has key by doing XOR operations based on the previous moves taken
            //Hash test
            int a = 14, b = 11, c = 14, result;
            result = a ^ b;
            Console.WriteLine("a^b: " + result);
            result = a ^ b ^ c;
            Console.WriteLine("a^b^c: " + result);
            result = result ^ c;
            Console.WriteLine("result^c: " + result);
            return 0;
        }

        /// <summary>
        /// Given state retrieve TT entry
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public TTEntry GetEntryFromState(BoardState state)
        {
            // generate hash key from state 
            int key = ZobristHashing(state);
            if(TT.Exists(x => x.Key == key))
            {

            }
            else
            {
                // if position is not found, depth will be -1
                return new TTEntry(0) { Depth = -1 };
            }
            return new TTEntry(0);
        }

        /// <summary>
        /// Store new TTEntry in TT
        /// </summary>
        public void Store(BoardState s, int bestMove, int bestValue, AIUtils.ITTEntryFlag flag, int depth)
        {
            TTEntry a = new TTEntry(0);
            TT.Add(a);
        }
    }
}
