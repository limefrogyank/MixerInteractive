using MixerInteractive.InteractiveError;
using MixerInteractive.Methods;
using MixerInteractive.State;
using MixerInteractive.State.Controls;
using MixerInteractive.Wire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MixerInteractive
{
    public class Client
    {
        public ClientType ClientType { get; set; }

        public State.State State { get; set; }

        protected InteractiveSocket Socket { get; set; }

        private MethodHandlerManager _methodHandler = new MethodHandlerManager();

        private Subject<Unit> _open = new Subject<Unit>();
        private Subject<Base> _error = new Subject<Base>();
        private Subject<string> _message = new Subject<string>();
        private Subject<object> _send = new Subject<object>();
        private Subject<object> _close = new Subject<object>();
        private Subject<Unit> _hello = new Subject<Unit>();
        public IObservable<Unit> OpenObs => _open.AsObservable();
        public IObservable<object> CloseObs => _close.AsObservable();

        protected Client(ClientType clientType)
        {
            this.ClientType = clientType;
            this.State = new State.State(clientType);
            this.State.SetClient(this);
            _methodHandler.AddHandler("hello", method =>
           {
               _hello.OnNext(Unit.Default);
               return null;
           });
        }

        protected async Task<Client> OpenAsync(SocketOptions options)
        {
            this.State.Reset();
            CreateSocket(options);
            await Socket.ConnectAsync();
            return this;
        }

        public async Task<Tuple<IEnumerable<Group>, IEnumerable<Scene>>> SynchronizeStateAsync()
        {
            var groupsTask = SynchronizeGroupsAsync();
            var scenesTask = SynchronizeScenesAsync();
            await Task.WhenAll(groupsTask,scenesTask);


            return new Tuple<IEnumerable<Group>, IEnumerable<Scene>>(groupsTask.Result, scenesTask.Result);
        }

        public async Task<IEnumerable<Group>> SynchronizeGroupsAsync()
        {
            var groups = await GetGroupsAsync();
            return groups;
        }

        public async Task<IEnumerable<Group>> GetGroupsAsync()
        {
            var result = await ExecuteAsync("getGroups", null, false);
            //var groups = JsonSerializer.Deserialize<IEnumerable<Group>>(((JsonElement)((Dictionary<string, object>)result)["groups"]).GetRawText());
            var groups = ((JsonElement)result).GetProperty("groups").EnumerateArray().Select(x => JsonSerializer.Deserialize<Group>(x.GetRawText())).ToList();
            return groups;
        }

        public async Task<IEnumerable<Scene>> SynchronizeScenesAsync()
        {
            var scenes = await GetScenesAsync();
            return State.SynchronizeScenes(scenes);
        }

        public async Task<IEnumerable<SceneData>> GetScenesAsync()
        {
            var result = await ExecuteAsync("getScenes", null, false);
            var scenes = ((JsonElement)result).GetProperty("scenes").EnumerateArray().Select(x=>JsonSerializer.Deserialize<SceneData>(x.GetRawText())).ToList();

            return scenes;
        }



        public Reply ProcessMethod(Method method)
        {
            return _methodHandler.Handle(method);
        }

        private void CreateSocket(SocketOptions socketOptions)
        {
            if (Socket != null)
            {
                if (Socket.State != SocketState.Closing)
                {
                    Socket.Close();
                }
                Socket = null;
            }

            Socket = new InteractiveSocket(socketOptions);
            Socket.MethodObs.Subscribe(async method =>
            {
                var clientReply = ProcessMethod(method);
                if (clientReply != null)
                {
                    await ReplyAsync(clientReply);
                    return;
                }

                var reply = State.ProcessMethod(method);
                if (clientReply != null)
                {
                    await ReplyAsync(reply);
                }

            });

            Socket.OpenObs.Subscribe(_ =>
            {
                _open.OnNext(_);
            });

            Socket.ErrorObs.Subscribe(err =>
            {
                _error.OnNext(err);
            });

            Socket.MessageObs.Subscribe(mesg =>
            {
                _message.OnNext(mesg);
            });

            Socket.SendObs.Subscribe(send =>
            {
                _send.OnNext(send);
            });

            Socket.CloseObs.Subscribe(close =>
            {
                _close.OnNext(close);
            });

        }

        public Task ReplyAsync(Reply reply)
        {
            return Socket.ReplyAsync(reply);
        }

        public async Task<long> GetTime()
        {
            var result=await ExecuteAsync("getTime", null, false);
            var p = (JsonElement)result;
            return p.GetProperty("time").GetInt64();            
        }

        public Task<object> ExecuteAsync(string methodName, object parameters, bool discard)
        {
            return Socket.ExecuteAsync(methodName, parameters, discard);
        }


        public virtual Task UpdateControlsAsync(object data)
        {
            throw new NotImplementedException();
        }

        public virtual Task GiveInputAsync(JsonElement input)
        {
            throw new NotImplementedException();
        }
    }  
}
