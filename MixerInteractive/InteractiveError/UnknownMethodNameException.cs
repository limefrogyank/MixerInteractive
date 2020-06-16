using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public class UnknownMethodNameException : BaseError
    {
        public UnknownMethodNameException(string message): base(message)
        { }
    }
}
