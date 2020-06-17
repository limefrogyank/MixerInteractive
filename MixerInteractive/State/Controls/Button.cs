using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MixerInteractive.State.Controls
{
    public class Button : Control
    {
        [JsonPropertyName("text")] public string Text { get; set; }
        [JsonPropertyName("tooltip")] public string Tooltip { get; set; }
        [JsonPropertyName("cost")] public int Cost { get; set; }
        [JsonPropertyName("progress")] public double Progress { get; set; }
        [JsonPropertyName("cooldown")] public long Cooldown { get; set; }
        [JsonPropertyName("keyCode")] public int KeyCode { get; set; }
        [JsonPropertyName("textColor")] public string TextColor { get; set; }
        [JsonPropertyName("textSize")] public string TextSize { get; set; }
        [JsonPropertyName("borderColor")] public string BorderColor { get; set; }
        [JsonPropertyName("backgroundColor")] public string BackgroundColor { get; set; }
        [JsonPropertyName("backgroundImage")] public string BackgroundImage { get; set; }
        [JsonPropertyName("focusColor")] public string FocusColor { get; set; }
        [JsonPropertyName("accentColor")] public string AccentColor { get; set; }

        public Button(ButtonData controlData) : base(controlData)
        {

        }


    }
}
