using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.Wire
{
    public enum SocketState
    {
        Idle =1,
        Connecting,
        Connected,
        Closing,
        Reconnecting,
        Refreshing
    }
}
