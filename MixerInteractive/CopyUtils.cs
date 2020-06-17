using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MixerInteractive
{
    public static class CopyUtils
    {
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();
            
            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Select(x=> (x,x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name) ).Any(x => x.x.Name == sourceProp.Name || x.Name == sourceProp.Name))
                {
                    var p = destProps.Select(x => (x, x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name)).First(x => x.x.Name == sourceProp.Name || x.Name == sourceProp.Name);
                    if (p.x.CanWrite)
                    { // check if the property can be set or no.
                        p.x.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }
            }
            //now check for overflow properties
            var overflowProps = sourceProps.FirstOrDefault(x => x.Name == "ExtensionData");
            if (overflowProps!= null)
            {
                var dic = (Dictionary<string,object>)overflowProps.GetValue(source);
                foreach (var pair in dic)
                {
                    if (destProps.Select(x => (x, x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name)).Any(x => x.x.Name == pair.Key || x.Name == pair.Key))
                    {
                        var p = destProps.Select(x => (x, x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name)).First(x => x.x.Name == pair.Key || x.Name == pair.Key);
                        if (p.x.CanWrite)
                        { // check if the property can be set or no.
                            var t = p.x.PropertyType;
                            if (t == typeof(string))
                            {
                                p.x.SetValue(dest, ((JsonElement)pair.Value).GetString(), null);
                            }
                            else if (t == typeof(int))
                            {
                                p.x.SetValue(dest, ((JsonElement)pair.Value).GetInt32(), null);
                            }
                            else if (t==typeof(long))
                            {
                                p.x.SetValue(dest, ((JsonElement)pair.Value).GetInt64(), null);
                            }
                            else if (t == typeof(double))
                            {
                                p.x.SetValue(dest, ((JsonElement)pair.Value).GetDouble(), null);
                            }

                        }
                    }
                }

            }
        }
    }
}
