using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

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
        public List<Cell> SoldierList { get; set; }
        public List<Move> FriendLegalMoves { get; set; }
        /// <summary>
        /// List of moves from root to this state of the board
        /// </summary>
        public List<Move> History { get; set; }
        /// <summary>
        /// if node is terminal node, then return its terminalNode value
        /// </summary>
        public CannonUtils.INode TerminalState { get; set; }
        public Cell DarkTown { get; set; }
        public Cell LightTown { get; set; } 
        #endregion

        #region Constructor (root node)
        /// <summary>
        /// Called only once at root node
        /// </summary>
        public void root_init()
        {
            History = new List<Move>();
            TurnCounter = 0;
            initGrid();
            initLegalMoves();
            TerminalState = CannonUtils.INode.leaf;
        }

        /// <summary>
        /// Initialize [Grid];
        /// Assign soldiers to its position;
        /// Initialize [soldierList];
        /// </summary>
        private void initGrid()
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

        #region Make move
        /// <summary>
        /// Generate child based on move_id reference
        /// </summary>
        public BoardState Successor(int move_id)
        {
            // create [child] that is exactly the same as [this]
            BoardState child = DeepCopy();

            // move soldiers in [child] based on [move_id]
            Move move = child.FriendLegalMoves[move_id];
            switch (move.Type)
            {
                case CannonUtils.IMoves.shootCannon:
                    child.IsTerminalMove(move);
                    child.Grid[move.NewCell.Row, move.NewCell.Column].Piece = CannonUtils.ISoldiers.empty;
                    break;
                case CannonUtils.IMoves.step:
                case CannonUtils.IMoves.retreat:
                case CannonUtils.IMoves.slideCannon:
                    child.Grid[move.NewCell.Row, move.NewCell.Column].Piece = move.OldCell.Piece;
                    child.Grid[move.OldCell.Row, move.OldCell.Column].Piece = CannonUtils.ISoldiers.empty;
                    break;
                case CannonUtils.IMoves.capture:
                    child.IsTerminalMove(move);
                    child.Grid[move.NewCell.Row, move.NewCell.Column].Piece = move.OldCell.Piece;
                    child.Grid[move.OldCell.Row, move.OldCell.Column].Piece = CannonUtils.ISoldiers.empty;
                    break;
            }

            // update history and counter
            child.History.Add(move);
            child.TurnCounter++;

            // update LegalMoves
            child.initLegalMoves();
            child.LegalMovesLeft();

            // TODO: check if child is terminal node
            return child;
        }

        /// <summary>
        /// To make children from current BoardState and change their properties without referencing its parent;
        /// Copied parameters: History, Grid, TurnCounter, LegalMoves
        /// </summary>
        public BoardState DeepCopy()
        {
            BoardState other = (BoardState)this.MemberwiseClone();
            other.FriendLegalMoves = new List<Move>(FriendLegalMoves);
            other.History = new List<Move>(History);
            other.TurnCounter = TurnCounter;
            other.Grid = new Cell[CannonUtils.Size, CannonUtils.Size];
            other.TerminalState = CannonUtils.INode.leaf;
            for (int i = 0; i < CannonUtils.Size; i++)
            {
                for (int j = 0; j < CannonUtils.Size; j++)
                {
                    other.Grid[i, j] = Grid[i, j].DeepCopy();
                }
            }
            return other;
        }

        private void IsTerminalMove(Move move)
        {
            if(move.NewCell.Piece == CannonUtils.ISoldiers.dark_town)
            {
                // light wins
                 TerminalState = CannonUtils.INode.light_wins;
            }
            else if (move.NewCell.Piece == CannonUtils.ISoldiers.light_town)
            {
                // dark wins
                TerminalState = CannonUtils.INode.dark_wins;
            }
        }

        private void LegalMovesLeft()
        {
            if (FriendLegalMoves.Count == 0)
            {
                if (Friend == CannonUtils.ISoldiers.dark_soldier)
                {
                    // dark loses
                    TerminalState = CannonUtils.INode.light_wins;
                }
                else if (Friend == CannonUtils.ISoldiers.light_soldier)
                {
                    // light loses
                    TerminalState = CannonUtils.INode.dark_wins;
                }
            }
        }
        #endregion

        #region Moves
        /// <summary>
        /// Reset AVAILABLE_MOVES and CURRENT_SOLDIER
        /// </summary>
        public void initLegalMoves()
        {
            SoldierList = new List<Cell>();
            FriendLegalMoves = new List<Move>();

            // fill LegalMoves list
            foreach (Cell soldier in Grid)
            {
                if (soldier.Piece == Friend) 
                {
                    SoldierList.Add(soldier);
                    stepCaptureRetreatMoves(soldier);
                    slideMoves(soldier);
                }
                else if (soldier.Piece == Enemy || soldier.Piece == TownEnemy) 
                {
                    SoldierList.Add(soldier);
                    shootMoves(soldier); 
                }
            }

            // Move Ordering based on IMove enum type
            FriendLegalMoves = FriendLegalMoves.OrderByDescending(o => (int)o.Type).ToList();
        }
        
        /// <param name="_column"> Column to be added the town </param>
        /// <param name="_color"> True == Dark,  False == Light </param>
        public void AddTown(int _column, CannonUtils.ISoldiers _color)
        {
            if (_color == CannonUtils.ISoldiers.dark_soldier) 
            { 
                Grid[0, _column].Piece = CannonUtils.ISoldiers.dark_town;
                DarkTown = Grid[0, _column];
            }
            else if (_color == CannonUtils.ISoldiers.light_soldier) 
            { 
                Grid[9, _column].Piece = CannonUtils.ISoldiers.light_town;
                LightTown = Grid[9, _column];
            }
            TurnCounter++;
        }

        /// <summary>
        /// A soldier may move one STEP forward or diagonally forward to an adjacent empty point.
        /// A soldier may CAPTURE an enemy piece (a soldier or the Town) standing on an adjacent point 
        /// by moving one step sideways, forward or diagonally forward
        /// </summary>
        private void stepCaptureRetreatMoves(Cell soldier)
        {
            bool retreat = false;
            if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
            {
                if(soldier.Row < 9)
                {
                    // (i+1, j)
                    // capture or step
                    Cell c = Grid[soldier.Row + 1, soldier.Column];
                    if (StepCaptureRetreat(soldier, c)) { retreat = true; }

                    // (i+1, j-1)
                    if (soldier.Column > 0)
                    {
                        c = Grid[soldier.Row + 1, soldier.Column - 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }

                    // (i+1, j+1)
                    if (soldier.Column < 9)
                    {
                        c = Grid[soldier.Row + 1, soldier.Column + 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }
                }
                // (i, j-1)
                if (soldier.Column > 0)
                {
                    Cell c = Grid[soldier.Row, soldier.Column - 1];
                    if (c.Piece == Enemy || c.Piece == TownEnemy)
                    {
                        // capture or retreat
                        if (soldier.Row > 1)
                        {
                            retreat = true;
                        }
                        FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
                }

                // (i, j+1)
                if (soldier.Column < 9)
                {
                    Cell c = Grid[soldier.Row, soldier.Column + 1];
                    if (c.Piece == Enemy || c.Piece == TownEnemy)
                    {
                        // capture or retreat
                        if (soldier.Row > 1)
                        {
                            retreat = true;
                        }
                        FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
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
                    if (StepCaptureRetreat(soldier, c)) { retreat = true; }

                    // (i-1, j-1)
                    if (soldier.Column > 0)
                    {
                        c = Grid[soldier.Row - 1, soldier.Column - 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }

                    // (i-1, j+1)
                    if (soldier.Column < 9)
                    {
                        c = Grid[soldier.Row - 1, soldier.Column + 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }
                }
                // (i, j-1)
                if (soldier.Column > 0)
                {
                    Cell c = Grid[soldier.Row, soldier.Column - 1];
                    if (c.Piece == Enemy || c.Piece == TownEnemy)
                    {
                        // capture or retreat
                        if (soldier.Row < 8)
                        {
                            retreat = true;
                        }
                        FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
                }

                // (i, j+1)
                if (soldier.Column < 9)
                {
                    Cell c = Grid[soldier.Row, soldier.Column + 1];
                    if (c.Piece == Enemy || c.Piece == TownEnemy)
                    {
                        // capture or retreat
                        if (soldier.Row < 8)
                        {
                            retreat = true;
                        }
                        FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
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
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
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
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
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
                if (Grid[soldier.Row - 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty &&
                    Grid[soldier.Row - 2, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
                {
                    FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                            Grid[soldier.Row - 2, soldier.Column].DeepCopy(), 
                                            CannonUtils.IMoves.retreat));
                }

                // retreat left diagonal
                if (canRetreatLeft &&
                    Grid[soldier.Row - 1, soldier.Column-1].Piece == CannonUtils.ISoldiers.empty &&
                    Grid[soldier.Row - 2, soldier.Column - 2].Piece == CannonUtils.ISoldiers.empty)
                {
                    FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                    Grid[soldier.Row - 2, soldier.Column - 2].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat right diagonal
                if (canRetreatRight &&
                    Grid[soldier.Row - 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty &&
                    Grid[soldier.Row - 2, soldier.Column + 2].Piece == CannonUtils.ISoldiers.empty)
                {
                    FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                    Grid[soldier.Row - 2, soldier.Column + 2].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

            }
            else if (soldier.Piece == CannonUtils.ISoldiers.light_soldier)
            {
                // i to i+1
                // retreat backward
                if (Grid[soldier.Row + 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty &&
                    Grid[soldier.Row + 2, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
                {
                    FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                    Grid[soldier.Row + 2, soldier.Column].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat left diagonal
                if (canRetreatLeft &&
                    Grid[soldier.Row + 1, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty &&
                    Grid[soldier.Row + 2, soldier.Column - 2].Piece == CannonUtils.ISoldiers.empty)
                {
                    FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                    Grid[soldier.Row + 2, soldier.Column - 2].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat right diagonal
                if (canRetreatRight &&
                    Grid[soldier.Row + 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty &&
                    Grid[soldier.Row + 2, soldier.Column + 2].Piece == CannonUtils.ISoldiers.empty)
                {
                    FriendLegalMoves.Add(new Move(soldier.DeepCopy(),
                                    Grid[soldier.Row + 2, soldier.Column + 2].DeepCopy(),
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
        private void slideMoves(Cell soldier)
        {
            // ORTHOGONAL 
            // Two soldiers at left -> slide to left
            if (2 < soldier.Column
                && Grid[soldier.Row, soldier.Column - 1].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column - 2].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column - 3].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row, soldier.Column - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
                  
            }

            // two soldiers at right -> slide to right
            if (soldier.Column < 7
                && Grid[soldier.Row, soldier.Column + 1].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column + 2].Piece == soldier.Piece
                && Grid[soldier.Row, soldier.Column + 3].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row, soldier.Column + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // Two soldiers bellow -> slide bellow
            if (2 < soldier.Row
                && Grid[soldier.Row - 1, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row - 2, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row - 3, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row - 3, soldier.Column].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // two soldiers above -> slide above
            if (soldier.Row < 7
                && Grid[soldier.Row + 1, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row + 2, soldier.Column].Piece == soldier.Piece
                && Grid[soldier.Row + 3, soldier.Column].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row + 3, soldier.Column].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // DIAGONAL
            // two solidiers at diagonal upper left -> slide diagonal up left
            if (soldier.Row < 7 && soldier.Column > 2
                && Grid[soldier.Row + 1, soldier.Column - 1].Piece == soldier.Piece
                && Grid[soldier.Row + 2, soldier.Column - 2].Piece == soldier.Piece
                && Grid[soldier.Row + 3, soldier.Column - 3].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row + 3, soldier.Column - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // two soldiers at diagonal lower left 
            if (soldier.Row > 2 && soldier.Column > 2
                && Grid[soldier.Row - 1, soldier.Column - 1].Piece == soldier.Piece
                && Grid[soldier.Row - 2, soldier.Column - 2].Piece == soldier.Piece
                && Grid[soldier.Row - 3, soldier.Column - 3].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row - 3, soldier.Column - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // diagonal upper right
            if (soldier.Row < 7 && soldier.Column < 7
                && Grid[soldier.Row + 1, soldier.Column + 1].Piece == soldier.Piece
                && Grid[soldier.Row + 2, soldier.Column + 2].Piece == soldier.Piece
                && Grid[soldier.Row + 3, soldier.Column + 3].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row + 3, soldier.Column + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // diagonal lower right 
            if (soldier.Row > 2 && soldier.Column < 7
                && Grid[soldier.Row - 1, soldier.Column + 1].Piece == soldier.Piece
                && Grid[soldier.Row - 2, soldier.Column + 2].Piece == soldier.Piece
                && Grid[soldier.Row - 3, soldier.Column + 3].Piece == CannonUtils.ISoldiers.empty)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), Grid[soldier.Row - 3, soldier.Column + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }
        }

        private void shootMoves(Cell soldier)
        {
            // ORTHOGONAL 
            // Three soldiers at left
            if (3 < soldier.Column
                && Grid[soldier.Row, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row, soldier.Column - 2].Piece == Friend
                && Grid[soldier.Row, soldier.Column - 3].Piece == Friend
                && Grid[soldier.Row, soldier.Column - 4].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // shoot right
            else if (soldier.Column < 6
                && Grid[soldier.Row, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row, soldier.Column + 2].Piece == Friend
                && Grid[soldier.Row, soldier.Column + 3].Piece == Friend
                && Grid[soldier.Row, soldier.Column + 4].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // Two soldiers bellow -> slide bellow
            else if(3 < soldier.Row
                && Grid[soldier.Row - 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row - 2, soldier.Column].Piece == Friend
                && Grid[soldier.Row - 3, soldier.Column].Piece == Friend
                && Grid[soldier.Row - 4, soldier.Column].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // two soldiers above -> slide above
            else if(soldier.Row < 6
                && Grid[soldier.Row + 1, soldier.Column].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row + 2, soldier.Column].Piece == Friend
                && Grid[soldier.Row + 3, soldier.Column].Piece == Friend
                && Grid[soldier.Row + 4, soldier.Column].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // DIAGONAL
            // two solidiers at diagonal upper left -> slide diagonal up left
            else if(soldier.Row < 6 && soldier.Column > 3
                && Grid[soldier.Row + 1, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row + 2, soldier.Column - 2].Piece == Friend
                && Grid[soldier.Row + 3, soldier.Column - 3].Piece == Friend
                && Grid[soldier.Row + 4, soldier.Column - 4].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // two soldiers at diagonal lower left 
            else if(soldier.Row > 3 && soldier.Column > 3
                && Grid[soldier.Row - 1, soldier.Column - 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row - 2, soldier.Column - 2].Piece == Friend
                && Grid[soldier.Row - 3, soldier.Column - 3].Piece == Friend
                && Grid[soldier.Row - 4, soldier.Column - 4].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // diagonal upper right
            else if(soldier.Row < 6 && soldier.Column < 6
                && Grid[soldier.Row + 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row + 2, soldier.Column + 2].Piece == Friend
                && Grid[soldier.Row + 3, soldier.Column + 3].Piece == Friend
                && Grid[soldier.Row + 4, soldier.Column + 4].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // diagonal lower right 
            else if(soldier.Row > 3 && soldier.Column < 6
                && Grid[soldier.Row - 1, soldier.Column + 1].Piece == CannonUtils.ISoldiers.empty
                && Grid[soldier.Row - 2, soldier.Column + 2].Piece == Friend
                && Grid[soldier.Row - 3, soldier.Column + 3].Piece == Friend
                && Grid[soldier.Row - 4, soldier.Column + 4].Piece == Friend)
            {
                FriendLegalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }
        }
        #endregion

        #region Enemy Friend Utils
        public CannonUtils.ISoldiers Friend => CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers Enemy => !CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers TownFriend => CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;
        public CannonUtils.ISoldiers TownEnemy => !CannonUtils.IsOdd(TurnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;
        #endregion
    }
}
