using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopwatch
{
    public class StopwatchWithOffset : System.Diagnostics.Stopwatch
    {
        public TimeSpan StartOffset { get; private set; }

        public StopwatchWithOffset(TimeSpan startOffset)
        {
            StartOffset = startOffset;
        }

        public new TimeSpan Elapsed
        {
            get
            {
                return base.Elapsed.Add(StartOffset);
            }
        }

        public new void Reset()
        {
            StartOffset = new TimeSpan(0);
            base.Reset();
        }
    }
}
