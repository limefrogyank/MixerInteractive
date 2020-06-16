using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public class CancelledException : BaseError
    {
        public CancelledException() : base("Cancelled")
        {
        }
    }
}
