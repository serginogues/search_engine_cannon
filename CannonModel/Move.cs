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
        public CannonUtils.ISoldiers startPiece { get; set; }

        /// <summary>
        /// Cell to update 
        /// </summary>
        public int targetIndex { get; set; }

        public CannonUtils.ISoldiers targetPiece { get; set; }

        public CannonUtils.IMoves moveType { get; set; }

        public Move(int start_index, int target_index, CannonUtils.IMoves type, CannonUtils.ISoldiers start_piece, CannonUtils.ISoldiers target_piece)
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
