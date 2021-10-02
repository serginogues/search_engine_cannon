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
        private static List<List<int>> RandomNumbersMatrix { get; set; }

        public int InitialStateKey { get; set; }

        public TranspositionTable()
        {
            TT = new List<TTEntry>(); 
            GenerateRandomNumbers();
        }

        /// <summary>
        /// [m] different types of pieces in total (Go m=1+1=2)(Chess m=6+6=12)(Cannon m=2+2=4 if town is considered) 
        /// [n] squares (Go n=19x19=361)(Chess n=8x8=64)(Cannon n=10x10=100)
        /// [mxn] random numbers needed (+ one extra XOR for changing side to move)
        /// </summary>
        private void GenerateRandomNumbers()
        {
            RandomNumbersMatrix = new List<List<int>>();
            Random rnd = new Random();
            for (int m = 0; m < m_pieces; m++)
            {
                RandomNumbersMatrix.Add(new List<int>());
                for (int n = 0; n < n_squares; n++)
                {
                    RandomNumbersMatrix[m].Add(rnd.Next(1, n_squares*m_pieces*10));
                }
            }
        }

        /// <summary>
        /// RandomNumbersMatrix is 2D
        /// FIRST DIMENSION
        /// dark_soldier    -> RandomNumbersMatrix [0][:]
        /// light_soldier   -> RandomNumbersMatrix [1][:]
        /// dark_town       -> RandomNumbersMatrix [2][:]
        /// light_town      -> RandomNumbersMatrix [3][:]
        /// 
        /// SECOND DIMENSION
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
        /// </summary>
        private int PieceToRandomValue(int row, int column, CannonUtils.ISoldiers type) 
        {
            int dim2 = row * 10 + column;
            int dim1 = (int)type - 1; 
            return RandomNumbersMatrix[dim1][dim2];
        }

        /// <summary>
        /// Given state retrieve TT entry
        /// if position is not found, depth will be -1
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public TTEntry GetEntryFromState(Node state)
        {
            // generate hash key from state 
            int key = HashKeyByScan(state);
            return TT.Exists(x => x.EntryKey == key) ? TT.Where(x => x.EntryKey == key).FirstOrDefault() : new TTEntry(0) { Depth = -1 };
        }

        /// <summary>
        /// Store new TTEntry in TT
        /// </summary>
        public void Store(Node s, int bestMove, int bestValue, AIUtils.ITTEntryFlag flag, int depth)
        {
            int key;
            // generate key from old and new cell values
            key = HashKeyByScan(s);
            TTEntry a = new TTEntry(key) 
            {
                BestMove = bestMove,
                Depth = depth,
                Flag = flag,
                Value = bestValue
            };
            TT.Add(a);
        }

        public int HashKeyByScan(Node state)
        {
            int key = 0;
            foreach (Cell soldier in state.State.Grid)
            {
                if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier ||
                    soldier.Piece == CannonUtils.ISoldiers.light_soldier ||
                    soldier.Piece == CannonUtils.ISoldiers.dark_town ||
                    soldier.Piece == CannonUtils.ISoldiers.light_town)
                {
                    key = key ^ PieceToRandomValue(soldier.Row, soldier.Column, soldier.Piece);
                }
            }
            return key;
        }

        //private int ZobristHashing(Node state)
        //{
        //    switch (state.NewCell.Move)
        //    {
        //        case CannonUtils.IMoves.step:
        //        case CannonUtils.IMoves.retreat:
        //        case CannonUtils.IMoves.slideCannon:
        //            return StepRetreatSlide(state);
        //        case CannonUtils.IMoves.capture:
        //            return Capture(state);
        //        case CannonUtils.IMoves.shootCannon:
        //            return Shoot(state);
        //    }
        //    return 0;
        //}
        
        ///// <summary>
        ///// Moving a piece
        ///// </summary>
        //private int StepRetreatSlide(Node state)
        //{
        //    // remove old friend
        //    int new_empty_cell = PieceToRandomValue(state.OldCell.Row, state.OldCell.Column, state.OldCell.Piece);
        //    // add new friend
        //    int soldier_moves_to_here = PieceToRandomValue(state.NewCell.Row, state.NewCell.Column, state.OldCell.Piece);
        //    return state.ParentTTKey ^ new_empty_cell ^ soldier_moves_to_here;
        //}

        ///// <summary>
        ///// key = key (^ r_old ^ r_new) ^
        ///// three changes: one soldier pops out, one cell becomes empty and one cell gets a new soldier
        ///// </summary>
        //private int Capture(Node state)
        //{
        //    // remove captured enemy
        //    int soldier_captured = PieceToRandomValue(state.NewCell.Row, state.NewCell.Column, state.NewCell.Piece);
        //    // remove old friend
        //    int new_empty_cell = PieceToRandomValue(state.OldCell.Row, state.OldCell.Column, state.OldCell.Piece);
        //    // add new friend
        //    int soldier_moves_to_here = PieceToRandomValue(state.NewCell.Row, state.NewCell.Column, state.OldCell.Piece);
        //    return state.ParentTTKey ^ soldier_captured ^ new_empty_cell ^ soldier_moves_to_here;
        //}

        //private int Shoot(Node state)
        //{
        //    // remove captured enemy
        //    return state.ParentTTKey ^ PieceToRandomValue(state.NewCell.Row, state.NewCell.Column, state.NewCell.Piece);
        //}
    }
}
