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
        public Cell OldCell { get; set; }
        /// <summary>
        /// Cell to update 
        /// </summary>
        public Cell NewCell { get; set; }

        public CannonUtils.IMoves Type { get; set; }

        public Move(Cell old_c, Cell new_c, CannonUtils.IMoves type)
        {
            OldCell = old_c;
            NewCell = new_c;
            Type = type;
        }

        public Move DeepCopy()
        {
            return new Move(OldCell.DeepCopy(), NewCell.DeepCopy(), Type);
        }
    }
}
