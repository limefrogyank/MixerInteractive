using MixerInteractive.State.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MixerInteractive
{
    public class ParticipantClient : Client
    {
        public ParticipantClient(): base(ClientType.Participant)
        {

        }

        public override Task GiveInputAsync(JsonElement input)
        {
            return ExecuteAsync("giveInput", input, false);
        }
    }
}
