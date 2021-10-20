using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace CannonModel
{
    /// <summary>
    /// Board coordinates follow this code: 
    /// http://www.iggamecenter.com/info/en/cannon.html
    ///   A B C D E F G H I J
    /// 9 · x · · · · · · · ·
    /// 8 · · · · · · · · · ·
    /// 7 · · · · · · · · · ·
    /// 6 · · · · · · · · · ·
    /// 5 · · · · · · · · · ·
    /// 4 · · · · · · · · · ·
    /// 3 · · · · · · · · · ·
    /// 2 · · · · · · · · · ·
    /// 1 · · · · · · · · · ·
    /// 0 · · · · · · · · · ·
    /// x = Grid[8,1]
    /// </summary>
    public class BoardState
    {
        #region Properties
        public int turnCounter { get; set; }
        public Cell[,] myGrid { get; set; }
        public List<Cell> soldierList { get; set; }
        public List<Move> legalMoves { get; set; }
        /// <summary>
        /// List of moves from root to this state of the board
        /// </summary>
        public List<Move> myHistory { get; set; }
        /// <summary>
        /// if node is terminal node, then return its terminalNode value
        /// </summary>
        public CannonUtils.INode terminalState { get; set; }
        public Cell darkTown { get; set; }
        public Cell lightTown { get; set; }
        #endregion

        #region Constructor (root node)
        /// <summary>
        /// Called only once at root node
        /// </summary>
        public void root_init()
        {
            myHistory = new List<Move>();
            turnCounter = 0;
            initGrid();
            initLegalMoves();
            terminalState = CannonUtils.INode.leaf;
        }

        /// <summary>
        /// Initialize [Grid];
        /// Assign soldiers to its position;
        /// Initialize [soldierList];
        /// </summary>
        private void initGrid()
        {
            int _size = CannonUtils.Size;

            myGrid = new Cell[_size, _size];
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    myGrid[i, j] = new Cell(i, j);
                    if (CannonUtils.RowDarkSoldiers.Contains(i) && CannonUtils.ColumnDarkSoldiers.Contains(j))
                    {
                        myGrid[i, j].myPiece = CannonUtils.ISoldiers.dark_soldier;
                    }
                    else if (CannonUtils.RowLightSoldiers.Contains(i) && CannonUtils.ColumnLightSoldiers.Contains(j))
                    {
                        myGrid[i, j].myPiece = CannonUtils.ISoldiers.light_soldier;
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
            Move move = child.legalMoves[move_id];
            switch (move.Type)
            {
                case CannonUtils.IMoves.shootCannon:
                    child.IsTerminalMove(move);
                    child.myGrid[move.NewCell.myRow, move.NewCell.myColumn].myPiece = CannonUtils.ISoldiers.empty;
                    break;
                case CannonUtils.IMoves.step:
                case CannonUtils.IMoves.retreat:
                case CannonUtils.IMoves.slideCannon:
                    child.myGrid[move.NewCell.myRow, move.NewCell.myColumn].myPiece = move.OldCell.myPiece;
                    child.myGrid[move.OldCell.myRow, move.OldCell.myColumn].myPiece = CannonUtils.ISoldiers.empty;
                    break;
                case CannonUtils.IMoves.capture:
                    child.IsTerminalMove(move);
                    child.myGrid[move.NewCell.myRow, move.NewCell.myColumn].myPiece = move.OldCell.myPiece;
                    child.myGrid[move.OldCell.myRow, move.OldCell.myColumn].myPiece = CannonUtils.ISoldiers.empty;
                    break;
            }

            // update history and counter
            child.myHistory.Add(move);
            child.turnCounter++;

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
            other.legalMoves = new List<Move>(legalMoves);
            other.myHistory = new List<Move>(myHistory);
            other.turnCounter = turnCounter;
            other.myGrid = new Cell[CannonUtils.Size, CannonUtils.Size];
            other.terminalState = CannonUtils.INode.leaf;
            for (int i = 0; i < CannonUtils.Size; i++)
            {
                for (int j = 0; j < CannonUtils.Size; j++)
                {
                    other.myGrid[i, j] = myGrid[i, j].DeepCopy();
                }
            }
            return other;
        }

        private void IsTerminalMove(Move move)
        {
            if(move.NewCell.myPiece == CannonUtils.ISoldiers.dark_town)
            {
                // light wins
                 terminalState = CannonUtils.INode.light_wins;
            }
            else if (move.NewCell.myPiece == CannonUtils.ISoldiers.light_town)
            {
                // dark wins
                terminalState = CannonUtils.INode.dark_wins;
            }
        }

        private void LegalMovesLeft()
        {
            if (legalMoves.Count == 0)
            {
                if (myFriend == CannonUtils.ISoldiers.dark_soldier)
                {
                    // dark loses
                    terminalState = CannonUtils.INode.light_wins;
                }
                else if (myFriend == CannonUtils.ISoldiers.light_soldier)
                {
                    // light loses
                    terminalState = CannonUtils.INode.dark_wins;
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
            soldierList = new List<Cell>();
            legalMoves = new List<Move>();

            // fill LegalMoves list
            foreach (Cell soldier in myGrid)
            {
                if (soldier.myPiece == myFriend) 
                {
                    soldierList.Add(soldier);
                    stepCaptureRetreatMoves(soldier);
                    slideMoves(soldier);
                }
                else if (soldier.myPiece == myEnemy || soldier.myPiece == townEnemy) 
                {
                    soldierList.Add(soldier);
                    shootMoves(soldier); 
                }
            }

            // Move Ordering based on IMove enum type
            legalMoves = legalMoves.OrderByDescending(o => (int)o.Type).ToList();
        }
        
        /// <param name="_column"> Column to be added the town </param>
        /// <param name="_color"> True == Dark,  False == Light </param>
        public void AddTown(int _column, CannonUtils.ISoldiers _color)
        {
            if (_color == CannonUtils.ISoldiers.dark_soldier) 
            { 
                myGrid[0, _column].myPiece = CannonUtils.ISoldiers.dark_town;
                darkTown = myGrid[0, _column];
            }
            else if (_color == CannonUtils.ISoldiers.light_soldier) 
            { 
                myGrid[9, _column].myPiece = CannonUtils.ISoldiers.light_town;
                lightTown = myGrid[9, _column];
            }
            turnCounter++;
        }

        /// <summary>
        /// A soldier may move one STEP forward or diagonally forward to an adjacent empty point.
        /// A soldier may CAPTURE an enemy piece (a soldier or the Town) standing on an adjacent point 
        /// by moving one step sideways, forward or diagonally forward
        /// </summary>
        private void stepCaptureRetreatMoves(Cell soldier)
        {
            bool retreat = false;
            if (soldier.myPiece == CannonUtils.ISoldiers.dark_soldier)
            {
                if(soldier.myRow < 9)
                {
                    // (i+1, j)
                    // capture or step
                    Cell c = myGrid[soldier.myRow + 1, soldier.myColumn];
                    if (StepCaptureRetreat(soldier, c)) { retreat = true; }

                    // (i+1, j-1)
                    if (soldier.myColumn > 0)
                    {
                        c = myGrid[soldier.myRow + 1, soldier.myColumn - 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }

                    // (i+1, j+1)
                    if (soldier.myColumn < 9)
                    {
                        c = myGrid[soldier.myRow + 1, soldier.myColumn + 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }
                }
                // (i, j-1)
                if (soldier.myColumn > 0)
                {
                    Cell c = myGrid[soldier.myRow, soldier.myColumn - 1];
                    if (c.myPiece == myEnemy || c.myPiece == townEnemy)
                    {
                        // capture or retreat
                        if (soldier.myRow > 1)
                        {
                            retreat = true;
                        }
                        legalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
                }

                // (i, j+1)
                if (soldier.myColumn < 9)
                {
                    Cell c = myGrid[soldier.myRow, soldier.myColumn + 1];
                    if (c.myPiece == myEnemy || c.myPiece == townEnemy)
                    {
                        // capture or retreat
                        if (soldier.myRow > 1)
                        {
                            retreat = true;
                        }
                        legalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
                }

                // check if enemy in back row
                // (i-1, j+1)
                if (soldier.myRow > 1)
                {
                    // (i-1, j)
                    if (myGrid[soldier.myRow - 1, soldier.myColumn].myPiece == myEnemy)
                    {
                        retreat = true;
                    }
                    // (i-1, j-1)
                    else if (soldier.myColumn > 0 && myGrid[soldier.myRow - 1, soldier.myColumn - 1].myPiece == myEnemy)
                    {
                        retreat = true;
                    }
                    else if (soldier.myColumn < 9 && myGrid[soldier.myRow - 1, soldier.myColumn + 1].myPiece == myEnemy)
                    {
                        retreat = true;
                    }
                }
            }            
            else
            {
                if (soldier.myRow > 0)
                {
                    // (i-1, j)
                    // capture or step
                    Cell c = myGrid[soldier.myRow - 1, soldier.myColumn];
                    if (StepCaptureRetreat(soldier, c)) { retreat = true; }

                    // (i-1, j-1)
                    if (soldier.myColumn > 0)
                    {
                        c = myGrid[soldier.myRow - 1, soldier.myColumn - 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }

                    // (i-1, j+1)
                    if (soldier.myColumn < 9)
                    {
                        c = myGrid[soldier.myRow - 1, soldier.myColumn + 1];
                        if (StepCaptureRetreat(soldier, c)) { retreat = true; }
                    }
                }
                // (i, j-1)
                if (soldier.myColumn > 0)
                {
                    Cell c = myGrid[soldier.myRow, soldier.myColumn - 1];
                    if (c.myPiece == myEnemy || c.myPiece == townEnemy)
                    {
                        // capture or retreat
                        if (soldier.myRow < 8)
                        {
                            retreat = true;
                        }
                        legalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
                }

                // (i, j+1)
                if (soldier.myColumn < 9)
                {
                    Cell c = myGrid[soldier.myRow, soldier.myColumn + 1];
                    if (c.myPiece == myEnemy || c.myPiece == townEnemy)
                    {
                        // capture or retreat
                        if (soldier.myRow < 8)
                        {
                            retreat = true;
                        }
                        legalMoves.Add(new Move(soldier.DeepCopy(),
                                                c.DeepCopy(),
                                                CannonUtils.IMoves.capture));
                    }
                }

                // check if enemy in back row
                if (soldier.myRow < 8)
                {
                    // (i+1, j)
                    if (myGrid[soldier.myRow + 1, soldier.myColumn].myPiece == myEnemy)
                    {
                        retreat = true;
                    }
                    // (i+1, j-1)
                    else if (soldier.myColumn > 0 && myGrid[soldier.myRow + 1, soldier.myColumn - 1].myPiece == myEnemy)
                    {
                        retreat = true;
                    }
                    // (i+1, j+1)
                    else if (soldier.myColumn < 9 && myGrid[soldier.myRow + 1, soldier.myColumn + 1].myPiece == myEnemy)
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
            if (c.myPiece == CannonUtils.ISoldiers.empty)
            {
                // step
                legalMoves.Add(new Move(soldier.DeepCopy(),
                                        c.DeepCopy(),
                                        CannonUtils.IMoves.step));
            }
            else if (c.myPiece == myEnemy || c.myPiece == townEnemy)
            {
                // capture or retreat
                if ((soldier.myPiece == CannonUtils.ISoldiers.dark_soldier && soldier.myRow > 1) ||
                    soldier.myPiece == CannonUtils.ISoldiers.light_soldier && soldier.myRow < 8)
                {
                    retreat = true;
                }
                legalMoves.Add(new Move(soldier.DeepCopy(),
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
            bool canRetreatLeft = soldier.myColumn > 1 ? true : false;
            bool canRetreatRight = soldier.myColumn < 8 ? true : false;
            if (soldier.myPiece == CannonUtils.ISoldiers.dark_soldier)
            {
                // i to i-1
                // retreat backward
                if (myGrid[soldier.myRow - 1, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty &&
                    myGrid[soldier.myRow - 2, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty)
                {
                    legalMoves.Add(new Move(soldier.DeepCopy(),
                                            myGrid[soldier.myRow - 2, soldier.myColumn].DeepCopy(), 
                                            CannonUtils.IMoves.retreat));
                }

                // retreat left diagonal
                if (canRetreatLeft &&
                    myGrid[soldier.myRow - 1, soldier.myColumn-1].myPiece == CannonUtils.ISoldiers.empty &&
                    myGrid[soldier.myRow - 2, soldier.myColumn - 2].myPiece == CannonUtils.ISoldiers.empty)
                {
                    legalMoves.Add(new Move(soldier.DeepCopy(),
                                    myGrid[soldier.myRow - 2, soldier.myColumn - 2].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat right diagonal
                if (canRetreatRight &&
                    myGrid[soldier.myRow - 1, soldier.myColumn + 1].myPiece == CannonUtils.ISoldiers.empty &&
                    myGrid[soldier.myRow - 2, soldier.myColumn + 2].myPiece == CannonUtils.ISoldiers.empty)
                {
                    legalMoves.Add(new Move(soldier.DeepCopy(),
                                    myGrid[soldier.myRow - 2, soldier.myColumn + 2].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

            }
            else if (soldier.myPiece == CannonUtils.ISoldiers.light_soldier)
            {
                // i to i+1
                // retreat backward
                if (myGrid[soldier.myRow + 1, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty &&
                    myGrid[soldier.myRow + 2, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty)
                {
                    legalMoves.Add(new Move(soldier.DeepCopy(),
                                    myGrid[soldier.myRow + 2, soldier.myColumn].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat left diagonal
                if (canRetreatLeft &&
                    myGrid[soldier.myRow + 1, soldier.myColumn - 1].myPiece == CannonUtils.ISoldiers.empty &&
                    myGrid[soldier.myRow + 2, soldier.myColumn - 2].myPiece == CannonUtils.ISoldiers.empty)
                {
                    legalMoves.Add(new Move(soldier.DeepCopy(),
                                    myGrid[soldier.myRow + 2, soldier.myColumn - 2].DeepCopy(),
                                    CannonUtils.IMoves.retreat));
                }

                // retreat right diagonal
                if (canRetreatRight &&
                    myGrid[soldier.myRow + 1, soldier.myColumn + 1].myPiece == CannonUtils.ISoldiers.empty &&
                    myGrid[soldier.myRow + 2, soldier.myColumn + 2].myPiece == CannonUtils.ISoldiers.empty)
                {
                    legalMoves.Add(new Move(soldier.DeepCopy(),
                                    myGrid[soldier.myRow + 2, soldier.myColumn + 2].DeepCopy(),
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
            if (2 < soldier.myColumn
                && myGrid[soldier.myRow, soldier.myColumn - 1].myPiece == soldier.myPiece
                && myGrid[soldier.myRow, soldier.myColumn - 2].myPiece == soldier.myPiece
                && myGrid[soldier.myRow, soldier.myColumn - 3].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow, soldier.myColumn - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
                  
            }

            // two soldiers at right -> slide to right
            if (soldier.myColumn < 7
                && myGrid[soldier.myRow, soldier.myColumn + 1].myPiece == soldier.myPiece
                && myGrid[soldier.myRow, soldier.myColumn + 2].myPiece == soldier.myPiece
                && myGrid[soldier.myRow, soldier.myColumn + 3].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow, soldier.myColumn + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // Two soldiers bellow -> slide bellow
            if (2 < soldier.myRow
                && myGrid[soldier.myRow - 1, soldier.myColumn].myPiece == soldier.myPiece
                && myGrid[soldier.myRow - 2, soldier.myColumn].myPiece == soldier.myPiece
                && myGrid[soldier.myRow - 3, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow - 3, soldier.myColumn].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // two soldiers above -> slide above
            if (soldier.myRow < 7
                && myGrid[soldier.myRow + 1, soldier.myColumn].myPiece == soldier.myPiece
                && myGrid[soldier.myRow + 2, soldier.myColumn].myPiece == soldier.myPiece
                && myGrid[soldier.myRow + 3, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow + 3, soldier.myColumn].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // DIAGONAL
            // two solidiers at diagonal upper left -> slide diagonal up left
            if (soldier.myRow < 7 && soldier.myColumn > 2
                && myGrid[soldier.myRow + 1, soldier.myColumn - 1].myPiece == soldier.myPiece
                && myGrid[soldier.myRow + 2, soldier.myColumn - 2].myPiece == soldier.myPiece
                && myGrid[soldier.myRow + 3, soldier.myColumn - 3].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow + 3, soldier.myColumn - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // two soldiers at diagonal lower left 
            if (soldier.myRow > 2 && soldier.myColumn > 2
                && myGrid[soldier.myRow - 1, soldier.myColumn - 1].myPiece == soldier.myPiece
                && myGrid[soldier.myRow - 2, soldier.myColumn - 2].myPiece == soldier.myPiece
                && myGrid[soldier.myRow - 3, soldier.myColumn - 3].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow - 3, soldier.myColumn - 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // diagonal upper right
            if (soldier.myRow < 7 && soldier.myColumn < 7
                && myGrid[soldier.myRow + 1, soldier.myColumn + 1].myPiece == soldier.myPiece
                && myGrid[soldier.myRow + 2, soldier.myColumn + 2].myPiece == soldier.myPiece
                && myGrid[soldier.myRow + 3, soldier.myColumn + 3].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow + 3, soldier.myColumn + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }

            // diagonal lower right 
            if (soldier.myRow > 2 && soldier.myColumn < 7
                && myGrid[soldier.myRow - 1, soldier.myColumn + 1].myPiece == soldier.myPiece
                && myGrid[soldier.myRow - 2, soldier.myColumn + 2].myPiece == soldier.myPiece
                && myGrid[soldier.myRow - 3, soldier.myColumn + 3].myPiece == CannonUtils.ISoldiers.empty)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), myGrid[soldier.myRow - 3, soldier.myColumn + 3].DeepCopy(), CannonUtils.IMoves.slideCannon));
            }
        }

        private void shootMoves(Cell soldier)
        {
            // ORTHOGONAL 
            // Three soldiers at left
            if (3 < soldier.myColumn
                && myGrid[soldier.myRow, soldier.myColumn - 1].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow, soldier.myColumn - 2].myPiece == myFriend
                && myGrid[soldier.myRow, soldier.myColumn - 3].myPiece == myFriend
                && myGrid[soldier.myRow, soldier.myColumn - 4].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // shoot right
            else if (soldier.myColumn < 6
                && myGrid[soldier.myRow, soldier.myColumn + 1].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow, soldier.myColumn + 2].myPiece == myFriend
                && myGrid[soldier.myRow, soldier.myColumn + 3].myPiece == myFriend
                && myGrid[soldier.myRow, soldier.myColumn + 4].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // Two soldiers bellow -> slide bellow
            else if(3 < soldier.myRow
                && myGrid[soldier.myRow - 1, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow - 2, soldier.myColumn].myPiece == myFriend
                && myGrid[soldier.myRow - 3, soldier.myColumn].myPiece == myFriend
                && myGrid[soldier.myRow - 4, soldier.myColumn].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // two soldiers above -> slide above
            else if(soldier.myRow < 6
                && myGrid[soldier.myRow + 1, soldier.myColumn].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow + 2, soldier.myColumn].myPiece == myFriend
                && myGrid[soldier.myRow + 3, soldier.myColumn].myPiece == myFriend
                && myGrid[soldier.myRow + 4, soldier.myColumn].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // DIAGONAL
            // two solidiers at diagonal upper left -> slide diagonal up left
            else if(soldier.myRow < 6 && soldier.myColumn > 3
                && myGrid[soldier.myRow + 1, soldier.myColumn - 1].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow + 2, soldier.myColumn - 2].myPiece == myFriend
                && myGrid[soldier.myRow + 3, soldier.myColumn - 3].myPiece == myFriend
                && myGrid[soldier.myRow + 4, soldier.myColumn - 4].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // two soldiers at diagonal lower left 
            else if(soldier.myRow > 3 && soldier.myColumn > 3
                && myGrid[soldier.myRow - 1, soldier.myColumn - 1].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow - 2, soldier.myColumn - 2].myPiece == myFriend
                && myGrid[soldier.myRow - 3, soldier.myColumn - 3].myPiece == myFriend
                && myGrid[soldier.myRow - 4, soldier.myColumn - 4].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // diagonal upper right
            else if(soldier.myRow < 6 && soldier.myColumn < 6
                && myGrid[soldier.myRow + 1, soldier.myColumn + 1].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow + 2, soldier.myColumn + 2].myPiece == myFriend
                && myGrid[soldier.myRow + 3, soldier.myColumn + 3].myPiece == myFriend
                && myGrid[soldier.myRow + 4, soldier.myColumn + 4].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }

            // diagonal lower right 
            else if(soldier.myRow > 3 && soldier.myColumn < 6
                && myGrid[soldier.myRow - 1, soldier.myColumn + 1].myPiece == CannonUtils.ISoldiers.empty
                && myGrid[soldier.myRow - 2, soldier.myColumn + 2].myPiece == myFriend
                && myGrid[soldier.myRow - 3, soldier.myColumn + 3].myPiece == myFriend
                && myGrid[soldier.myRow - 4, soldier.myColumn + 4].myPiece == myFriend)
            {
                legalMoves.Add(new Move(soldier.DeepCopy(), soldier.DeepCopy(), CannonUtils.IMoves.shootCannon));
            }
        }
        #endregion

        #region Enemy Friend Utils
        public CannonUtils.ISoldiers myFriend => CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers myEnemy => !CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers TownFriend => CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;
        public CannonUtils.ISoldiers townEnemy => !CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;
        #endregion
    }
}
