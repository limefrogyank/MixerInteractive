using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;

namespace MixerInteractive.State
{
    public class Control : IControl
    {
        public Client Client { get; set; }
        public string ControlID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Disabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Kind { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Meta Meta { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public GridPlacement Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        protected Scene Scene { get; set; }

        private ISubject<IControl> _updated = new Subject<IControl>();

        public void OnUpdate(IControlData controlData)
        {
            this.Disabled = controlData.Disabled;
            this.Kind = controlData.Kind;
            this.Meta = controlData.Meta;
            this.Position = controlData.Position;
            this.ControlID = controlData.ControlID;

            _updated.OnNext(this);
        }

    }
}
