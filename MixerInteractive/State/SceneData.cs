using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.State
{
    public class SceneData
    {
        public string SceneID { get; set; }
        public IEnumerable<IControlData> Controls { get; set; }
        public Meta Meta { get; set; }

    }
}
