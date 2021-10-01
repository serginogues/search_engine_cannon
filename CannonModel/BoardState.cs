using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CannonModel
{
    /// <summary>
    /// Board coordinates follow this code: http://www.iggamecenter.com/info/en/cannon.html
    /// That is, position 9-B = Grid[8,1]
    /// Game States:
    /// - A:
    ///     - given current TURN, return AVAILABLE_SOLDIERS (cells) and AVAILABLE_SHOTS (cells). 
    ///     - Now USER has to:
    ///         - click (choose) CURRENT_SOLDIER from AVAILABLE_SOLDIERS to play with and go to B
    ///         OR
    ///         - click a CURRENT_SHOT and go to E
    /// - B: 
    ///     - A CURRENT_SOLDIER is selected and game logic returns AVAILABLE_MOVES (cells).
    ///     - USER can go to A by executing C.
    ///     - USER can go to D by clicking a MOVE (cell) from AVAILABLE_MOVES.
    /// - C:
    ///     - USER wants to choose another CURRENT_SOLDIER so we need to go back to A.
    ///     - Reset AVAILABLE_MOVES and CURRENT_SOLDIER.
    ///     - Go to A.
    /// - D:
    ///     - USER has chosen MOVE and game logic has to execute it.
    ///     - Update CURRENT_SOLDIER (cell) and MOVE (cell) values and do TURN++.
    ///     - Go to C.
    /// - E:
    ///     - update CURRENT_SHOT (cell) and do TURN++.
    ///     - Go to C.
    /// </summary>
    public class BoardState
    {

        #region Properties
        public int TurnCounter { get; set; }
        public Cell[,] Grid { get; set; }
        public Cell ChosenSoldier { get; set; }
        public List<BoardState> ChildrenList { get; set; }

        public BoardState ()
        {
            TurnCounter = 0;
            ChosenSoldier = new Cell(0, 0);

            int _size = CannonUtils.Size;
            Grid = new Cell[_size, _size];
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    Grid[i, j] = new Cell(i, j);
                    if (CannonUtils.RowDarkSoldiers.Contains(i) && CannonUtils.ColumnDarkSoldiers.Contains(j))
                    {
                        Grid[i, j].CurrentValue = CannonUtils.ISoldiers.dark_soldier;
                    }
                    else if (CannonUtils.RowLightSoldiers.Contains(i) && CannonUtils.ColumnLightSoldiers.Contains(j))
                    {
                        Grid[i, j].CurrentValue = CannonUtils.ISoldiers.light_soldier;
                    }
                    
                }
            }
        }
        #endregion

        #region Children
        /// <summary>
        /// To make children from current BoardState and change their properties without referencing its parent
        /// </summary>
        public BoardState DeepCopy()
        {
            BoardState other = (BoardState)this.MemberwiseClone();
            other.ChildrenList = new List<BoardState>(ChildrenList);
            other.ChosenSoldier = new Cell(ChosenSoldier.Row, ChosenSoldier.Column)
            {
                CurrentValue = ChosenSoldier.CurrentValue,
                Action = ChosenSoldier.Action
            };
            other.TurnCounter = TurnCounter;
            other.Grid = new Cell[CannonUtils.Size, CannonUtils.Size];
            for (int i = 0; i < CannonUtils.Size; i++)
            {
                for (int j = 0; j < CannonUtils.Size; j++)
                {
                    other.Grid[i, j] = new Cell(Grid[i, j].Row, Grid[i, j].Column)
                    {
                        CurrentValue = Grid[i, j].CurrentValue,
                        Action = Grid[i, j].Action
                    };
                }
            }

            return other;
        }

        /// <summary>
        /// Given current TURN, set AVAILABLE_SOLDIERS (cells) and AVAILABLE_SHOTS (cells). 
        /// </summary>
        public void A_SetLegalMoves()
        {
            ChildrenList = new List<BoardState>();
            SoldierLegalMoves();
            SetAvailableSHOOTS();
            ShootLegalMoves();
        }

        private void SoldierLegalMoves()
        {
            // For each cell
            foreach (Cell soldier in Grid)
            {
                // Every soldier gives legal moves
                if (soldier.CurrentValue == CurrentFriend)
                {
                    BoardState clonedBoard = DeepCopy();
                    // Select Soldier and Set legal moves
                    clonedBoard.StateManager(CannonUtils.IUserActions.click_soldier, soldier.Row, soldier.Column);

                    foreach (Cell move in clonedBoard.Grid)
                    {
                        if (move.Action == CannonUtils.IMoves.step ||
                            move.Action == CannonUtils.IMoves.capture ||
                            move.Action == CannonUtils.IMoves.retreat ||
                            move.Action == CannonUtils.IMoves.slideCannon)
                        {
                            BoardState child = clonedBoard.DeepCopy();
                            //child.StateManager(CannonUtils.IUserActions.click_moves, move.Row, move.Column);
                            child.D_MoveSoldier(move.Row, move.Column);
                            ChildrenList.Add(child);
                        }
                    }
                }
            }
        }

        private void ShootLegalMoves()
        {
            foreach (Cell move in Grid)
            {
                if (move.Action == CannonUtils.IMoves.shootCannon)
                {
                    BoardState child = DeepCopy();
                    child.StateManager(CannonUtils.IUserActions.click_shoot, move.Row, move.Column);
                    ChildrenList.Add(child);
                }
            }
        }
        #endregion

        #region State methods
        /// <summary>
        /// click_soldier           -> row,col are new chosen soldier
        /// click_shoot             -> row,col are eliminated cell by cannon's shoot
        /// reset_available_moves   -> user resets                      after doing click_soldier
        /// click_moves             -> row,col are chosen move (cell)   after doing click_soldier
        /// </summary>
        public void StateManager(CannonUtils.IUserActions action, int row, int col)
        {
            switch (action)
            {
                case CannonUtils.IUserActions.click_soldier:
                    B_AssignLegalMovesToCells(row, col);
                    break;
                case CannonUtils.IUserActions.click_shoot:
                    E_ShootEnemy(row, col);
                    break;
                case CannonUtils.IUserActions.reset_available_moves:
                    C_ResetStateBoard();
                    break;
                case CannonUtils.IUserActions.click_moves:
                    D_MoveSoldier(row, col);
                    break;
            }
        }

        

        /// <summary>
        /// A CURRENT_SOLDIER is selected and game logic returns AVAILABLE_MOVES (cells)
        /// </summary>
        private void B_AssignLegalMovesToCells(int row, int col)
        {
            // Set selected soldier
            ChosenSoldier = Grid[row, col];

            // Set available moves
            SetSTEPMoves();
            SetRETREATMoves();
            SetCAPTUREMoves();
            SetCANNONMoves();
        }

        /// <summary>
        /// Reset AVAILABLE_MOVES and CURRENT_SOLDIER
        /// </summary>
        private void C_ResetStateBoard()
        {
            ChosenSoldier = new Cell(0, 0);
            foreach (Cell item in Grid)
            {
                item.Action = CannonUtils.IMoves.none;
            }
        }

        /// <summary>
        /// Update CURRENT_SOLDIER (cell) and MOVE (cell) values and do TURN++
        /// </summary>
        private void D_MoveSoldier(int row, int col)
        {
            Grid[row, col].CurrentValue = ChosenSoldier.CurrentValue;
            Grid[ChosenSoldier.Row, ChosenSoldier.Column].CurrentValue = CannonUtils.ISoldiers.empty;
            TurnCounter++;
            C_ResetStateBoard();
        }

        /// <summary>
        /// Update CURRENT_SHOT (cell) and do TURN++
        /// </summary>
        private void E_ShootEnemy(int row, int col)
        {
            Grid[row, col].CurrentValue = CannonUtils.ISoldiers.empty;
            TurnCounter++;
            C_ResetStateBoard();
        }
        #endregion

        #region Moves
        /// <param name="_column"> Column to be added the town </param>
        /// <param name="_color"> True == Dark,  False == Light </param>
        public void AddTown(int _column, CannonUtils.ISoldiers _color)
        {
            if (_color == CannonUtils.ISoldiers.dark_soldier) { Grid[0, _column].CurrentValue = CannonUtils.ISoldiers.dark_town; }
            else if (_color == CannonUtils.ISoldiers.light_soldier) { Grid[9, _column].CurrentValue = CannonUtils.ISoldiers.light_town; }
            TurnCounter++;
        }

        /// <summary>
        /// A soldier may move one step forward or diagonally forward to an adjacent empty point
        /// </summary>
        private void SetSTEPMoves()
        {
            if (ChosenSoldier.CurrentValue == CannonUtils.ISoldiers.dark_soldier)
            {
                // i to i+1
                foreach (Cell cell in GetNeighbours(ChosenSoldier).Where(x => ChosenSoldier.Row < x.Row && x.CurrentValue == CannonUtils.ISoldiers.empty).ToList())
                {
                    cell.Action = CannonUtils.IMoves.step;
                }
            }
            else
            {
                // i to i-1
                foreach (Cell cell in GetNeighbours(ChosenSoldier).Where(x => ChosenSoldier.Row > x.Row && x.CurrentValue == CannonUtils.ISoldiers.empty).ToList())
                {
                    cell.Action = CannonUtils.IMoves.step;
                }

            }
        }

        /// <summary>
        /// A soldier can retreat two points backwards or diagonally backwards 
        /// if it is adjacent to an enemy soldier and if the target and intermediate spots are empty
        /// </summary>
        private void SetRETREATMoves()
        {
            if (IsEnemyNeighbour(ChosenSoldier))
            {
                bool canRetreatLeft = ChosenSoldier.Column > 1 ? true : false;
                bool canRetreatRight = ChosenSoldier.Column < 8 ? true : false;
                if (ChosenSoldier.CurrentValue == CannonUtils.ISoldiers.dark_soldier && ChosenSoldier.Row > 1)
                {
                    // i to i-1
                    // retreat backward
                    if (Grid[ChosenSoldier.Row - 1, ChosenSoldier.Column].CurrentValue == CannonUtils.ISoldiers.empty &&
                        Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column].CurrentValue == CannonUtils.ISoldiers.empty)
                    {
                        Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column].Action = CannonUtils.IMoves.retreat;
                    }
                    
                    // retreat left diagonal
                    if (canRetreatLeft &&
                        Grid[ChosenSoldier.Row - 1, ChosenSoldier.Column-1].CurrentValue == CannonUtils.ISoldiers.empty &&
                        Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column-2].CurrentValue == CannonUtils.ISoldiers.empty)
                    {
                        Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column - 2].Action = CannonUtils.IMoves.retreat;
                    }

                    // retreat right diagonal
                    if (canRetreatRight &&
                        Grid[ChosenSoldier.Row - 1, ChosenSoldier.Column + 1].CurrentValue == CannonUtils.ISoldiers.empty &&
                        Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column + 2].CurrentValue == CannonUtils.ISoldiers.empty)
                    {
                        Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column + 2].Action = CannonUtils.IMoves.retreat;
                    }

                }
                else if (ChosenSoldier.CurrentValue == CannonUtils.ISoldiers.light_soldier && ChosenSoldier.Row < 8)
                {
                    // i to i+1
                    // retreat backward
                    if (Grid[ChosenSoldier.Row + 1, ChosenSoldier.Column].CurrentValue == CannonUtils.ISoldiers.empty &&
                        Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column].CurrentValue == CannonUtils.ISoldiers.empty)
                    {
                        Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column].Action = CannonUtils.IMoves.retreat;
                    }

                    // retreat left diagonal
                    if (canRetreatLeft &&
                        Grid[ChosenSoldier.Row + 1, ChosenSoldier.Column - 1].CurrentValue == CannonUtils.ISoldiers.empty &&
                        Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column - 2].CurrentValue == CannonUtils.ISoldiers.empty)
                    {
                        Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column - 2].Action = CannonUtils.IMoves.retreat;
                    }

                    // retreat right diagonal
                    if (canRetreatRight &&
                        Grid[ChosenSoldier.Row + 1, ChosenSoldier.Column + 1].CurrentValue == CannonUtils.ISoldiers.empty &&
                        Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column + 2].CurrentValue == CannonUtils.ISoldiers.empty)
                    {
                        Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column + 2].Action = CannonUtils.IMoves.retreat;
                    }

                }
            }
        }

        /// <summary>
        /// A soldier may capture an enemy piece (a soldier or the Town) standing on an adjacent point 
        /// by moving one step sideways, forward or diagonally forward
        /// </summary>
        private void SetCAPTUREMoves()
        {
            if (ChosenSoldier.CurrentValue == CannonUtils.ISoldiers.dark_soldier)
            {
                foreach (Cell item in GetNeighbours(ChosenSoldier).Where(x => IsEnemy(ChosenSoldier, x) && ChosenSoldier.Row <= x.Row).ToList())
                {
                    item.Action = CannonUtils.IMoves.capture;
                }
            }
            else if (ChosenSoldier.CurrentValue == CannonUtils.ISoldiers.light_soldier)
            {
                foreach (Cell item in GetNeighbours(ChosenSoldier).Where(x => IsEnemy(ChosenSoldier, x) && ChosenSoldier.Row >= x.Row).ToList())
                {
                    item.Action = CannonUtils.IMoves.capture;
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
        private void SetCANNONMoves()
        {
            // ORTHOGONAL 
            // Two soldiers at left -> slide to left
            if (2 < ChosenSoldier.Column
                && Grid[ChosenSoldier.Row, ChosenSoldier.Column - 1].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row, ChosenSoldier.Column - 2].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row, ChosenSoldier.Column - 3].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row, ChosenSoldier.Column - 3].Action = CannonUtils.IMoves.slideCannon;
            }

            // two soldiers at right -> slide to right
            if (ChosenSoldier.Column < 7
                && Grid[ChosenSoldier.Row, ChosenSoldier.Column + 1].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row, ChosenSoldier.Column + 2].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row, ChosenSoldier.Column + 3].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row, ChosenSoldier.Column + 3].Action = CannonUtils.IMoves.slideCannon;
            }

            // Two soldiers bellow -> slide bellow
            if (2 < ChosenSoldier.Row
                && Grid[ChosenSoldier.Row - 1, ChosenSoldier.Column].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row - 3, ChosenSoldier.Column].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row - 3, ChosenSoldier.Column].Action = CannonUtils.IMoves.slideCannon;
            }

            // two soldiers above -> slide above
            if (ChosenSoldier.Row < 7
                && Grid[ChosenSoldier.Row + 1, ChosenSoldier.Column].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row + 3, ChosenSoldier.Column].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row + 3, ChosenSoldier.Column].Action = CannonUtils.IMoves.slideCannon;
            }

            // DIAGONAL
            // two solidiers at diagonal upper left -> slide diagonal up left
            if (ChosenSoldier.Row < 7 && ChosenSoldier.Column > 2
                && Grid[ChosenSoldier.Row + 1, ChosenSoldier.Column - 1].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column - 2].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row + 3, ChosenSoldier.Column - 3].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row + 3, ChosenSoldier.Column - 3].Action = CannonUtils.IMoves.slideCannon;
            }

            // two soldiers at diagonal lower left 
            if (ChosenSoldier.Row > 2 && ChosenSoldier.Column > 2
                && Grid[ChosenSoldier.Row - 1, ChosenSoldier.Column - 1].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column - 2].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row - 3, ChosenSoldier.Column - 3].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row - 3, ChosenSoldier.Column - 3].Action = CannonUtils.IMoves.slideCannon;
            }

            // diagonal upper right
            if (ChosenSoldier.Row < 7 && ChosenSoldier.Column < 7
                && Grid[ChosenSoldier.Row + 1, ChosenSoldier.Column + 1].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row + 2, ChosenSoldier.Column + 2].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row + 3, ChosenSoldier.Column + 3].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row + 3, ChosenSoldier.Column + 3].Action = CannonUtils.IMoves.slideCannon;
            }

            // diagonal lower right 
            if (ChosenSoldier.Row > 2 && ChosenSoldier.Column < 7
                && Grid[ChosenSoldier.Row - 1, ChosenSoldier.Column + 1].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row - 2, ChosenSoldier.Column + 2].CurrentValue == ChosenSoldier.CurrentValue
                && Grid[ChosenSoldier.Row - 3, ChosenSoldier.Column + 3].CurrentValue == CannonUtils.ISoldiers.empty)
            {
                Grid[ChosenSoldier.Row - 3, ChosenSoldier.Column + 3].Action = CannonUtils.IMoves.slideCannon;
            }
        }

        private void SetAvailableSHOOTS()
        {
            // In order to find the cells that correspond to available shoots, we need first to check the available cannons (lines of 3 soldiers).

            // Create a list of the soldier cells with which to work better
            List<Cell> soldier_list = new List<Cell>();
            bool is_odd = CannonUtils.IsOdd(TurnCounter);
            foreach (Cell item in Grid)
            {
                // Even counter -> Dark Soldiers (Player 1)
                // Odd counter ->  Light soldiers
                if (!is_odd && item.CurrentValue == CannonUtils.ISoldiers.dark_soldier) { soldier_list.Add(item); }
                else if (is_odd && item.CurrentValue == CannonUtils.ISoldiers.light_soldier) { soldier_list.Add(item); }
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
                    if (1 < col0 && Grid[row0, col0-1].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0, col0 - 2]))
                    {
                        Grid[row0, col0 - 2].Action = CannonUtils.IMoves.shootCannon;
                    }

                    // if there is an empty cell at their right and an enemy two cells, this is a shoot cell
                    if (col2 < 8 && Grid[row0, col2 + 1].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0, col2 + 2]))
                    {
                        Grid[row0, col2 + 2].Action = CannonUtils.IMoves.shootCannon;
                    }
                }
                // if the three column values are the same and the row values are a sequence, this is a vertical cannon
                else if (col0 == col1 && col1 == col2 && row0 + 1 == row1 && row1 + 1 == row2)
                {
                    // if there is an empty cell at their left and an enemy two cells at their left, this is a shoot cell
                    if (1 < row0 && Grid[row0 - 1, col0].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0 - 2, col0]))
                    {
                        Grid[row0 - 2, col0].Action = CannonUtils.IMoves.shootCannon;
                    }

                    // if there is an empty cell at their right and an enemy two cells, this is a shoot cell
                    if (row2 < 8 && Grid[row2 + 1, col2].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row2+ 2, col2]))
                    {
                        Grid[row2 + 2, col2].Action = CannonUtils.IMoves.shootCannon;
                    }
                }

                // if both column and row values are a sequence, this is a diagonal cannon
                // left - up -> right-down
                else if (col0 + 1 == col1 && col1 + 1 == col2 && row0 == row1 + 1 && row1 == row2 + 1)
                {
                    // shoot upper left
                    if (1 < col0 && row0 < 8 && Grid[row0 + 1, col0-1].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0 + 2, col0 - 2]))
                    {
                        Grid[row0 + 2, col0 - 2].Action = CannonUtils.IMoves.shootCannon;
                    }

                    // shoot bellow right
                    if (col2 < 8 && 1 < row2 && Grid[row2 - 1, col2 + 1].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row2 - 2, col2 + 2]))
                    {
                        Grid[row2 - 2, col2 + 2].Action = CannonUtils.IMoves.shootCannon;
                    }
                }
                // left-down -> right-up
                else if (col0 + 1 == col1 && col1 + 1 == col2 && row0 + 1 == row1 && row1 + 1 == row2)
                {
                    // shoot bellow left
                    if (1 < col0 && 1 < row0 && Grid[row0 - 1, col0 - 1].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row0 - 2, col0 - 2]))
                    {
                        Grid[row0 - 2, col0 - 2].Action = CannonUtils.IMoves.shootCannon;
                    }

                    // shoot upper right
                    if (col2 < 8 && row2 < 8 && Grid[row2 + 1, col2 + 1].CurrentValue == CannonUtils.ISoldiers.empty && IsEnemy(perm.ElementAt(0), Grid[row2 + 2, col2 + 2]))
                    {
                        Grid[row2 + 2, col2 + 2].Action = CannonUtils.IMoves.shootCannon;
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
            if (_soldier.CurrentValue == CannonUtils.ISoldiers.dark_soldier &&
                (possibleEnemy.CurrentValue == CannonUtils.ISoldiers.light_soldier || possibleEnemy.CurrentValue == CannonUtils.ISoldiers.light_town)) { return true; }
            else if (_soldier.CurrentValue == CannonUtils.ISoldiers.light_soldier &&
                (possibleEnemy.CurrentValue == CannonUtils.ISoldiers.dark_soldier || possibleEnemy.CurrentValue == CannonUtils.ISoldiers.dark_town)) { return true; }
            else { return false; }
        }

        public CannonUtils.ISoldiers CurrentFriend => CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;

        public bool TerminalState()
        {
            if (TurnCounter == 100) { return true; }
            else { return false; }
        }

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
                        if (c.CurrentValue == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                        else if (c.CurrentValue == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                        else if (c.CurrentValue == CannonUtils.ISoldiers.dark_town || c.CurrentValue == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.CurrentValue == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
                    }
                    else
                    {
                        if (c == ChosenSoldier) { Console.Write("S"); }
                        else if (c.Action == CannonUtils.IMoves.step) { Console.Write("m"); }
                        else if (c.Action == CannonUtils.IMoves.retreat) { Console.Write("r"); }
                        else if (c.Action == CannonUtils.IMoves.capture) { Console.Write("k"); }
                        else if (c.Action == CannonUtils.IMoves.slideCannon) { Console.Write("s"); }
                        else if (c.CurrentValue == CannonUtils.ISoldiers.dark_soldier) { Console.Write("X"); }
                        else if (c.CurrentValue == CannonUtils.ISoldiers.light_soldier) { Console.Write("O"); }
                        else if (c.CurrentValue == CannonUtils.ISoldiers.dark_town || c.CurrentValue == CannonUtils.ISoldiers.light_town) { Console.Write("T"); }
                        else if (c.CurrentValue == CannonUtils.ISoldiers.empty) { Console.Write("·"); }
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

        public void printAvailableShoots()
        {
            Console.Write("Available shoots: ");
            foreach (Cell item in Grid)
            {
                if (item.Action == CannonUtils.IMoves.shootCannon)
                {
                    int nu = item.Row + 1;
                    Console.Write("(" + nu + "," + CannonUtils.ColumnInt_To_Char(item.Column) + ")");
                }
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
