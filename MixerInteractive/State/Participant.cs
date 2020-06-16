using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MixerInteractive.State
{
    public class Participant
    {
        [JsonPropertyName("sessionID")]
        public string SessionID { get; set; }

        [JsonPropertyName("userID")]
        public long UserID { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("lastInputAt")]
        public long LastInputAt { get; set; }

        [JsonPropertyName("connectedAt")]
        public long ConnectedAt { get; set; }

        [JsonIgnore]
        public string ReadableConnectedAt => DateTimeOffset.FromUnixTimeMilliseconds(ConnectedAt).ToLocalTime().ToString();

        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; }

        [JsonPropertyName("groupID")]
        public string GroupID { get; set; }

        [JsonPropertyName("channelGroups")]
        public IEnumerable<string> ChannelGroups { get; set; }

        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }

        [JsonPropertyName("anonymous")]
        public bool Anonymous { get; set; }
    }
}
