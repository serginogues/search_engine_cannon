using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public class Entry
    {
        /// <summary>
        /// The evaluation score for the position. 
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// There are 3 node types, Exact, Alpha and Beta.  
        /// Exact means this is an exact score for the tree.  
        /// However in the events that an alpha beta cut-off occurs we can’t use the score as an exact score.  
        /// An Alpha Node Type means the value of the node was at most equal to Score.  The Beta Node Type means the value is at leastequal to score. 
        /// </summary>
        public AIUtils.eTTEntryFlag Flag { get; set; }
        public int BestMove { get; set; }
        /// <summary>
        /// The depth remaining in the alpha beta search.  So depth 5 would mean the score is recorded for a 5 ply search.
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// Store 32bit as key
        /// </summary>
        public ulong zobristHashKey { get; set; }
        /// <summary>
        /// Set to false when Entry is retrieved during a Search.
        /// True by default.
        /// </summary>
        public bool Ancient { get; set; }

        public Entry(ulong key_)
        {
            Score = 0;
            Flag = AIUtils.eTTEntryFlag.exact_value;
            BestMove = 0;
            Depth = 0;
            zobristHashKey = key_;
            Ancient = true;
        }
    }
}
