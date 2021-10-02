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

        public BoardState ()
        {
            TurnCounter = 0;
            InitGrid();
            ResetStateBoard(); 
        }
        #endregion

        #region Main methods
        /// <summary>
        /// Reset AVAILABLE_MOVES and CURRENT_SOLDIER
        /// </summary>
        private void ResetStateBoard()
        {
            LegalMoves = new List<Move>();
            // STEP, CAPTURE, SLIDE, RETREAT, ...
            // For each cell
            foreach (Cell soldier in Grid)
            {
                // Every soldier gives legal moves
                if (soldier.Piece == Friend) 
                {
                    SetSTEPMoves(soldier);
                    SetRETREATMoves(soldier);
                    SetCAPTUREMoves(soldier);
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
            TurnCounter++;
            ResetStateBoard();
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
        /// A soldier may move one step forward or diagonally forward to an adjacent empty point
        /// </summary>
        private void SetSTEPMoves(Cell soldier)
        {
            if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
            {
                // i to i+1
                foreach (Cell cell in GetNeighbours(soldier).Where(x => soldier.Row < x.Row && x.Piece == CannonUtils.ISoldiers.empty).ToList())
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), cell.DeepCopy(), CannonUtils.IMoves.step));
                }
            }
            else
            {
                // i to i-1
                foreach (Cell cell in GetNeighbours(soldier).Where(x => soldier.Row > x.Row && x.Piece == CannonUtils.ISoldiers.empty).ToList())
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), cell.DeepCopy(), CannonUtils.IMoves.step));
                }
            }
        }

        /// <summary>
        /// A soldier can retreat two points backwards or diagonally backwards 
        /// if it is adjacent to an enemy soldier and if the target and intermediate spots are empty
        /// </summary>
        private void SetRETREATMoves(Cell soldier)
        {
            if (IsEnemyNeighbour(soldier))
            {
                bool canRetreatLeft = soldier.Column > 1 ? true : false;
                bool canRetreatRight = soldier.Column < 8 ? true : false;
                if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier && soldier.Row > 1)
                {
                    // i to i-1
                    // retreat backward
                    if (Grid[soldier.Row - 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty &&
                        Grid[soldier.Row - 2, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
                    {
                        LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                                Grid[soldier.Row - 2, soldier.Column].DeepCopy(), 
                                                CannonUtils.IMoves.retreat));
                    }
                    
                    // retreat left diagonal
                    if (canRetreatLeft &&
                        Grid[soldier.Row - 1, soldier.Column-1].Piece == CannonUtils.ISoldiers.empty &&
                        Grid[soldier.Row - 2, soldier.Column-2].Piece == CannonUtils.ISoldiers.empty)
                    {
                        LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                        Grid[soldier.Row - 2, soldier.Column - 2].DeepCopy(),
                                        CannonUtils.IMoves.retreat));
                    }

                    // retreat right diagonal
                    if (canRetreatRight &&
                        Grid[soldier.Row - 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty &&
                        Grid[soldier.Row - 2, soldier.Column + 2].Piece == CannonUtils.ISoldiers.empty)
                    {
                        LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                        Grid[soldier.Row - 2, soldier.Column + 2].DeepCopy(),
                                        CannonUtils.IMoves.retreat));
                    }

                }
                else if (soldier.Piece == CannonUtils.ISoldiers.light_soldier && soldier.Row < 8)
                {
                    // i to i+1
                    // retreat backward
                    if (Grid[soldier.Row + 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty &&
                        Grid[soldier.Row + 2, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
                    {
                        LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                        Grid[soldier.Row + 2, soldier.Column].DeepCopy(),
                                        CannonUtils.IMoves.retreat));
                    }

                    // retreat left diagonal
                    if (canRetreatLeft &&
                        Grid[soldier.Row + 1, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty &&
                        Grid[soldier.Row + 2, soldier.Column - 2].Piece == CannonUtils.ISoldiers.empty)
                    {
                        LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                        Grid[soldier.Row + 2, soldier.Column - 2].DeepCopy(),
                                        CannonUtils.IMoves.retreat));
                    }

                    // retreat right diagonal
                    if (canRetreatRight &&
                        Grid[soldier.Row + 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty &&
                        Grid[soldier.Row + 2, soldier.Column + 2].Piece == CannonUtils.ISoldiers.empty)
                    {
                        LegalMoves.Add(new Move(soldier.DeepCopy(), 
                                        Grid[soldier.Row + 2, soldier.Column + 2].DeepCopy(),
                                        CannonUtils.IMoves.retreat));
                    }

                }
            }
        }

        /// <summary>
        /// A soldier may capture an enemy piece (a soldier or the Town) standing on an adjacent point 
        /// by moving one step sideways, forward or diagonally forward
        /// </summary>
        private void SetCAPTUREMoves(Cell soldier)
        {
            if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
            {
                foreach (Cell item in GetNeighbours(soldier).Where(x => IsEnemy(soldier, x) && soldier.Row <= x.Row).ToList())
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), item.DeepCopy(), CannonUtils.IMoves.capture));
                }
            }
            else if (soldier.Piece == CannonUtils.ISoldiers.light_soldier)
            {
                foreach (Cell item in GetNeighbours(soldier).Where(x => IsEnemy(soldier, x) && soldier.Row >= x.Row).ToList())
                {
                    LegalMoves.Add(new Move(soldier.DeepCopy(), item.DeepCopy(), CannonUtils.IMoves.capture));
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
                && Grid[soldier.Row + 4, soldier.Column - 4].Piece == Friend)
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

        private void SetSHOOTSDeprecated()
        {
            // In order to find the cells that correspond to available shoots, we need first to check the available cannons (lines of 3 soldiers).

            // Create a list of the soldier cells with which to work better
            List<Cell> soldier_list = new List<Cell>();
            bool is_odd = CannonUtils.IsOdd(TurnCounter);
            foreach (Cell item in Grid)
            {
                // Even counter -> Dark Soldiers (Player 1)
                // Odd counter ->  Light soldiers
                if (!is_odd && item.Piece == CannonUtils.ISoldiers.dark_soldier) { soldier_list.Add(item); }
                else if (is_odd && item.Piece == CannonUtils.ISoldiers.light_soldier) { soldier_list.Add(item); }
            }

            // For each permutation of 3 soldiers (without repetition), check if they are in line
            foreach (var perm in CannonUtils.GetPermutations(soldier_list, 3))
            {
                // perm = list of 3 soldiers  
                int row0 = perm.ElementAt(0).Row;
                int row1 = perm.ElementAt(1).Row;
                int row2 = perm.ElementAt(2).Row;
                int col0 = perm.ElementAt(0).Column;
                int col1 = perm.ElementAt(1).Column;
                int col2 = perm.ElementAt(2).Column;

                // check first orthogonal lines
                // if the three row values are the same and the column values are a sequence, this is an horizontal cannon
                if (row0 == row1 && row1 == row2 && col0 + 1 == col1 && col1 + 1 == col2)
                {
                    // if there is an empty cell at their left and an enemy two cells at their left, this is a shoot cell
                    if (1 < col0 && Grid[row0, col0-1].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0, col0 - 2]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row0, col0 - 2].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }

                    // if there is an empty cell at their right and an enemy two cells, this is a shoot cell
                    if (col2 < 8 && Grid[row0, col2 + 1].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0, col2 + 2]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row0, col2 + 2].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }
                }
                // if the three column values are the same and the row values are a sequence, this is a vertical cannon
                else if (col0 == col1 && col1 == col2 && row0 + 1 == row1 && row1 + 1 == row2)
                {
                    // if there is an empty cell at their left and an enemy two cells at their left, this is a shoot cell
                    if (1 < row0 && Grid[row0 - 1, col0].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0 - 2, col0]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row0 - 2, col0].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }

                    // if there is an empty cell at their right and an enemy two cells, this is a shoot cell
                    if (row2 < 8 && Grid[row2 + 1, col2].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row2+ 2, col2]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row2 + 2, col2].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }
                }

                // if both column and row values are a sequence, this is a diagonal cannon
                // left - up -> right-down
                else if (col0 + 1 == col1 && col1 + 1 == col2 && row0 == row1 + 1 && row1 == row2 + 1)
                {
                    // shoot upper left
                    if (1 < col0 && row0 < 8 && Grid[row0 + 1, col0-1].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0 + 2, col0 - 2]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row0 + 2, col0 - 2].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }

                    // shoot bellow right
                    if (col2 < 8 && 1 < row2 && Grid[row2 - 1, col2 + 1].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row2 - 2, col2 + 2]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row2 - 2, col2 + 2].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }
                }
                // left-down -> right-up
                else if (col0 + 1 == col1 && col1 + 1 == col2 && row0 + 1 == row1 && row1 + 1 == row2)
                {
                    // shoot bellow left
                    if (1 < col0 && 1 < row0 && Grid[row0 - 1, col0 - 1].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0 - 2, col0 - 2]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row0 - 2, col0 - 2].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }

                    // shoot upper right
                    if (col2 < 8 && row2 < 8 && Grid[row2 + 1, col2 + 1].Piece == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row2 + 2, col2 + 2]))
                    {
                        LegalMoves.Add(new Move(new Cell(row0, col0), Grid[row2 + 2, col2 + 2].DeepCopy(), CannonUtils.IMoves.shootCannon));
                    }
                }
                
            }

        }
        #endregion

        #region Utils
        /// <summary> True if enemy arround </summary>
        private bool IsEnemyNeighbour(Cell _soldier) => GetNeighbours(_soldier).Any(x => IsEnemy(_soldier, x));

        private bool IsEnemy(Cell _soldier, Cell possibleEnemy)
        {
            if (_soldier.Piece == CannonUtils.ISoldiers.dark_soldier &&
                (possibleEnemy.Piece == CannonUtils.ISoldiers.light_soldier || possibleEnemy.Piece == CannonUtils.ISoldiers.light_town)) { return true; }
            else if (_soldier.Piece == CannonUtils.ISoldiers.light_soldier &&
                (possibleEnemy.Piece == CannonUtils.ISoldiers.dark_soldier || possibleEnemy.Piece == CannonUtils.ISoldiers.dark_town)) { return true; }
            else { return false; }
        }

        public CannonUtils.ISoldiers Friend => CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers Enemy => !CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers TownEnemy => !CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;

        private List<Cell> GetNeighbours(Cell _soldier)
        {
            List<Cell> neighbours_list = new List<Cell>();

            switch (CannonUtils.BoundsState(_soldier))
            {
                case CannonUtils.IBounds.middle:
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column-1]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column+1]);

                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column + 1]);

                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column + 1]);
                    break;
                case CannonUtils.IBounds.leftBound:
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column + 1]);

                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column + 1]);

                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column + 1]);
                    break;
                case CannonUtils.IBounds.rightBound:
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column - 1]);

                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column - 1]);

                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column - 1]);
                    break;
                case CannonUtils.IBounds.upperBound:
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column + 1]);

                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column + 1]);
                    break;
                case CannonUtils.IBounds.lowerBound:
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column + 1]);

                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column + 1]);
                    break;
                case CannonUtils.IBounds.LeftDownC:
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column + 1]);
                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column + 1]);
                    break;
                case CannonUtils.IBounds.LeftUpperC:
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column + 1]);
                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column + 1]);
                    break;
                case CannonUtils.IBounds.RightDownC:
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row + 1, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column - 1]);
                    break;
                case CannonUtils.IBounds.RightUpperC:
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column]);
                    neighbours_list.Add(Grid[_soldier.Row - 1, _soldier.Column - 1]);
                    neighbours_list.Add(Grid[_soldier.Row, _soldier.Column - 1]);
                    break;
            }

            return neighbours_list;
        }


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
        public void printBoard(bool printAvailableMoves)
        {
            string[] columnChar = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            Console.WriteLine("================================= Counter:" + TurnCounter);
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < columnChar.Length; i++) { Console.Write(columnChar[i]); Console.Write("   "); }
            Console.WriteLine();

            for (int i = 10 - 1; i >= 0; i--)
            {
                Console.Write(i + 1);
                Console.Write(" ");
                if (i != 9) { Console.Write(" "); }
                for (int j = 0; j < 10; j++)
                {
                    Cell c = Grid[i, j];

                    if (!printAvailableMoves)
                    {
                        if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                        else if (c.Piece == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                        else if (c.Piece == CannonUtils.ISoldiers.dark_town || c.Piece == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.Piece == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
                    }
                    else
                    {
                        Move mov = LegalMoves.Where(x => x.NewCell.Row == i & x.NewCell.Column == j).FirstOrDefault();
                        if (c == mov.OldCell) { Console.Write("S"); }
                        else if (mov != null)
                        {
                            if (mov.Type == CannonUtils.IMoves.step) { Console.Write("m"); }
                            else if (mov.Type == CannonUtils.IMoves.retreat) { Console.Write("r"); }
                            else if (mov.Type == CannonUtils.IMoves.capture) { Console.Write("k"); }
                            else if (mov.Type == CannonUtils.IMoves.slideCannon) { Console.Write("s"); }
                        }
                        else if (c.Piece == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
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
            if (printAvailableMoves) { Console.WriteLine("Available Moves legend: m = step, r = retreat, k = capture, s = slide"); }
            Console.WriteLine("=================================");

        }

        public void printLegalMoves()
        {
            int count = 0;
            Console.WriteLine("List of legal moves: ");
            foreach (Move move in LegalMoves)
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
