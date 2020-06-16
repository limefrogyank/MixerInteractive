using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MixerInteractive.State
{
    public class Scene
    {
        public string SceneID { get; set; }
        public Dictionary<string, IControl> Controls { get; set; }
        public object Groups { get; set; }
        public Meta Meta { get; set; }

        private Client _client;
        private StateFactory _stateFactory = new StateFactory();

        private ISubject<IControl> _controlAdded = new Subject<IControl>();
        private ISubject<string> _controlDeleted = new Subject<string>();

        public Scene(SceneData sceneData)
        {
            SceneID = sceneData.SceneID;
            Meta = sceneData.Meta;
        }

        public void SetClient(Client client)
        {
            _client = client;
            _stateFactory.SetClient(client);
        }

        public IEnumerable<IControl> OnControlsCreated(IEnumerable<IControlData> controlDatas)
        {
            return Controls.Values.Select(control => OnControlCreated(control));
        }

        private IControl OnControlCreated(IControlData controlData)
        {
            if (Controls.TryGetValue(controlData.ControlID, out var control))
            {

                
                return control;
            }





            return control;
        }

        public void OnControlsDeleted(IEnumerable<IControlData> controlDatas)
        {
            foreach (var control in Controls.Values)
            {
                OnControlDeleted(control);
            }
        }

        private void OnControlDeleted(IControlData controlData)
        {
                Controls.Remove(controlData.ControlID);
                _controlDeleted.OnNext(controlData.ControlID);
        }

        public void OnControlsUpdated(IEnumerable<IControlData> controlDatas)
        {
            foreach (var controlData in controlDatas)
            {
                OnControlUpdated(controlData);
            }
        }

        private void OnControlUpdated(IControlData controlData)
        {
            if (Controls.TryGetValue(controlData.ControlID, out var control))
            {
                control.OnUpdate(controlData);                
            }
        }


        public void Update(SceneData sceneData)
        {
            if (sceneData.Meta != null)
            {
                if (this.Meta != null)
                    Meta.Merge(sceneData.Meta);
                else
                    Meta = sceneData.Meta;
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
