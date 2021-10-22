using System;
using System.Collections.Generic;
using System.Text;

namespace CannonModel
{
    public class Move
    {
        /// <summary>
        /// Soldier that executes move
        /// </summary>
        public int startIndex { get; set; }
        public CannonUtils.eSoldiers startPiece { get; set; }

        /// <summary>
        /// Cell to update 
        /// </summary>
        public int targetIndex { get; set; }

        public CannonUtils.eSoldiers targetPiece { get; set; }

        public CannonUtils.eMoves moveType { get; set; }

        public Move(int start_index, int target_index, CannonUtils.eMoves type, CannonUtils.eSoldiers start_piece, CannonUtils.eSoldiers target_piece)
        {
            startIndex = start_index;
            targetIndex = target_index;
            moveType = type;
            targetPiece = target_piece;
            startPiece = start_piece;
        }

        public Move deepCopy() => new Move(startIndex, targetIndex, moveType, startPiece, targetPiece);
    }
}
