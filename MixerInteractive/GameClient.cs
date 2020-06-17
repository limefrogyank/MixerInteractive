using MixerInteractive.State;
using MixerInteractive.Wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MixerInteractive
{
    public class GameClient : Client
    {
        EndpointDiscovery _discovery;
        WebSocket webSocket;

        public GameClient() : base(ClientType.GameClient)
        {            
            _discovery = new EndpointDiscovery();
        }

        public async Task OpenAsync(GameClientOptions options)
        {
            Dictionary<string, object> extraHeaders = new Dictionary<string, object>();
            extraHeaders.Add("X-Interactive-Version", options.VersionId);

            if (options.ShareCode != null)
            {
                extraHeaders.Add("X-Interactive-Sharecode", options.ShareCode);
            }

            var endpoints = await _discovery.RetrieveEndpointsAsync();

            var socketOptions = new SocketOptions()
            {
                AuthToken = options.AuthToken,
                Urls = endpoints,
                ExtraHeaders = extraHeaders
            };

            await base.OpenAsync(socketOptions);

        }

        public Task ReadyAsync(bool isReady=true)
        {
            var dic = new Dictionary<string, bool>();
            dic.Add("isReady", isReady);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(dic));
            return ExecuteAsync("ready", doc.RootElement, false);
        }

        public Task UpdateParticipantsAsync(IEnumerable<Participant> participants)
        {
            var dic = new Dictionary<string, object>();
            dic.Add("participants", participants);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(dic));
            return ExecuteAsync("updateParticipants", doc.RootElement, false);
        }

        public async Task<IEnumerable<Group>> CreateGroupsAsync(IEnumerable<Group> groups)
        {
            var dic = new Dictionary<string, object>();
            dic.Add("groups", groups);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(dic));
            var result = await ExecuteAsync("createGroups", doc.RootElement, false);
            return ((JsonElement)result).GetProperty("groups").EnumerateArray().Select(x=> JsonSerializer.Deserialize<Group>(x.GetRawText())).ToList();
        }


        public override Task UpdateControlsAsync(object data)
        {
            //var doc = JsonDocument.Parse(JsonSerializer.Serialize(data));
            return this.ExecuteAsync("updateControls", data, false);
        }
    }
}
