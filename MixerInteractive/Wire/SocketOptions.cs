using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MixerInteractive.Wire
{
    public class SocketOptions
    {
        public IReconnectionPolicy ReconnectionPolicy { get; set; } = new ExponentialReconnectionPolicy();
        public bool AutoReconnect { get; set; } = true;
        public IEnumerable<string> Urls { get; set; } = new List<string>();
        public string CompressionScheme { get; set; } = "none";
        public Dictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
        public string AuthToken { get; set; }
        public int ReplyTimeout { get; set; } = 10000;
        public int PingInterval { get; set; } = 10 * 1000;
        public Dictionary<string, object> ExtraHeaders { get; set; } = new Dictionary<string, object>();
        public Task ReconnectChecker { get; set; } = Task.CompletedTask;
        
    }
}
