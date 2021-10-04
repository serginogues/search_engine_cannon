using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CannonModel
{
    /// <summary>
    /// Board coordinates follow this code: http://www.iggamecenter.com/info/en/cannon.html
    ///   A B C D E F G H I J
    /// 9 · · · · · · · · · ·
    /// 8 · · · · · · · · · ·
    /// 7 · · · · · · · · · ·
    /// 6 · · · · · · · · · ·
    /// 5 · · · · · · · · · ·
    /// 4 · · · · · · · · · ·
    /// 3 · · · · · · · · · ·
    /// 2 · · · · · · · · · ·
    /// 1 · · · · · · · · · ·
    /// 0 · · · · · · · · · ·
    /// That is, position 9-B = Grid[8,1]
    /// ****************************************************************************************
    /// Compute legal moves given current state of the board with ResetStateBoard()
    /// 
    /// </summary>
    public class BoardState
    {
        #region Properties
        public int TurnCounter { get; set; }
        public Cell[,] Grid { get; set; }
        public List<Move> LegalMoves { get; set; }
        /// <summary>
        /// List of moves from root to this state of the board
        /// </summary>
        public List<Move> History { get; set; }

        public BoardState ()
        {
            History = new List<Move>();
            TurnCounter = 0;
            InitGrid();
            ResetStateBoard(); 
        }
        #endregion

        #region Main methods
        /// <summary>
        /// Reset AVAILABLE_MOVES and CURRENT_SOLDIER
        /// </summary>
        public void ResetStateBoard()
        {
            LegalMoves = new List<Move>();
            foreach (Cell soldier in Grid)
            {
                if (soldier.Piece == Friend) 
                {
                    SetSTEP_CAPTURE_RETREATMoves(soldier);
                    SetCANNONMoves(soldier);
                }
                if (soldier.Piece == Enemy || soldier.Piece == TownEnemy) { SetSHOOTS(soldier); }
            }
        }

        public void ExecuteMove(Move move)
        {
            switch (move.Type)
            {
                case CannonUtils.IMoves.shootCannon:
                    Grid[move.NewCell.Row, move.NewCell.Column].Piece = CannonUtils.ISoldiers.empty;
                    break;
                case CannonUtils.IMoves.step:
                case CannonUtils.IMoves.retreat:
                case CannonUtils.IMoves.capture:
                case CannonUtils.IMoves.slideCannon:
                    Grid[move.NewCell.Row, move.NewCell.Column].Piece = move.OldCell.Piece;
                    Grid[move.OldCell.Row, move.OldCell.Column].Piece = CannonUtils.ISoldiers.empty;
                    break;
            }
            History.Add(move);
            TurnCounter++;
            ResetStateBoard();
        }


        /// <summary>
        /// To make children from current BoardState and change their properties without referencing its parent
        /// </summary>
        public BoardState DeepCopy()
        {
            BoardState other = (BoardState)this.MemberwiseClone();
            other.LegalMoves = new List<Move>(LegalMoves);
            other.History = new List<Move>(History);
            other.TurnCounter = TurnCounter;
            other.Grid = new Cell[CannonUtils.Size, CannonUtils.Size];
            for (int i = 0; i < CannonUtils.Size; i++)
            {
                for (int j = 0; j < CannonUtils.Size; j++)
                {
                    other.Grid[i, j] = Grid[i, j].DeepCopy();
                }
            }
            return other;
        }
        #endregion

        #region Moves
        /// <param name="_column"> Column to be added the town </param>
        /// <param name="_color"> True == Dark,  False == Light </param>
        public void AddTown(int _column, CannonUtils.ISoldiers _color)
        {
            if (_color == CannonUtils.ISoldiers.dark_soldier) { Grid[0, _column].Piece = CannonUtils.ISoldiers.dark_town; }
            else if (_color == CannonUtils.ISoldiers.light_soldier) { Grid[9, _column].Piece = CannonUtils.ISoldiers.light_town; }
            TurnCounter++;
        }

        /// <summary>
        /// A soldier may move one STEP forward or diagonally forward to an adjacent empty point.
        /// A soldier may CAPTURE an enemy piece (a soldier or the Town) standing on an adjacent point 
        /// by moving one step sideways, forward or diagonally forward
        /// </summary>
        private void SetSTEP_CAPTURE_RETREATMoves(Cell soldier)
        {
            bool retreat = false;
            if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
            {
                if(soldier.Row < 9)
                {
                    // (i+1, j)
                    // capture or step
                    Cell c = Grid[soldier.Row + 1, soldier.Column];
                    retreat = StepCaptureRetreat(soldier, c);

                    // (i+1, j-1)
                    if (soldier.Column > 0)
                    {
                        c = Grid[soldier.Row + 1, soldier.Column - 1];
                        retreat = StepCaptureRetreat(soldier, c);
                    }

                    // (i+1, j+1)
                    if (soldier.Column < 9)
                    {
                        c = Grid[soldier.Row + 1, soldier.Column + 1];
                        retreat = StepCaptureRetreat(soldier, c);
                    }
                }
                // (i, j-1)
                if (soldier.Column > 0)
                {
                    Cell c = Grid[soldier.Row, soldier.Column - 1];
                    retreat = StepCaptureRetreat(soldier, c);
                }

                // (i, j+1)
                if (soldier.Column < 9)
                {
                    Cell c = Grid[soldier.Row, soldier.Column + 1];
                    retreat = StepCaptureRetreat(soldier, c);
                }

                // check if enemy in back row
                
                
                // (i-1, j+1)
                if (soldier.Row > 1)
                {
                    // (i-1, j)
                    if (Grid[soldier.Row - 1, soldier.Column].Piece == Enemy)
                    {
                        retreat = true;
                    }
                    // (i-1, j-1)
                    else if (soldier.Column > 0 && Grid[soldier.Row - 1, soldier.Column - 1].Piece == Enemy)
                    {
                        retreat = true;
                    }
                    else if (soldier.Column < 9 && Grid[soldier.Row - 1, soldier.Column + 1].Piece == Enemy)
                    {
                        retreat = true;
                    }
                }
            }            
            else
            {
                if (soldier.Row > 0)
                {
                    // (i-1, j)
                    // capture or step
                    Cell c = Grid[soldier.Row - 1, soldier.Column];
                    retreat = StepCaptureRetreat(soldier, c);

                    // (i-1, j-1)
                    if (soldier.Column > 0)
                    {
                        c = Grid[soldier.Row - 1, soldier.Column - 1];
                        retreat = StepCaptureRetreat(soldier, c);
                    }

                    // (i-1, j+1)
                    if (soldier.Column < 9)
                    {
                        c = Grid[soldier.Row - 1, soldier.Column + 1];
                        retreat = StepCaptureRetreat(soldier, c);
                    }
                }
                // (i, j-1)
                if (soldier.Column > 0)
                {
                    Cell c = Grid[soldier.Row, soldier.Column - 1];
                    retreat = StepCaptureRetreat(soldier, c);
                }

                // (i, j+1)
                if (soldier.Column < 9)
                {
                    Cell c = Grid[soldier.Row, soldier.Column + 1];
                    retreat = StepCaptureRetreat(soldier, c);
                }

                // check if enemy in back row
                if (soldier.Row < 8)
                {
                    // (i+1, j)
                    if (Grid[soldier.Row + 1, soldier.Column].Piece == Enemy)
                    {
                        retreat = true;
                    }
                    // (i+1, j-1)
                    else if (soldier.Column > 0 && Grid[soldier.Row + 1, soldier.Column - 1].Piece == Enemy)
                    {
                        retreat = true;
                    }
                    // (i+1, j+1)
                    else if (soldier.Column < 9 && Grid[soldier.Row + 1, soldier.Column + 1].Piece == Enemy)
                    {
                        retreat = true;
                    }
                }
            }

            if (retreat) { SetRETREATMoves(soldier); }
        }

        /// <summary>
        /// return true if c is enemy
        /// </summary>
        private bool StepCaptureRetreat(Cell soldier, Cell c)
        {
            bool retreat = false;
            if (c.Piece == CannonUtils.ISoldiers.empty)
            {
                // step
                LegalMoves.Add(new Move(soldier.DeepCopy(),
                                        c.DeepCopy(),
                                        CannonUtils.IMoves.step));
            }
            else if (c.Piece == Enemy || c.Piece == TownEnemy)
            {
                // capture or retreat
                if ((soldier.Piece == CannonUtils.ISoldiers.dark_soldier && soldier.Row > 1) ||
                    soldier.Piece == CannonUtils.ISoldiers.light_soldier && soldier.Row < 8)
                {
                    retreat = true;
                }
                LegalMoves.Add(new Move(soldier.DeepCopy(),
                                        c.DeepCopy(),
                                        CannonUtils.IMoves.capture));
            }
            return retreat;
        }

        /// <summary>
        /// A soldier can retreat two points backwards or diagonally backwards 
        /// if it is adjacent to an enemy soldier and if the target and intermediate spots are empty
        /// </summary>
        private void SetRETREATMoves(Cell soldier)
        {
            bool canRetreatLeft = soldier.Column > 1 ? true : false;
            bool canRetreatRight = soldier.Column < 8 ? true : false;
            if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
            {
                // i to i-1
                // retreat backward
                Cell c = Grid[soldier.Row - 2, soldier.Column];
                if (Grid[soldier.Row - 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty &&
                    c.Piece == CannonUtils.ISoldiers.empty)
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                            c.DeepCopy(), 
                                            CannonUtils.IMoves.retreat));
                }

                // retreat left diagonal
                c = Grid[soldier.Row - 2, soldier.Column - 2];
                if (canRetreatLeft &&
                    Grid[soldier.Row - 1, soldier.Column-1].Piece == CannonUtils.ISoldiers.empty &&
                    c.Piece == CannonUtils.ISoldiers.empty)
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                    c.DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat right diagonal
                c = Grid[soldier.Row - 2, soldier.Column + 2];
                if (canRetreatRight &&
                    Grid[soldier.Row - 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty &&
                    c.Piece == CannonUtils.ISoldiers.empty)
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                    c.DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

            }
            else if (soldier.Piece == CannonUtils.ISoldiers.light_soldier)
            {
                // i to i+1
                // retreat backward
                Cell c = Grid[soldier.Row + 2, soldier.Column];
                if (Grid[soldier.Row + 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty &&
                    c.Piece == CannonUtils.ISoldiers.empty)
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                    c.DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat left diagonal
                c = Grid[soldier.Row + 2, soldier.Column - 2];
                if (canRetreatLeft &&
                    Grid[soldier.Row + 1, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty &&
                    c.Piece == CannonUtils.ISoldiers.empty)
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                    c.DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat right diagonal
                c = Grid[soldier.Row + 2, soldier.Column + 2];
                if (canRetreatRight &&
                    Grid[soldier.Row + 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty &&
                    c.Piece == CannonUtils.ISoldiers.empty)
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                    c.DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

            }
        }

        /// <summary>
        /// Cannon is a (orthogonal or diagonal) line of 3 adjacent friendly soldiers. 
        ///  - A cannon may SLIDE along its line in either direction if the target spot is empty.
        ///  - A cannon may make a capture without sliding, i.e. to "shoot" an enemy piece (either a soldier or the Town) 
        ///     standing on the same line as the shooting cannon 
        ///     if there is one or two empty points between the cannon's front soldier and the enemy piece
        /// </summary>
        private void SetCANNONMoves(Cell soldier)
        {
            // ORTHOGONAL 
            // Two soldiers at left -> slide to left
            if (2 < soldier.Column
                && Grid[soldier.Row, soldier.Column - 1].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column - 2].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column - 3].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row, soldier.Column - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
                  
            }

            // two soldiers at right -> slide to right
            if (soldier.Column < 7
                && Grid[soldier.Row, soldier.Column + 1].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column + 2].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column + 3].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row, soldier.Column + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // Two soldiers bellow -> slide bellow
            if (2 < soldier.Row
                && Grid[soldier.Row - 1, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row - 2, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row - 3, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row - 3, soldier.Column].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // two soldiers above -> slide above
            if (soldier.Row < 7
                && Grid[soldier.Row + 1, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row + 2, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row + 3, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row + 3, soldier.Column].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // DIAGONAL
            // two solidiers at diagonal upper left -> slide diagonal up left
            if (soldier.Row < 7 && soldier.Column > 2
                && Grid[soldier.Row + 1, soldier.Column - 1].Piece == soldier.Piece
                && Grid[soldier.Row + 2, soldier.Column - 2].Piece == soldier.Piece
                && Grid[soldier.Row + 3, soldier.Column - 3].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row + 3, soldier.Column - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // two soldiers at diagonal lower left 
            if (soldier.Row > 2 && soldier.Column > 2
                && Grid[soldier.Row - 1, soldier.Column - 1].Piece == soldier.Piece
                && Grid[soldier.Row - 2, soldier.Column - 2].Piece == soldier.Piece
                && Grid[soldier.Row - 3, soldier.Column - 3].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row - 3, soldier.Column - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // diagonal upper right
            if (soldier.Row < 7 && soldier.Column < 7
                && Grid[soldier.Row + 1, soldier.Column + 1].Piece == soldier.Piece
                && Grid[soldier.Row + 2, soldier.Column + 2].Piece == soldier.Piece
                && Grid[soldier.Row + 3, soldier.Column + 3].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row + 3, soldier.Column + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // diagonal lower right 
            if (soldier.Row > 2 && soldier.Column < 7
                && Grid[soldier.Row - 1, soldier.Column + 1].Piece == soldier.Piece
                && Grid[soldier.Row - 2, soldier.Column + 2].Piece == soldier.Piece
                && Grid[soldier.Row - 3, soldier.Column + 3].Piece == CannonUtils.ISoldiers.empty)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row - 3, soldier.Column + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }
        }

        private void SetSHOOTS(Cell soldier)
        {
            // ORTHOGONAL 
            // Three soldiers at left
            if (3 < soldier.Column
                && Grid[soldier.Row, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row, soldier.Column - 2].Piece == Friend
                && Grid[soldier.Row, soldier.Column - 3].Piece == Friend
                && Grid[soldier.Row, soldier.Column - 4].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // shoot right
            else if (soldier.Column < 6
                && Grid[soldier.Row, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row, soldier.Column + 2].Piece == Friend
                && Grid[soldier.Row, soldier.Column + 3].Piece == Friend
                && Grid[soldier.Row, soldier.Column + 4].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // Two soldiers bellow -> slide bellow
            else if(3 < soldier.Row
                && Grid[soldier.Row - 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row - 2, soldier.Column].Piece == Friend
                && Grid[soldier.Row - 3, soldier.Column].Piece == Friend
                && Grid[soldier.Row - 4, soldier.Column].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // two soldiers above -> slide above
            else if(soldier.Row < 6
                && Grid[soldier.Row + 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row + 2, soldier.Column].Piece == Friend
                && Grid[soldier.Row + 3, soldier.Column].Piece == Friend
                && Grid[soldier.Row + 4, soldier.Column].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // DIAGONAL
            // two solidiers at diagonal upper left -> slide diagonal up left
            else if(soldier.Row < 6 && soldier.Column > 3
                && Grid[soldier.Row + 1, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row + 2, soldier.Column - 2].Piece == Friend
                && Grid[soldier.Row + 3, soldier.Column - 3].Piece == Friend
                && Grid[soldier.Row + 4, soldier.Column - 4].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // two soldiers at diagonal lower left 
            else if(soldier.Row > 3 && soldier.Column > 3
                && Grid[soldier.Row - 1, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row - 2, soldier.Column - 2].Piece == Friend
                && Grid[soldier.Row - 3, soldier.Column - 3].Piece == Friend
                && Grid[soldier.Row - 4, soldier.Column - 4].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // diagonal upper right
            else if(soldier.Row < 6 && soldier.Column < 6
                && Grid[soldier.Row + 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row + 2, soldier.Column + 2].Piece == Friend
                && Grid[soldier.Row + 3, soldier.Column + 3].Piece == Friend
                && Grid[soldier.Row + 4, soldier.Column + 4].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // diagonal lower right 
            else if(soldier.Row > 3 && soldier.Column < 6
                && Grid[soldier.Row - 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row - 2, soldier.Column + 2].Piece == Friend
                && Grid[soldier.Row - 3, soldier.Column + 3].Piece == Friend
                && Grid[soldier.Row - 4, soldier.Column + 4].Piece == Friend)
            {
                LegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }
        }
        #endregion

        #region Utils
        public CannonUtils.ISoldiers Friend => CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers Enemy => !CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers TownEnemy => !CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;

        private void InitGrid()
        {
            int _size = CannonUtils.Size;
            Grid = new Cell[_size, _size];
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    Grid[i, j] = new Cell(i, j);
                    if (CannonUtils.RowDarkSoldiers.Contains(i) && CannonUtils.ColumnDarkSoldiers.Contains(j))
                    {
                        Grid[i, j].Piece = CannonUtils.ISoldiers.dark_soldier;
                    }
                    else if (CannonUtils.RowLightSoldiers.Contains(i) && CannonUtils.ColumnLightSoldiers.Contains(j))
                    {
                        Grid[i, j].Piece = CannonUtils.ISoldiers.light_soldier;
                    }

                }
            }
        }
        #endregion

        #region printMethods
        public void printBoard()
        {
            string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            Console.WriteLine("================================= Counter:" + TurnCounter);
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();
            int counter = 0;
            for (int i = 10 - 1; i >= 0; i--)
            {
                Console.Write(i + 1);
                Console.Write(" ");
                if (i != 9) { Console.Write(" "); }
                for (int j = 0; j < 10; j++)
                {
                    Cell c = Grid[i, j];
                    if (CannonUtils.IsOdd(TurnCounter)) 
                    {
                        // Light soldiers turn (p2)
                        if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                        else if (c.Piece == CannonUtils.ISoldiers.light_soldier) { Console.Write(counter); counter++; }
                        else if (c.Piece == CannonUtils.ISoldiers.dark_town || c.Piece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.Piece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }

                    }
                    else
                    {
                        if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write(counter); counter++; }
                        else if (c.Piece == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                        else if (c.Piece == CannonUtils.ISoldiers.dark_town || c.Piece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.Piece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
                    }
                    if (j != 9) { Console.Write(" - "); }

                }
                Console.WriteLine();
                if (i != 0)
                {
                    Console.Write(" ");
                    for (int j = 0; j < 10; j++)
                    {
                        Console.Write("  ");
                        Console.Write("| ");
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
            Console.WriteLine("Soldiers legend: X = Dark Soldier, O = Light Soldier, T = Town, · = empty cell");
            Console.WriteLine("=================================");

        }

        public void printBoardWithMoves(Cell chosen) 
        {
            string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            Console.WriteLine("================================= Counter:" + TurnCounter);
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();

            List<Move> list = LegalMoves.Where(x => x.OldCell.Row == chosen.Row && x.OldCell.Column == chosen.Column).ToList();
            for (int i = 10 - 1; i >= 0; i--)
            {
                Console.Write(i + 1);
                Console.Write(" ");
                if (i != 9) { Console.Write(" "); }
                for (int j = 0; j < 10; j++)
                {
                    Cell c = Grid[i, j];
                    Move mm = list.Where(x => x.NewCell.Row == c.Row && x.NewCell.Column == c.Column).FirstOrDefault();
                    if (c == chosen) { Console.Write("S"); }
                    else if (mm != null)
                    {
                        if (mm.Type == CannonUtils.IMoves.step) { Console.Write("m"); }
                        else if (mm.Type == CannonUtils.IMoves.retreat) { Console.Write("r"); }
                        else if (mm.Type == CannonUtils.IMoves.capture) { Console.Write("k"); }
                        else if (mm.Type == CannonUtils.IMoves.slideCannon) { Console.Write("s"); }
                    }
                    else if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                    else if (c.Piece == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                    else if (c.Piece == CannonUtils.ISoldiers.dark_town || c.Piece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                    else if (c.Piece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
                    if (j != 9) { Console.Write(" - "); }

                }
                Console.WriteLine();
                if (i != 0)
                {
                    Console.Write(" ");
                    for (int j = 0; j < 10; j++)
                    {
                        Console.Write("  ");
                        Console.Write("| ");
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
            Console.WriteLine("Soldiers legend: X = Dark Soldier, O = Light Soldier, T = Town, · = empty cell");
            Console.WriteLine("Available Moves legend: m = step, r = retreat, k = capture, s = slide");
            Console.WriteLine("=================================");

        }

        public void printSoldiers()
        {
            Console.WriteLine("List of soldiers: ");
            int count = 0;
            foreach (Cell s in Grid)
            {
                if (s.Piece == CannonUtils.ISoldiers.dark_soldier ||
                    s.Piece == CannonUtils.ISoldiers.light_soldier ||
                    s.Piece == CannonUtils.ISoldiers.dark_town ||
                    s.Piece == CannonUtils.ISoldiers.light_town)
                {
                    Console.WriteLine(count+ " - Soldier at: (" + s.Row + "," + s.Column+ ")");
                    count++;
                }
            }
        }

        public void printLegalMoves(List<Move> moves)
        {
            int count = 0;
            Console.WriteLine("List of legal moves: ");
            foreach (Move move in moves)
            {
                printMove(move, count);
                count++;
            }
        }

        private void printMove(Move move, int count)
        {
            switch (move.Type)
            {
                case CannonUtils.IMoves.step:
                    Console.WriteLine(count + "     - Soldier steps from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column+")");
                    break;
                case CannonUtils.IMoves.capture:
                    Console.WriteLine(count + "     - Soldier captures from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
                case CannonUtils.IMoves.retreat:
                    Console.WriteLine(count + "     - Soldier retreats from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
                case CannonUtils.IMoves.slideCannon:
                    Console.WriteLine(count + "     - Soldier slides from (" + move.OldCell.Row + "," + move.OldCell.Column + ") to (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
                case CannonUtils.IMoves.shootCannon:
                    Console.WriteLine(count + "     - Shoot soldier at cell (" + move.NewCell.Row + "," + move.NewCell.Column + ")");
                    break;
            }
        }

        public void printNextPlayer()
        {
            if (CannonUtils.IsOdd(TurnCounter)) { Console.WriteLine("Light soldiers turn (p2)"); }
            else { Console.WriteLine("Dark soldiers turn (p1)"); }
        }
        #endregion
    }
}
