using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;
using System.Linq;
using System.Collections;

namespace SearchEngine
{
    /// <summary>
    /// http://mediocrechess.blogspot.com/2007/01/guide-transposition-tables.html
    /// https://stackoverflow.com/questions/20009796/transposition-tables
    /// https://adamberent.com/2019/03/02/transposition-table-and-zobrist-hashing/
    /// </summary>
    public class TranspositionTable
    {
        /// <summary>
        /// 20 bit per entry.key
        /// </summary>
        public Entry[] TT { get; set; }
        
        public SoldierToUlong Table { get; set; }

        public ulong rootZobristHash { get; set; }

        public TranspositionTable(BoardState root)
        {
            TT = new Entry[ushort.MaxValue];
            Table = new SoldierToUlong();
            rootZobristHash = zobristHash(root);
        }

        /// <summary>
        /// [zobrist key]%[total size of the hashtable]
        /// </summary>
        public int hashFunction(ulong zobristHashKey)
        {
            int value = (int)zobristHashKey % TT.Length;
            if (value < 0)
            {
                return -value;
            }
            return value;
        }

        /// <summary>
        /// Table Lookup
        /// For a given position, compute the zobrist key, and then a hash of the zobrist key, which will be your index in your transposition table.
        /// You don't want to store 2^32 or 2^64 entries, so you take a "hash" of the Zobrist key to limit entries of the transposition table.
        /// (i.e. 16 bits for 2^16 game positions (in reality it's probably >=2^20)). 
        /// To obtain this hash, a simple method is to "modulo" the zobrist key, or do a "binary and" : transposition table index = zobrist_key & 0xFFFF
        /// If position is not found, depth will be -1
        /// </summary>
        public Entry TableLookup(BoardState s)
        {
            // compute the zobrist key for the given game position
            ulong zobristHashKey = zobristHashWithOperations(s);

            // 16-bit transposition table index
            int entry_key = hashFunction(zobristHashKey);
            var n = TT[entry_key];

            if (n == null) { return new Entry(0) { Depth = -1 }; }
            else if (n.zobristHashKey == zobristHashKey) 
            {
                n.Ancient = false;
                return n; 
            }
            else
            {
                // collision
                return new Entry(0) { Depth = -1 };
            }
        }

        public void Store(BoardState s, int bestMove, int bestValue, AIUtils.ITTEntryFlag flag, int depth)
        {
            // For a given position, compute the zobrist key, and then a hash of the zobrist key, which will be your index in your transposition table.
            ulong zobristHashKey = zobristHashWithOperations(s);
            int entry_key = hashFunction(zobristHashKey);
            //var n = TT[entry_key];

            // always replace
            TT[entry_key] = new Entry(zobristHashKey)
            {
                BestMove = bestMove,
                Depth = depth,
                Flag = flag,
                Score = bestValue
            };

            // Type - 2 error Replacing Scheme: Always replace(if not ancient)
            //if (n != null) { ReplaceIfNotAncient(n, entry_key, zobristHashKey, bestMove, bestValue, flag, depth); }
            //else
            //{
            //    TT[entry_key] = new Entry(zobristHashKey)
            //    {
            //        BestMove = bestMove,
            //        Depth = depth,
            //        Flag = flag,
            //        Score = bestValue
            //    };
            //}

        }

        private void ReplaceIfNotAncient(Entry n, int entry_key, ulong zobristHashKey, int bestMove, int bestValue, AIUtils.ITTEntryFlag flag, int depth)
        {
            if (n.Ancient)
            {
                TT[entry_key] = new Entry(zobristHashKey)
                {
                    BestMove = bestMove,
                    Depth = depth,
                    Flag = flag,
                    Score = bestValue
                };
            }
        }

        public void Update(int bestMove, int bestValue, AIUtils.ITTEntryFlag flag, int depth, ulong zobristHashKey)
        {
            int entry_key = hashFunction(zobristHashKey);
            Entry e = TT[entry_key];
            e.BestMove = bestMove;
            e.Score = bestValue;
            e.Flag = flag;
            e.Depth = depth;
        }

        public void ResetAllAncientFlags()
        {
            foreach (Entry item in TT.Where(x => x != null))
            {
                item.Ancient = true;
            }
        }

        #region Zobrist Hash

        public ulong zobristHash(BoardState s)
        {
            ulong key = 0;
            foreach (Cell soldier in s.Grid)
            {
                if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier ||
                    soldier.Piece == CannonUtils.ISoldiers.light_soldier ||
                    soldier.Piece == CannonUtils.ISoldiers.dark_town ||
                    soldier.Piece == CannonUtils.ISoldiers.light_town)
                {
                    key = key ^ Table.SoldierToBitArray(soldier.Row, soldier.Column, soldier.Piece);
                }
            }
            // Return 20 first bits
            // byte[] bits64 = BitConverter.GetBytes(key);
            // byte[] bits20 = bits64.Take(3).ToArray();
            return key;
        }

        public ulong zobristHashWithOperations(BoardState state)
        {
            ulong key = rootZobristHash;
            foreach (Move move in state.History)
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
        private ulong StepRetreatSlide(ulong key, Move state)
        {
            // remove old friend
            ulong new_empty_cell = Table.SoldierToBitArray(state.OldCell.Row, state.OldCell.Column, state.OldCell.Piece);
            // add new friend
            ulong soldier_moves_to_here = Table.SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.OldCell.Piece);
            return key ^ new_empty_cell ^ soldier_moves_to_here;
        }

        /// <summary>
        /// key = key (^ r_old ^ r_new) ^
        /// three changes: one soldier pops out, one cell becomes empty and one cell gets a new soldier
        /// </summary>
        private ulong Capture(ulong key, Move state)
        {
            // remove captured enemy
            ulong soldier_captured = Table.SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.NewCell.Piece);
            // remove old friend
            ulong new_empty_cell = Table.SoldierToBitArray(state.OldCell.Row, state.OldCell.Column, state.OldCell.Piece);
            // add new friend
            ulong soldier_moves_to_here = Table.SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.OldCell.Piece);
            return key ^ soldier_captured ^ new_empty_cell ^ soldier_moves_to_here;
        }

        private ulong Shoot(ulong key, Move state)
        {
            // remove captured enemy
            return key ^ Table.SoldierToBitArray(state.NewCell.Row, state.NewCell.Column, state.NewCell.Piece);
        }
        #endregion
    }
}
