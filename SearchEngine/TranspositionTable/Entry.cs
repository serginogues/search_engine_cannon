using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public class Entry
    {
        public int Value { get; set; }
        public AIUtils.ITTEntryFlag Flag { get; set; }
        public int BestMove { get; set; }
        public int Depth { get; set; }
        public long EntryKey { get; set; }
        public List<int> ChildList { get; set; }

        public Entry(long key_)
        {
            Value = 0;
            Flag = AIUtils.ITTEntryFlag.exact_value;
            BestMove = 0;
            Depth = 0;
            EntryKey = key_;
        }
    }
}
