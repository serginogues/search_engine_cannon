using System;
using System.Collections.Generic;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public class TTEntry
    {
        /// <summary>
        /// Value from evaluation function
        /// </summary>
        public int Value { get; set; }
        public AIUtils.ITTEntryFlag Flag { get; set; }
        /// <summary>
        /// Best move for this state
        /// </summary>
        public int BestMove { get; set; }
        public int Depth { get; set; }
        /// <summary>
        /// Zobrist Hashing key
        /// </summary>
        public int EntryKey { get; set; }

        public TTEntry(int key_)
        {
            Value = 0;
            Flag = AIUtils.ITTEntryFlag.exact_value;
            BestMove = 0;
            Depth = 0;
            EntryKey = key_;
        }

    }
}
