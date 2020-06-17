using MixerInteractive.State.Controls;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixerInteractive.State
{
    public class Control : ControlData
    {
        [JsonIgnore] public Client Client { get; set; }
        //[JsonPropertyName("controlID")] public string ControlID { get; set; }
        //[JsonPropertyName("disabled")] public bool Disabled { get; set; }
        //[JsonPropertyName("kind")] public string Kind { get; set; }
        //[JsonPropertyName("meta")] public Meta Meta { get; set; }
        //[JsonPropertyName("position")] public IEnumerable<GridPlacement> Position { get; set; }
        [JsonIgnore] protected Scene Scene { get; set; }

        private ISubject<Control> _updated = new Subject<Control>();
        private ISubject<Tuple<object, Participant>> _inputEvent = new Subject<Tuple<object, Participant>>();

        public Control(ControlData controlData)
        {
            controlData.CopyPropertiesTo(this);
        }

        public void OnUpdate(JsonElement controlData)
        {
            var controlD = JsonSerializer.Deserialize<ControlData>(controlData.GetRawText());
            this.Disabled = controlD.Disabled;
            this.Kind = controlD.Kind;
            this.Meta = controlD.Meta;
            this.Position = controlD.Position;
            this.ControlID = controlD.ControlID;

            _updated.OnNext(this);
        }

        public void ReceiveInput(object input, Participant participant)
        {
            _inputEvent.OnNext(new Tuple<object, Participant>(input, participant));
        }

        protected Task SendInputAsync<K>(K input)
            where K: Input
        {
            input.ControlID = this.ControlID;
            return this.Client.GiveInputAsync(input);
        }

        protected Task UpdateAttributeAsync(PropertyInfo attribute, object value)
        {
            ControlData packet = default;

            packet.ControlID = this.ControlID;

            attribute.SetValue(packet, value);

            return this.Client.UpdateControlsAsync(new SceneData { SceneID = Scene.SceneID, Controls = new JsonElement[] {  } });
        }
                
        
    }
}
