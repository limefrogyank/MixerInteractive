using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixerInteractive.State
{
    public class Meta : Dictionary<string,object>
    {
        public void Merge(Meta metaToMerge)
        {
            foreach (var i in metaToMerge)
            {
                if (this.ContainsKey(i.Key))
                {
                    this[i.Key] = i.Value;
                }
                else
                {
                    this.Add(i.Key, i.Value);
                }
            }
        }
    }

    public class MetaProperty
    {
        public System.Text.Json.JsonElement Value { get; set; }
    }


}
