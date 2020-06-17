using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json.Serialization;

namespace MixerInteractive.State
{
    public class Group : IGroup
    {
        [JsonPropertyName("groupID")] public string GroupID { get; set; }
        [JsonPropertyName("sceneID")] public string SceneID { get; set; }
        [JsonPropertyName("meta")] public Meta Meta { get; set; }

        private Subject<IGroup> _updated = new Subject<IGroup>();
        private Subject<IGroup> _deleted = new Subject<IGroup>();

        [JsonIgnore] public IObservable<IGroup> OnUpdated => _updated.AsObservable();
        [JsonIgnore] public IObservable<IGroup> OnDeleted => _deleted.AsObservable();

        public Group()
        {

        }

        public Group(IGroupData groupData)
        {
            GroupID = groupData.GroupID;
            SceneID = groupData.SceneID;
            Meta = groupData.Meta;
        }

        public void Update(IGroupData data)
        {
            GroupID = data.GroupID;
            SceneID = data.SceneID;
            Meta = data.Meta;
            _updated.OnNext(this);
        }

        public void Destroy()
        {
            _deleted.OnNext(this);
        }
    }
}
