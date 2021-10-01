using System;
using System.Collections.Generic;
using System.Text;

namespace CannonModel
{
    public class Cell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public CannonUtils.ISoldiers CurrentValue { get; set; }
        public CannonUtils.IMoves Action { get; set; }

        public Cell(int _row, int _column)
        {
            Row = _row;
            Column = _column;
            CurrentValue = CannonUtils.ISoldiers.empty;
            Action = CannonUtils.IMoves.none;
        }
    }
}
