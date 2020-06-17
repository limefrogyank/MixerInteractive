using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MixerInteractive.State.Controls
{
    public class Input
    {
        [JsonPropertyName("controlID")] public string ControlID { get; set; }
        [JsonPropertyName("event")] public string Event { get; set; }
    }

    public class ButtonInput : Input
    {
        [JsonPropertyName("button")] public int Button { get; set; }
    }
      
    public class JoystickInput : Input
    {
        [JsonPropertyName("x")] public double X { get; set; }
        [JsonPropertyName("y")] public double Y { get; set; }
    }

    public class ScreenInput : Input
    {
        [JsonPropertyName("x")] public double X { get; set; }
        [JsonPropertyName("y")] public double Y { get; set; }
    }

    public class TextboxInput : Input
    {
        [JsonPropertyName("value")] public string Value { get; set; }
    }

    public class InputEvent<T>
    {
        [JsonPropertyName("participantID")] public string ParticipantID { get; set; }
        [JsonPropertyName("input")] public T Input { get; set; }
        [JsonPropertyName("tranactionID")] public string TransactionID { get; set; }
    }

    public class TransactionCapture
    {
        [JsonPropertyName("transactionID")] public string TransactionID { get; set; }
    }
}
