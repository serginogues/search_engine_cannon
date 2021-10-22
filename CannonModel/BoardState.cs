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
    /// 
    /// <para> 
    ///   A B C D E F G H I J
    /// 9 · x · · · · · · · ·
    /// 8 · · · · · · · · · ·
    /// 7 · · · · · · · · · ·
    /// 6 · · · · · · · · · ·
    /// 5 · · · · y · · · · ·
    /// 4 · · · · · · · · · ·
    /// 3 · · · · · · · · · ·
    /// 2 · · · · · · · · · ·
    /// 1 · · · · · · · · · ·
    /// 0 · · · · · · · · · ·
    /// x = Grid[91]
    /// y = Grid[54]
    /// </para> 
    /// </summary>
    public class BoardState
    {
        #region Properties
        public int turnCounter { get; set; }
        public CannonUtils.eSoldiers[] Board { get; set; }

        /// <summary>
        /// List of move offsets from current node in clockwise order starting from left
        /// <para> 0 = left,          </para>  
        /// <para> 1 = d_up_left,     </para>
        /// <para> 2 = up,            </para>
        /// <para> 3 = d_up_right,    </para>
        /// <para> 4 = right,         </para>
        /// <para> 5 = d_down_right,  </para>
        /// <para> 6 = down,          </para>
        /// <para> 7 = d_down_left    </para>
        /// </summary>
        private static readonly int[] moveOffsets = { -1, 9, 10, 11, 1, -9, -10, -11 };

        /// <summary>
        /// <para> 0 = left,          </para>  
        /// <para> 1 = d_up_left,     </para>
        /// <para> 2 = up,            </para>
        /// <para> 3 = d_up_right,    </para>
        /// <para> 4 = right,         </para>
        /// <para> 5 = d_down_right,  </para>
        /// <para> 6 = down,          </para>
        /// <para> 7 = d_down_left    </para>
        /// <para> Inspired by: https://github.com/SebLague/Chess-AI </para> 
        /// </summary>
        private static int[][] numSquaresToEdge { get; set; }

        public List<Move> legalMoves { get; set; } 
        /// <summary>
        /// List of moves from root to this state of the board
        /// </summary>
        public List<Move> plyHistory { get; set; }
        /// <summary>
        /// if node is terminal node, then return its terminalNode value
        /// </summary>
        public CannonUtils.eNode terminalFlag { get; set; }

        public BoardCounter boardCounter { get; set; }

        public int darkTown { get; set; }
        public int lightTown { get; set; }

        #endregion

        #region Constructor (root node)
        /// <summary>
        /// Called only once at root node
        /// </summary>
        public void root_init()
        {
            plyHistory = new List<Move>();
            boardCounter = new BoardCounter();
            turnCounter = 0;
            initGrid();
            initNumSquaresToEdge();
            generateLegalMoves();
            terminalFlag = CannonUtils.eNode.leaf;
        }

        /// <summary>
        /// Assign soldiers to its position;
        /// </summary>
        private void initGrid()
        {
            int _size = CannonUtils.Size;
            Board = new CannonUtils.eSoldiers[_size*_size];
            for (int row = 0; row < _size; row++)
            {
                for (int column = 0; column < _size; column++)
                {
                    int boardIndex = row * 10 + column;
                    if (CannonUtils.rootDark.Contains(boardIndex))
                    {
                        Board[boardIndex] = CannonUtils.eSoldiers.dark_soldier;
                    }
                    else if (CannonUtils.rootLight.Contains(boardIndex))
                    {
                        Board[boardIndex] = CannonUtils.eSoldiers.light_soldier;
                    }
                }
            }
        }

        private void initNumSquaresToEdge()
        {
            numSquaresToEdge = new int[100][];
            int _size = CannonUtils.Size;
            for (int row = 0; row < _size; row++)
            {
                for (int column = 0; column < _size; column++)
                {
                    int boardIndex = row * 10 + column;

                    int up = _size - 1 - row;
                    int down = row; 
                    int left = column;
                    int right = _size - 1 - column;

                    // follow same clockwise pattern as movesOffset
                    numSquaresToEdge[boardIndex] = new int[8]
                    {
                        left, 
                        Math.Min(up, left),
                        up,
                        Math.Min(up, right),
                        right,
                        Math.Min(down, right),
                        down,
                        Math.Min(down, left)
                    };
                }
            }
        }
        #endregion

        #region Moves
        /// <summary>
        /// Reset moves
        /// </summary>
        public void generateLegalMoves()
        {
            legalMoves = new List<Move>();

            for (int square = 0; square < 100; square++)
            {
                CannonUtils.eSoldiers piece = Board[square];
                if(piece == friendSoldier)
                {
                    generateStepRetreatCaptureSlide(square, piece);
                }
                else if(piece == enemySoldier || piece == enemyTown)
                {
                    generateShoots(square, piece);
                }

                if(piece == CannonUtils.eSoldiers.dark_soldier) { boardCounter.darkPieceList.Add(square); }
                else if (piece == CannonUtils.eSoldiers.light_soldier) { boardCounter.lightPieceList.Add(square); }
            }

            // order moves by enum score: shoot > capture > step > retreat ...
            legalMoves = legalMoves.OrderByDescending(o => (int)o.moveType).ToList();

            if (legalMoves.Count == 0)
            {
                terminalFlag = friendSoldier == CannonUtils.eSoldiers.dark_soldier ? CannonUtils.eNode.light_wins : CannonUtils.eNode.dark_wins;
            }
        }

        /// <summary>
        /// A cannon may make a capture without sliding, i.e. to "SHOOT" an enemy piece 
        /// (either a soldier or the Town) standing on the same line as the shooting cannon 
        /// if there is one or two empty points between the cannon's front soldier and the enemy piece.
        /// 
        /// Evaluated from the point of view of the enemy cell.
        /// </summary>
        private void generateShoots(int startSquare, CannonUtils.eSoldiers startPiece)
        {
            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                int friendCount = 0;
                for (int n = 0; n < numSquaresToEdge[startSquare][directionIndex]; n++)
                {
                    // Determine the target square
                    int targetSquare = startSquare + moveOffsets[directionIndex] * (n + 1);
                    // Determine the target type
                    CannonUtils.eSoldiers targetPiece = Board[targetSquare];

                    // no squares further than 3 steps
                    bool isFurther = n > 4;

                    // if there is a soldier at distance 1, break because there is no shoot possible
                    bool soldierNeighbour = n == 0 && targetPiece != CannonUtils.eSoldiers.empty;

                    // if there is any empty square at a distance of 3 or 4, break because there is no cannon possible
                    bool emptySquare = (n == 2 || n == 3) && targetPiece == CannonUtils.eSoldiers.empty;

                    if (isFurther || soldierNeighbour || emptySquare) break;

                    if (targetPiece == friendSoldier) friendCount++;

                    if(friendCount == 3)
                    {
                        // cannon shoot
                        legalMoves.Add(new Move(startSquare, startSquare, CannonUtils.eMoves.shootCannon, startPiece, startPiece));
                        boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.shootCannon]++;
                        break;
                    }
                }
            }
        }

        private bool isEnemyAround(int startSquare)
        {
            // check all directions for enemy neighbours
            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                if(numSquaresToEdge[startSquare][directionIndex] > 0)
                {
                    // Determine the target square
                    int targetSquare = startSquare + moveOffsets[directionIndex];

                    // Check if enemy
                    if (Board[targetSquare] == enemySoldier) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// - A soldier may move one STEP forward or diagonally forward 
        /// - A soldier may CAPTURE an enemy piece (a soldier or the Tow
        ///   by moving one step sideways, forward or diagonally forward.
        /// - A soldier can RETREAT two points backwards or diagonally backwards 
        ///   if it is adjacent to an enemy soldier and if the target and intermediate spots are empty.
        /// </summary>
        private void generateStepRetreatCaptureSlide(int startSquare, CannonUtils.eSoldiers startPiece)
        {
            bool enemyAround = isEnemyAround(startSquare);
            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                int friendSoldiersCount = 0;

                for (int n = 0; n < numSquaresToEdge[startSquare][directionIndex]; n++)
                {
                    // There is no movements at distance bigger than 3
                    if(n > 2) { break; }

                    // Determine the target square
                    int targetSquare = startSquare + moveOffsets[directionIndex] * (n + 1);
                    // Determine the target type
                    CannonUtils.eSoldiers targetPiece = Board[targetSquare];

                    // [friendSoldiersCount] = 2 means that there is a cannon in direction [directionIndex] 
                    if (n < 2 && targetPiece == friendSoldier) { friendSoldiersCount++; }
                    else if (n == 2 && friendSoldiersCount == 2 && targetPiece == CannonUtils.eSoldiers.empty)
                    {
                        // there is three soldiers in a row and an empty spot where to slide
                        legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.eMoves.slideCannon, startPiece, targetPiece));
                        boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.slideCannon]++;
                        break;
                    }

                    // Check first neightbour squares
                    if (n == 0)
                    {
                        // Left and right direction squares check
                        if(directionIndex == 0 || directionIndex == 4)
                        {
                            if (targetPiece == CannonUtils.eSoldiers.empty)
                            {
                                // if target is empty, we won't slide, step, capture, shoot or retreat
                                // then simply stop searching this direction
                                break;
                            }
                            else if(targetPiece == enemySoldier || targetPiece == enemyTown)
                            {
                                // if target is enemy we can capture it
                                legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.eMoves.capture, startPiece, targetPiece));
                                boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.capture]++;

                                // there is no point in keep searching in this direction 
                                break;
                            }
                        }
                        else if (startPiece == CannonUtils.eSoldiers.dark_soldier)
                        {
                            // dark squares

                            if (directionIndex == 1 || directionIndex == 2 || directionIndex == 3)
                            {
                                // diagonal and orthogonal forward

                                if (targetPiece == enemyTown || targetPiece == enemySoldier)
                                {
                                    // if it is a enemy we can capture it and nothing else
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.eMoves.capture, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.capture]++;
                                    break;
                                }
                                else if(targetPiece == CannonUtils.eSoldiers.empty)
                                {
                                    // if its empty we can step and nothing else
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.eMoves.step, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.step]++;
                                    break;
                                }
                            }
                            else if (directionIndex == 5 || directionIndex == 6 || directionIndex == 7)
                            {
                                // diagonal or orthogonal backward
                                if (targetPiece == enemyTown || targetPiece == enemySoldier)
                                {
                                    // if it is a enemy or empty we can do nothing
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // light squares
                            if (directionIndex == 5 || directionIndex == 6 || directionIndex == 7)
                            {
                                // diagonal and orthogonal forward

                                if (targetPiece == enemyTown || targetPiece == enemySoldier)
                                {
                                    // if it is a enemy we can capture it and nothing else
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.eMoves.capture, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.capture]++;
                                    break;
                                }
                                else if (targetPiece == CannonUtils.eSoldiers.empty)
                                {
                                    // if its empty we can step and nothing else
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.eMoves.step, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.step]++;
                                    break;
                                }
                            }
                            else if (directionIndex == 1 || directionIndex == 2 || directionIndex == 3)
                            {
                                // diagonal or orthogonal backward
                                if (targetPiece == enemyTown || targetPiece == enemySoldier)
                                {
                                    // if it is a enemy or empty we can do nothing
                                    break;
                                }
                            }

                        }
                    }
                    else if (n == 1 && enemyAround)
                    {
                        // retreat moves

                        int middleSquare = startSquare + moveOffsets[directionIndex];
                        bool isEmpty = Board[middleSquare] == CannonUtils.eSoldiers.empty && targetPiece == CannonUtils.eSoldiers.empty;

                        if (isEmpty && (((directionIndex == 5 || directionIndex == 6 || directionIndex == 7) && startPiece == CannonUtils.eSoldiers.dark_soldier) ||
                                ((directionIndex == 1 || directionIndex == 2 || directionIndex == 3) && startPiece == CannonUtils.eSoldiers.light_soldier)))
                        {
                            legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.eMoves.retreat, startPiece, targetPiece));
                            boardCounter.moveTypeCounter[(int)CannonUtils.eMoves.retreat]++;
                            break;
                        }
                    }
                }
            }
        }

        /// <param name="_column"> Column to be added the town </param>
        /// <param name="_color"> True == Dark,  False == Light </param>
        public void AddTown(int _column, CannonUtils.eSoldiers _color)
        {
            if (_color == CannonUtils.eSoldiers.dark_soldier)
            {
                int boardIndex = _column;
                Board[boardIndex] = CannonUtils.eSoldiers.dark_town;
                darkTown = boardIndex;
            }
            else if (_color == CannonUtils.eSoldiers.light_soldier)
            {
                int boardIndex = 90 + _column;
                Board[boardIndex] = CannonUtils.eSoldiers.light_town;
                lightTown = boardIndex;
            }
            turnCounter++;
        }
        #endregion

        #region Make move
        /// <summary>
        /// Generate child based on move_id reference
        /// </summary>
        public BoardState Successor(int moveIndex)
        {
            // create [child] that is exactly the same as [this]
            BoardState child = DeepCopy();

            // move soldiers in [child] based on [move_id]
            Move move = child.legalMoves[moveIndex];
            CannonUtils.eSoldiers oldPiece = Board[move.startIndex];
            switch (move.moveType)
            {
                case CannonUtils.eMoves.shootCannon:
                    child.IsTerminalMove(move);
                    child.Board[move.targetIndex] = CannonUtils.eSoldiers.empty;
                    break;
                case CannonUtils.eMoves.step:
                case CannonUtils.eMoves.retreat:
                case CannonUtils.eMoves.slideCannon:
                    child.Board[move.targetIndex] = oldPiece;
                    child.Board[move.startIndex] = CannonUtils.eSoldiers.empty;
                    break;
                case CannonUtils.eMoves.capture:
                    child.IsTerminalMove(move);
                    child.Board[move.targetIndex] = oldPiece;
                    child.Board[move.startIndex] = CannonUtils.eSoldiers.empty;
                    break;
            }

            // update history and counter
            child.plyHistory.Add(move);
            child.turnCounter++;

            // update LegalMoves
            child.generateLegalMoves();

            return child;
        }

        private void IsTerminalMove(Move move)
        {
            CannonUtils.eSoldiers piece = Board[move.targetIndex];
            if (piece == CannonUtils.eSoldiers.dark_town) { terminalFlag = CannonUtils.eNode.light_wins; }
            else if (piece == CannonUtils.eSoldiers.light_town) { terminalFlag = CannonUtils.eNode.dark_wins; }
        }

        /// <summary>
        /// To make children from current BoardState and change their properties without referencing its parent;
        /// Copied parameters: History, Grid, TurnCounter, LegalMoves
        /// </summary>
        public BoardState DeepCopy()
        {
            BoardState new_position = (BoardState)this.MemberwiseClone();
            new_position.legalMoves = new List<Move>(legalMoves);
            new_position.plyHistory = new List<Move>(plyHistory);
            new_position.turnCounter = turnCounter;
            new_position.Board = new CannonUtils.eSoldiers[CannonUtils.Size* CannonUtils.Size];
            new_position.Board = Board.Select(a => a).ToArray();
            Array.Copy(new_position.Board, Board, Board.Length);
            new_position.terminalFlag = CannonUtils.eNode.leaf;
            new_position.boardCounter = new BoardCounter();
            return new_position;
        }
        #endregion

        #region Enemy Friend Utils
        public CannonUtils.eSoldiers friendSoldier => CannonUtils.IsOdd(turnCounter) ? CannonUtils.eSoldiers.light_soldier : CannonUtils.eSoldiers.dark_soldier;
        public CannonUtils.eSoldiers enemySoldier => !CannonUtils.IsOdd(turnCounter) ? CannonUtils.eSoldiers.light_soldier : CannonUtils.eSoldiers.dark_soldier;
        public CannonUtils.eSoldiers TownFriend => CannonUtils.IsOdd(turnCounter) ? CannonUtils.eSoldiers.light_town : CannonUtils.eSoldiers.dark_town;
        public CannonUtils.eSoldiers enemyTown => !CannonUtils.IsOdd(turnCounter) ? CannonUtils.eSoldiers.light_town : CannonUtils.eSoldiers.dark_town;
        #endregion
    }

    public class BoardCounter
    {
        public List<int> darkPieceList { get; set; }
        public List<int> lightPieceList { get; set; }
        public int[] moveTypeCounter { get; set; }

        public BoardCounter()
        {
            darkPieceList = new List<int>();
            lightPieceList = new List<int>();
            moveTypeCounter = new int[Enum.GetNames(typeof(CannonUtils.eMoves)).Length];
        }
    } 
}
