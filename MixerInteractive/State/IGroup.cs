using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.State
{
    public interface IGroup : IGroupData
    {
        IObservable<IGroup> OnUpdated { get; }
        IObservable<IGroup> OnDeleted { get; }
    }
}
