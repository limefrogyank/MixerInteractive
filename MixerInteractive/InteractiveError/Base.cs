using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public class Base : BaseError
    {
        public int Code { get; set; }
        public string Path { get; set; }
        public Base(string message, int code): base(message)
        {
            Code = code;

            //skipped setProto
        }
    }
}
