using MixerInteractive.InteractiveError;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MixerInteractive.Wire
{
    public class Reply
    {
        [JsonPropertyName("type")]
        public string Type => "reply";

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("result")]
        public object Result { get; set; }

        [JsonPropertyName("error")]
        public Base Error { get; set; }

        public Reply(int id, object result, Base error = null)
        {
            Id = id;
            Result = result;
            Error = error;
        }

        public static Reply FromSocket(Message message)
        {
            if (message.Error != null)
            {
                var baseErr = InteractiveError.ErrorUtil.FromSocketMessage(message.Error);

                return new Reply(message.Id, message.Result, baseErr);
            }
            else
                return new Reply(message.Id, message.Result);
        }

        public static Reply FromError(int id, InteractiveError.Base error)
        {
            return new Reply(id, null, error);
        }

    }

}
