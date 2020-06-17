using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MixerInteractive.State
{
    public class ControlData 
    {
        [JsonPropertyName("controlID")] public string ControlID { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
        [JsonPropertyName("kind")] public string Kind { get; set; }
        [JsonPropertyName("meta")] public Meta Meta { get; set; }
        [JsonPropertyName("position")] public IEnumerable<GridPlacement> Position { get; set; }
    }
}
