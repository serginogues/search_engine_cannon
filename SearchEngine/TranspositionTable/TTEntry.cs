using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public class TTEntry
    {
        public int Value { get; set; }
        public AIUtils.ITTEntryFlag Flag { get; set; }
        public int BestMove { get; set; }
        public int Depth { get; set; }
        public int Key { get; set; }

        public TTEntry(int key_)
        {
            Value = 0;
            Flag = AIUtils.ITTEntryFlag.exact_value;
            BestMove = 0;
            Depth = 0;
            Key = key_;
        }

    }
}
