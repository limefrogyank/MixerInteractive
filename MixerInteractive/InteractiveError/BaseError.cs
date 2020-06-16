using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public class BaseError : Exception
    {
        public string Message { get; private set; }

        public BaseError(string message):base(message)
        {
            Message = message;
        }

        
    }
}
