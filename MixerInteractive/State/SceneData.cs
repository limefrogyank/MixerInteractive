using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MixerInteractive.State
{
    public class SceneData 
    {
        [JsonPropertyName("sceneID")] public string SceneID { get; set; }
        [JsonPropertyName("controls")] public IEnumerable<ControlData> Controls { get; set; }
        [JsonPropertyName("meta")] public Optional<Meta> Meta { get; set; }

    }
}
