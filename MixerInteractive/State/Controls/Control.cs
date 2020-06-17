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

        protected ISubject<Control> _updated = new Subject<Control>();
        protected ISubject<Tuple<object, Participant>> _inputEvent = new Subject<Tuple<object, Participant>>();

        public Control()
        {

        }

        public Control(ControlData controlData)
        {
            controlData.CopyPropertiesTo(this);
        }

        public virtual void OnUpdate(ControlData controlData)
        {
            this.Disabled = controlData.Disabled;
            this.Kind = controlData.Kind;
            this.Meta = controlData.Meta;
            this.Position = controlData.Position;
            this.ControlID = controlData.ControlID;

            _updated.OnNext(this);
        }

        public virtual void ReceiveInput(Input input, Participant participant)
        {
            _inputEvent.OnNext(new Tuple<object, Participant>(input, participant));
        }

        protected Task SendInputAsync<K>(K input)
            where K: Input
        {
            input.ControlID = this.ControlID;
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(input));
            return this.Client.GiveInputAsync(doc.RootElement);
        }

        public virtual Task UpdateAsync(Dictionary<string, object> data)
        {
            if (data.ContainsKey("controlID"))
            {
                data.Add("controlID", this.ControlID);
            }

            return this.Client.UpdateControlsAsync(new Dictionary<string, object>()
            {
                { "sceneID", Scene.SceneID },
                { "controls", data
                }
            });
        }

        protected Task UpdateAttributeAsync(string propertyName, object value)
        {                                    
            return this.Client.UpdateControlsAsync(new Dictionary<string, object>() 
            {
                { "sceneID", Scene.SceneID },
                { "controls", new List<Dictionary<string,object>>()
                    {
                        new Dictionary<string,object>()
                        {
                            { "controlID", ControlID },
                            { propertyName, value }
                        }

                    }
                }
            });
        }

                
        
    }
}
