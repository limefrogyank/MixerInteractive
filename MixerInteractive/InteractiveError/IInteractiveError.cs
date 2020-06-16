using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.InteractiveError
{
    public interface IInteractiveError
    {
        int Code { get; set; }
        string Message { get; set; }
        string Path { get; set; }
    }
}
