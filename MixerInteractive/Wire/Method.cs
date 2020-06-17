using MixerInteractive.InteractiveError;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MixerInteractive.Wire
{
    public interface IMethod
    {
        Type GenericType { get; set; }    
    }

    public class Method
    {
        public readonly string Type = "method";

        [JsonPropertyName("seq")]
        public int Seq { get; set; }
        //public Type GenericType { get; set; }

        [JsonPropertyName("method")]
        public string Name { get; set; }

        [JsonPropertyName("params")]
        public JsonElement Parameters { get; set; }

        [JsonPropertyName("discard")]
        public bool Discard { get; set; } = false;

        [JsonPropertyName("id")]
        public int Id { get; set; }

        public Method()
        {
            var random = new Random();
            Id = random.Next();

        }

        public Method(string method, JsonElement parameters, bool discard)
        {
            Name = method;
            Parameters = parameters;
            Discard = discard;

            var random = new Random();
            Id = random.Next();
        }

        public Method(string method, JsonElement parameters, bool discard, int id)
        {
            Name = method;
            Parameters = parameters;
            Discard = discard;
            Id = id;
            //GenericType = typeof(T);
        }

        public static Method FromSocket(Message message)
        {
            return new Method(message.Method, message.Params, message.Discard, message.Id);
        }

        public Reply Reply(Dictionary<string,object> result, Base error)
        {
            return new Reply(Id, result, error);
        }
    }
}
