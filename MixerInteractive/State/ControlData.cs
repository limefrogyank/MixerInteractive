using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.State
{
    public class ControlData : IControlData
    {
        public string ControlID { get; set; }
        public string Kind { get; set; }
        public bool Disabled { get; set; }
        public Meta Meta { get; set; }
        public GridPlacement Position { get; set; }

    }
}
