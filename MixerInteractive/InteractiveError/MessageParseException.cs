using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public class MessageParseException : BaseError
    {
        public MessageParseException(string message) : base(message)
        { }
    }
}
