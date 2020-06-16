using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.Wire
{
    public class ExponentialReconnectionPolicy : IReconnectionPolicy
    {
        private int _retries = 0;

        public long MaxDelay { get; set; }
        public long BaseDelay { get; set; }

        public ExponentialReconnectionPolicy(long maxDelay = 20 * 1000, long baseDelay = 500)
        {
            MaxDelay = maxDelay;
            BaseDelay = baseDelay;
        }

        public long Next()
        {
            return Math.Min(MaxDelay, (1 << _retries++) * BaseDelay);
        }

        public void Reset()
        {
            _retries = 0;
        }
    }
}
