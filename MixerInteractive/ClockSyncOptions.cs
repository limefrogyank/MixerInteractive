using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MixerInteractive
{
    public class ClockSyncOptions
    {
        public int CheckInterval { get; set; } = 30 * 1000;
        public int SampleSize { get; set; } = 3;
        public int Threshold { get; set; } = 1000;
        public Func<Task<long>> SampleFunc { get; set; }
        public int SampleDelay { get; set; } = 5000;
    }
}
