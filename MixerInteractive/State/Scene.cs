using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MixerInteractive.State
{
    public class Scene
    {
        [JsonPropertyName("sceneID")] public string SceneID { get; set; }
        [JsonPropertyName("controls")] public Dictionary<string, Control> Controls { get; set; } = new Dictionary<string, Control>();
        [JsonPropertyName("groups")] public object Groups { get; set; }
        [JsonPropertyName("meta")] public Meta Meta { get; set; }

        private Client _client;
        private StateFactory _stateFactory = new StateFactory();

        private ISubject<Control> _controlAdded = new Subject<Control>();
        private ISubject<string> _controlDeleted = new Subject<string>();

        public Scene()
        { }

        public Scene(SceneData sceneData)
        {
            SceneID = sceneData.SceneID;
            Meta = sceneData.Meta.HasValue ? sceneData.Meta.Value : null;
        }

        public void SetClient(Client client)
        {
            _client = client;
            _stateFactory.SetClient(client);
        }

        public IEnumerable<Control> OnControlsCreate(IEnumerable<ControlData> controlDatas)
        {
            return controlDatas?.Select(control => OnControlCreate(control)).ToList();
        }

        private Control OnControlCreate(ControlData controlData)
        {
           // var controlID = controlData.GetProperty("controlID").GetString();
            if (Controls.TryGetValue(controlData.ControlID, out var control))
            {
                OnControlUpdate(controlData);                
                return control;
            }
            //var controlKind = controlData.GetProperty("kind").GetString();
            control = _stateFactory.CreateControl(controlData.Kind, controlData, this);
            Controls.Add(control.ControlID, control);
            _controlAdded.OnNext(control);

            return control;
        }

        public void OnControlsDelete(IEnumerable<ControlData> controlDatas)
        {
            foreach (var control in controlDatas)
            {
                OnControlDelete(control);
            }
        }

        private void OnControlDelete(ControlData controlData)
        {
            //var controlID = controlData.GetProperty("controlID").GetString();
            var controlID = controlData.ControlID;
            Controls.Remove(controlID);
            _controlDeleted.OnNext(controlID);
        }

        public void OnControlsUpdate(IEnumerable<ControlData> controlDatas)
        {
            foreach (var controlData in controlDatas)
            {
                OnControlUpdate(controlData);
            }
        }

        private void OnControlUpdate(ControlData controlData)
        {
            //var controlID = controlData.GetProperty("controlID").GetString();
            if (Controls.TryGetValue(controlData.ControlID, out var control))
            {
                control.OnUpdate(controlData);                
            }
        }
               
        public void Update(SceneData sceneData)
        {
            if (sceneData.Meta.HasValue)
            {
                if (this.Meta != null)
                    Meta.Merge(sceneData.Meta.Value);
                else
                    Meta = sceneData.Meta.Value;
            }
        }

        public void Destroy()
        {
            foreach (var control in Controls)
            {
                throw new NotImplementedException();
            }
        }

    }
}
