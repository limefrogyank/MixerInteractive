using MixerInteractive.Wire;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
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
            return ExecuteAsync("ready", dic, false);
        }
    }
}
