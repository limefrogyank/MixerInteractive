using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.State
{
    public interface IGroupData
    {
        string GroupID { get; set; }
        string SceneID { get; set; }
        Meta Meta { get; set; }
    }
}
