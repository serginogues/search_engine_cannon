using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;
using System.Linq;
using System.Collections;

namespace SearchEngine
{
    public class TranspositionTable
    {
        public List<Entry> TT { get; set; }
        private static long[,] Table { get; set; }

        public long RootKey { get; set; }

        public TranspositionTable(Node root)
        {
            TT = new List<Entry>(); 
            init_Zobrist();
            RootKey = Hash(root);
        }

        /// <summary>
        /// [m] different types of pieces in total (Go m=1+1=2)(Chess m=6+6=12)(Cannon m=2+2=4 if town is considered) 
        /// [n] squares (Go n=19x19=361)(Chess n=8x8=64)(Cannon n=10x10=100)
        /// [mxn] random numbers needed
        /// </summary>
        private void init_Zobrist()
        {
            // fill a table of random numbers/bitstrings
            int n_squares = 100;
            int m_pieces = 4;
            Table = new long[n_squares, m_pieces];
            Random rnd = new Random();
            for (int i = 0; i < n_squares; i++)
            {
                for (int j = 0; j < m_pieces; j++)
                {
                    Table[i, j] = random64Bit(rnd);
                }
            }
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

        private long SoldierToBitArray(int board_row, int board_column, CannonUtils.ISoldiers type)
        {
            int dim2 = board_row * 10 + board_column;
            int dim1 = (int)type - 1;
            return Table[dim2, dim1];
        }

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
        public long Hash(Node s)
        {
            long key = 0;
            foreach (Cell soldier in s.State.Grid)
            {
                if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier ||
                    soldier.Piece == CannonUtils.ISoldiers.light_soldier ||
                    soldier.Piece == CannonUtils.ISoldiers.dark_town ||
                    soldier.Piece == CannonUtils.ISoldiers.light_town)
                {
                    key = key ^ SoldierToBitArray(soldier.Row, soldier.Column, soldier.Piece);
                }
            }
            return key;
        }

        /// <summary>
        /// if position is not found, depth will be -1
        /// </summary>
        public Entry Retrieve(Node state)
        {
            // generate hash key from state 
            long key = HashByOperation(state);
            return TT.Exists(x => x.EntryKey == key) ? TT.Where(x => x.EntryKey == key).FirstOrDefault() : new Entry(random64Bit(new Random())) { Depth = -1 };
        }

        public long Store(Node s, int bestMove, int bestValue, AIUtils.ITTEntryFlag flag, int depth, List<int> list)
        {
            // generate key from old and new cell values
            long key = Hash(s);
            if (TT.Exists(x => x.EntryKey == key))
            {
                Console.WriteLine("repeated key!");
            }
            TT.Add(new Entry(key)
            {
                BestMove = bestMove,
                Depth = depth,
                Flag = flag,
                Value = bestValue,
                ChildList = list
            });
            return key;
        }

        public void Update(int bestMove, int bestValue, AIUtils.ITTEntryFlag flag, int depth, long key, List<int> indexes)
        {
            Entry e = TT.Where(x => x.EntryKey == key).FirstOrDefault();
            e.BestMove = bestMove;
            e.Value = bestValue;
            e.Flag = flag;
            e.Depth = depth;
            e.ChildList = indexes;
        }

        public long HashByOperation(Node state)
        {
            long key = RootKey;
            foreach (Move move in state.State.History)
            {
                switch (move.Type)
                {
                    case CannonUtils.IMoves.step:
                    case CannonUtils.IMoves.retreat:
                    case CannonUtils.IMoves.slideCannon:
                        key = StepRetreatSlide(key, move);
                        break;
                    case CannonUtils.IMoves.capture:
                        key = Capture(key, move);
                        break;
                    case CannonUtils.IMoves.shootCannon:
                        key = Shoot(key, move);
                        break;
                }
            }
            return key;
        }

        /// <summary>
        /// Moving a piece
        /// </summary>
        private long StepRetreatSlide(long key, Move state)
        {
            // remove old friend
            long new_empty_cell = SoldierToBitArray(state.OldCell.Row, state.OldCell.Column, state.OldCell.Piece);
            // add new friend
            long soldier_moves_to_here = SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.OldCell.Piece);
            return key ^ new_empty_cell ^ soldier_moves_to_here;
        }

        /// <summary>
        /// key = key (^ r_old ^ r_new) ^
        /// three changes: one soldier pops out, one cell becomes empty and one cell gets a new soldier
        /// </summary>
        private long Capture(long key, Move state)
        {
            // remove captured enemy
            long soldier_captured = SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.NewCell.Piece);
            // remove old friend
            long new_empty_cell = SoldierToBitArray(state.OldCell.Row, state.OldCell.Column, state.OldCell.Piece);
            // add new friend
            long soldier_moves_to_here = SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.OldCell.Piece);
            return key ^ soldier_captured ^ new_empty_cell ^ soldier_moves_to_here;
        }

        private long Shoot(long key, Move state)
        {
            // remove captured enemy
            return key ^ SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.NewCell.Piece);
        }
    }
}
