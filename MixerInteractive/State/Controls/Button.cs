using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

        private ISubject<Tuple<ButtonInput, Participant>> _mouseDown = new Subject<Tuple<ButtonInput, Participant>>();
        private ISubject<Tuple<ButtonInput, Participant>> _mouseUp = new Subject<Tuple<ButtonInput, Participant>>();

        [JsonIgnore] public IObservable<Tuple<ButtonInput, Participant>> OnMouseDown => _mouseDown.AsObservable();
        [JsonIgnore] public IObservable<Tuple<ButtonInput, Participant>> OnMouseUp => _mouseUp.AsObservable();

        public Button(ControlData controlData) : base()
        {
            controlData.CopyPropertiesTo(this);
        }

        public override void ReceiveInput(Input input, Participant participant)
        {
            ButtonInput buttonInput = new ButtonInput();
            input.CopyPropertiesTo(buttonInput);
            if (buttonInput.Event == "mousedown")
                _mouseDown.OnNext(new Tuple<ButtonInput, Participant>(buttonInput, participant));
            else if (buttonInput.Event == "mouseup")
                _mouseUp.OnNext(new Tuple<ButtonInput, Participant>(buttonInput, participant));
        }

        public override void OnUpdate(ControlData controlData)
        {
            controlData.CopyPropertiesTo(this);
            _updated.OnNext(this);
        }

        public Task SetTextAsync(string text)
        {
            return UpdateAttributeAsync("text", text);
        }

        public Task SetTextSizeAsync(string text)
        {
            return UpdateAttributeAsync("textSize", text);
        }

        public Task SetBorderColorAsync(string text)
        {
            return UpdateAttributeAsync("borderColor", text);
        }

        public Task SetBackgroundColorAsync(string text)
        {
            return UpdateAttributeAsync("backgroundColor", text);
        }

        public Task SetBackgroundImageAsync(string text)
        {
            return UpdateAttributeAsync("backgroundImage", text);
        }

        public Task SetFocusColorAsync(string text)
        {
            return UpdateAttributeAsync("focusColor", text);
        }

        public Task SetAccentColorAsync(string text)
        {
            return UpdateAttributeAsync("accentColor", text);
        }

        public Task SetTextColorAsync(string text)
        {
            return UpdateAttributeAsync("textColor", text);
        }

        public Task SetTooltipAsync(string text)
        {
            return UpdateAttributeAsync("tooltip", text);
        }

        public Task SetProgressAsync(double text)
        {
            return UpdateAttributeAsync("progress", text);
        }

        public Task SetCooldownAsync(long text)
        {
            return UpdateAttributeAsync("cooldown", text);
        }

        public Task SetCostAsync(int text)
        {
            return UpdateAttributeAsync("cost", text);
        }

    }
}
