using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive
{
    public class GameClientOptions
    {
        public int VersionId { get; set; }
        public string ShareCode { get; set; }
        public string AuthToken { get; set; }
        public string DiscoveryUrl { get; set; }
    }
}
