using System;
using System.Collections.Generic;
using System.Text;

namespace CannonModel
{
    public class Cell
    {
        public int myRow { get; set; }
        public int myColumn { get; set; }
        public CannonUtils.ISoldiers myPiece { get; set; }

        public Cell(int _row, int _column)
        {
            myRow = _row;
            myColumn = _column;
            myPiece = CannonUtils.ISoldiers.empty;
        }

        public Cell DeepCopy()
        {
            return new Cell(myRow, myColumn)
            {
                myPiece = myPiece,
                myRow = myRow,
                myColumn = myColumn
            };
        }
    }
}
