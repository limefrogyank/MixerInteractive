﻿using MixerInteractive.InteractiveError;
using MixerInteractive.Methods;
using MixerInteractive.Wire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _methodHandler.AddHandler("hello",  method =>
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
            var p = (Dictionary<string, object>)result;
            return ((JsonElement)p["time"]).GetInt64();            
        }

        public Task<object> ExecuteAsync(string methodName, object parameters, bool discard)
        {
            return Socket.ExecuteAsync(methodName, parameters, discard);
        }

    }  
}