using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixerInteractive.State.Controls
{
    public class Label : Control
    {
        [JsonPropertyName("text")] public string Text { get; set; }
        [JsonPropertyName("textColor")] public string TextColor { get; set; }
        [JsonPropertyName("textSize")] public string TextSize { get; set; }

        [JsonPropertyName("underline")] public bool Underline { get; set; }
        [JsonPropertyName("bold")] public bool Bold { get; set; }
        [JsonPropertyName("italic")] public bool Italic { get; set; }

        public Label(ControlData controlData) : base(controlData)
        {

        }

        public Task SetTextAsync(string text)
        {
            return UpdateAttributeAsync("text", text);
        }

        public Task SetTextSizeAsync(string textSize)
        {
            return UpdateAttributeAsync("textSize", textSize);
        }

        public Task SetTextColorAsync(string textColor)
        {
            return UpdateAttributeAsync("textColor", textColor);
        }


    }
}
