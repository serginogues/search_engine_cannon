using System;
using System.Collections.Generic;
using System.Text;

namespace CannonModel
{
    public class Cell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public CannonUtils.ISoldiers Piece { get; set; }

        public Cell(int _row, int _column)
        {
            Row = _row;
            Column = _column;
            Piece = CannonUtils.ISoldiers.empty;
        }

        public Cell DeepCopy()
        {
            return new Cell(Row, Column)
            {
                Piece = Piece,
                Row = Row,
                Column = Column
            };
        }
    }
}
