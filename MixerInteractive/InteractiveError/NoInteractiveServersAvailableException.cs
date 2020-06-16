using MixerInteractive.InteractiveError;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public class NoInteractiveServersAvailableException : BaseError
    {
        public NoInteractiveServersAvailableException(string message) : base(message)
        {

        }
    }
}
