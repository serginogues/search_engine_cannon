using System;
using System.Collections.Generic;
using System.Text;

namespace SearchEngine
{
    public static class AIUtils
    {
        public enum IType
        {
            MAX,
            MIN
        }

        public enum ITTEntryFlag
        {
            exact_value,
            lower_bound,
            upper_bound
        }
    }
}
