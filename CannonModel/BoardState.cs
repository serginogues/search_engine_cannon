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
        public CannonUtils.ISoldiers[] Board { get; set; }

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
        public CannonUtils.INode terminalFlag { get; set; }

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
            terminalFlag = CannonUtils.INode.leaf;
        }

        /// <summary>
        /// Assign soldiers to its position;
        /// </summary>
        private void initGrid()
        {
            int _size = CannonUtils.Size;
            Board = new CannonUtils.ISoldiers[_size*_size];
            for (int row = 0; row < _size; row++)
            {
                for (int column = 0; column < _size; column++)
                {
                    int boardIndex = row * 10 + column;
                    if (CannonUtils.rootDark.Contains(boardIndex))
                    {
                        Board[boardIndex] = CannonUtils.ISoldiers.dark_soldier;
                    }
                    else if (CannonUtils.rootLight.Contains(boardIndex))
                    {
                        Board[boardIndex] = CannonUtils.ISoldiers.light_soldier;
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
                CannonUtils.ISoldiers piece = Board[square];
                if(piece == friendSoldier)
                {
                    generateStepRetreatCaptureSlide(square, piece);
                }
                else if(piece == enemySoldier || piece == enemyTown)
                {
                    generateShoots(square, piece);
                }

                if(piece == CannonUtils.ISoldiers.dark_soldier) { boardCounter.darkPieceList.Add(square); }
                else if (piece == CannonUtils.ISoldiers.light_soldier) { boardCounter.lightPieceList.Add(square); }
            }

            // order moves by enum score: shoot > capture > step > retreat ...
            legalMoves = legalMoves.OrderByDescending(o => (int)o.moveType).ToList();

            if (legalMoves.Count == 0)
            {
                terminalFlag = friendSoldier == CannonUtils.ISoldiers.dark_soldier ? CannonUtils.INode.light_wins : CannonUtils.INode.dark_wins;
            }
        }

        /// <summary>
        /// A cannon may make a capture without sliding, i.e. to "SHOOT" an enemy piece 
        /// (either a soldier or the Town) standing on the same line as the shooting cannon 
        /// if there is one or two empty points between the cannon's front soldier and the enemy piece.
        /// </summary>
        /// <param name="targetSquare"></param>
        /// <param name="targetPiece"></param>
        private void generateShoots(int targetSquare, CannonUtils.ISoldiers targetPiece)
        {
            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                bool isShort = true;
                for (int n = 0; n < numSquaresToEdge[targetSquare][directionIndex]; n++)
                {
                    int startSquare = targetSquare + moveOffsets[directionIndex] * (n + 1);
                    CannonUtils.ISoldiers piece = Board[startSquare];
                    if (n == 0 && piece != CannonUtils.ISoldiers.empty) { break; }
                    else if (n == 1)
                    {
                        if(piece == friendSoldier)
                        {
                            isShort = true;
                        }
                        else if (piece == CannonUtils.ISoldiers.empty)
                        {
                            isShort = false;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if ((n == 2 || n == 3) && piece != friendSoldier) { break; }
                    else if (n == 4)
                    {
                        if (isShort)
                        {
                            legalMoves.Add(new Move(targetSquare, targetSquare, CannonUtils.IMoves.shootCannon, targetPiece, targetPiece));
                            boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.shootCannon]++;
                            break;
                        }
                        else
                        {
                            if(piece != friendSoldier) { break; }
                        }
                    }
                    else if(!isShort && n == 5)
                    {
                        legalMoves.Add(new Move(targetSquare, targetSquare, CannonUtils.IMoves.shootCannon, targetPiece, targetPiece));
                        boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.shootCannon]++;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// - A soldier may move one STEP forward or diagonally forward 
        /// - A soldier may CAPTURE an enemy piece (a soldier or the Tow
        ///   by moving one step sideways, forward or diagonally forward.
        /// - A soldier can RETREAT two points backwards or diagonally backwards 
        ///   if it is adjacent to an enemy soldier and if the target and intermediate spots are empty.
        /// </summary>
        private void generateStepRetreatCaptureSlide(int startSquare, CannonUtils.ISoldiers startPiece)
        {
            bool isEnemyAround = false;
            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                // soldier_count = 2 means that there is a cannon where 
                int soldier_count = 0;
                for (int n = 0; n < numSquaresToEdge[startSquare][directionIndex]; n++)
                {
                    // no movements at distance bigger than 3
                    if(n == 4) { break; }

                    // [n] = distance to [targetSquare] at direction [directionIndex]
                    int targetSquare = startSquare + moveOffsets[directionIndex] * (n + 1);
                    CannonUtils.ISoldiers targetPiece = Board[targetSquare];

                    if (n == 2 && soldier_count == 2 && targetPiece == CannonUtils.ISoldiers.empty)
                    {
                        // SLIDE
                        legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.slideCannon, startPiece, targetPiece));
                        boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.slideCannon]++;
                        break;
                    }
                    else if(targetPiece == friendSoldier)
                    {
                        soldier_count++;
                    }
                    else if (n == 0)
                    {
                        // STEP CAPTURE
                        // Left or right capture. Same for both colors
                        if ((directionIndex == 0 || directionIndex == 4) && (targetPiece == enemyTown || targetPiece == enemySoldier))
                        {
                            // if there is an enemy at the left or right of my piece at distance 1: capture move
                            legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.capture, startPiece, targetPiece));
                            boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.capture]++;
                            isEnemyAround = true;
                            break;
                        }
                        else if(startPiece == CannonUtils.ISoldiers.dark_soldier)
                        {
                            // dark can step or capture at distance 1 up, d_left_up or d_right_up
                            if (directionIndex == 1 || directionIndex == 2 || directionIndex == 3)
                            {
                                // if enemy: capture
                                // if empty: step
                                if(targetPiece == enemySoldier) 
                                { 
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.capture, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.capture]++;
                                    isEnemyAround = true;
                                }
                                else if (targetPiece == CannonUtils.ISoldiers.empty) 
                                { 
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.step, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.step]++;
                                }
                                break;
                            }
                        }
                        else
                        {
                            // light can step or capture at distance 1 down, d_left_down or d_right_down
                            if (directionIndex == 5 || directionIndex == 6 || directionIndex == 7)
                            {
                                // if enemy: capture
                                // if empty: step
                                if (targetPiece == enemySoldier) 
                                { 
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.capture, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.capture]++;
                                    isEnemyAround = true;
                                }
                                else if (targetPiece == CannonUtils.ISoldiers.empty) 
                                { 
                                    legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.step, startPiece, targetPiece));
                                    boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.step]++;
                                }
                                break;
                            }
                        }
                    }
                    else if(n == 1 && isEnemyAround)
                    {
                        int middleSquare = startSquare + moveOffsets[directionIndex] * n;
                        bool isEmpty = Board[middleSquare] == CannonUtils.ISoldiers.empty && targetPiece == CannonUtils.ISoldiers.empty;

                        // RETREAT
                        if ((directionIndex == 5 || directionIndex == 6 || directionIndex == 7) && startPiece == CannonUtils.ISoldiers.dark_soldier && isEmpty)
                        {
                            // dark retreat at distance 2 down, d_left_down or d_right_down
                            legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.retreat, startPiece, targetPiece));
                            boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.retreat]++;
                            break;
                        }
                        else if ((directionIndex == 1 || directionIndex == 2 || directionIndex == 3) && startPiece == CannonUtils.ISoldiers.light_soldier && isEmpty)
                        {
                            // light retreat at distance 2 up, d_left_up or d_right_up
                            legalMoves.Add(new Move(startSquare, targetSquare, CannonUtils.IMoves.retreat, startPiece, targetPiece));
                            boardCounter.moveTypeCounter[(int)CannonUtils.IMoves.retreat]++;
                            break;
                        }
                    }
                }
            }
        }

        /// <param name="_column"> Column to be added the town </param>
        /// <param name="_color"> True == Dark,  False == Light </param>
        public void AddTown(int _column, CannonUtils.ISoldiers _color)
        {
            if (_color == CannonUtils.ISoldiers.dark_soldier)
            {
                int boardIndex = _column;
                Board[boardIndex] = CannonUtils.ISoldiers.dark_town;
                darkTown = boardIndex;
            }
            else if (_color == CannonUtils.ISoldiers.light_soldier)
            {
                int boardIndex = 90 + _column;
                Board[boardIndex] = CannonUtils.ISoldiers.light_town;
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
            CannonUtils.ISoldiers oldPiece = Board[move.startIndex];
            switch (move.moveType)
            {
                case CannonUtils.IMoves.shootCannon:
                    child.IsTerminalMove(move);
                    child.Board[move.targetIndex] = CannonUtils.ISoldiers.empty;
                    break;
                case CannonUtils.IMoves.step:
                case CannonUtils.IMoves.retreat:
                case CannonUtils.IMoves.slideCannon:
                    child.Board[move.targetIndex] = oldPiece;
                    child.Board[move.startIndex] = CannonUtils.ISoldiers.empty;
                    break;
                case CannonUtils.IMoves.capture:
                    child.IsTerminalMove(move);
                    child.Board[move.targetIndex] = oldPiece;
                    child.Board[move.startIndex] = CannonUtils.ISoldiers.empty;
                    break;
            }

            // update history and counter
            child.plyHistory.Add(move);
            child.turnCounter++;

            // update LegalMoves
            child.generateLegalMoves();

            return child;
        }

        public BoardState NullMove()
        {
            // create [child] that is exactly the same as [this]
            BoardState child = DeepCopy();
            child.turnCounter++;

            // update LegalMoves
            child.generateLegalMoves();

            return child;
        }

        private void IsTerminalMove(Move move)
        {
            CannonUtils.ISoldiers piece = Board[move.targetIndex];
            if (piece == CannonUtils.ISoldiers.dark_town) { terminalFlag = CannonUtils.INode.light_wins; }
            else if (piece == CannonUtils.ISoldiers.light_town) { terminalFlag = CannonUtils.INode.dark_wins; }
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
            new_position.Board = new CannonUtils.ISoldiers[CannonUtils.Size* CannonUtils.Size];
            new_position.Board = Board.Select(a => a).ToArray();
            Array.Copy(new_position.Board, Board, Board.Length);
            new_position.terminalFlag = CannonUtils.INode.leaf;
            new_position.boardCounter = new BoardCounter();
            return new_position;
        }
        #endregion

        #region Enemy Friend Utils
        public CannonUtils.ISoldiers friendSoldier => CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers enemySoldier => !CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_soldier : CannonUtils.ISoldiers.dark_soldier;
        public CannonUtils.ISoldiers TownFriend => CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;
        public CannonUtils.ISoldiers enemyTown => !CannonUtils.IsOdd(turnCounter) ? CannonUtils.ISoldiers.light_town : CannonUtils.ISoldiers.dark_town;
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
            moveTypeCounter = new int[Enum.GetNames(typeof(CannonUtils.IMoves)).Length];
        }
    } 
}
