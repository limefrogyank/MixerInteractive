using MixerInteractive.InteractiveError;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MixerInteractive.Wire
{
    public class Message
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public object Params { get; set; }

        [JsonPropertyName("result")]
        public Dictionary<string,object> Result { get; set; }

        [JsonPropertyName("seq")]
        public int Seq { get; set; }

        [JsonPropertyName("discard")]
        public bool Discard { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("error")]
        public Base Error { get; set; }
    }
}
