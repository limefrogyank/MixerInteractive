using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.Wire
{
    public enum PacketState
    {
        Pending=1,
        Sending,
        Replied,
        Cancelled
    }
}
