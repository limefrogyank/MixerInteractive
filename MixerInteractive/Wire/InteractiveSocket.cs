using ConcurrentCollections;
using MixerInteractive.InteractiveError;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MixerInteractive.Wire
{
    public class InteractiveSocket
    {
        private SocketOptions _options;

        private IObservable<string> _message;//= new Subject<string>();
        private ISubject<Unit> _open = new Subject<Unit>();
        private ISubject<object> _close = new Subject<object>();
        private ISubject<Method> _method = new Subject<Method>();
        private ISubject<Reply> _reply = new Subject<Reply>();
        private ISubject<Base> _error = new Subject<Base>();
        private ISubject<object> _send = new Subject<object>();

        public IObservable<Method> MethodObs => _method.AsObservable();
        public IObservable<Unit> OpenObs => _open.AsObservable();
        public IObservable<string> MessageObs => _message.AsObservable();
        public IObservable<object> CloseObs => _close.AsObservable();
        public IObservable<Base> ErrorObs => _error.AsObservable();
        public IObservable<object> SendObs => _send.AsObservable();

        private SocketState _state = SocketState.Idle;
        private ConcurrentHashSet<Packet> _queue = new ConcurrentHashSet<Packet>();
        private int _lastSequenceNumber = 0;
        private int _endpointIndex = 0;

        private System.Timers.Timer _reconnectTimer;

        private ClientWebSocket _socket;
        private Task _sendTask = Task.CompletedTask;


        public SocketState State => _state;

        bool _loopRunning;
        public InteractiveSocket(SocketOptions options)
        {
            _message = Observable.Create<string>(obs =>
            {
                return Observable.Merge(_open.Select(x => true), _close.Select(x => false)).Subscribe(async isOpen =>
                {
                    if (isOpen && !_loopRunning)
                    {
                        _loopRunning = true;
                        Debug.WriteLine("Running loop");
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[8192]);
                        WebSocketReceiveResult result;
                        string stringMessage = "";
                        while (_socket.State == WebSocketState.Open)
                        {
                            do
                            {                                
                                result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                                stringMessage += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                            }
                            while (!result.EndOfMessage);
                            Debug.WriteLine(stringMessage);
                            obs.OnNext(stringMessage);
                            stringMessage = "";
                        }
                    }
                });
            });

            SetOptions(options);

            _message.Subscribe(msg =>
            {
                ExtractMessage(msg);
            });

            _open.Subscribe(_ =>
            {
                _options.ReconnectionPolicy.Reset();
                _state = SocketState.Connected;
                foreach (var packet in _queue)
                {
                    var task = SendAsync(packet);
                }
            });

            _close.Subscribe(closeEvent =>
            {
                throw new NotImplementedException();
            });

            
        }


        public void Close()
        {
            if (_state == SocketState.Reconnecting)
            {
                _reconnectTimer.Stop();
                _state = SocketState.Idle;
                return;
            }
            if (State != SocketState.Idle)
            {
                _state = SocketState.Closing;
                _ = _socket.CloseAsync(WebSocketCloseStatus.Empty, "Closed normally.", CancellationToken.None);
                foreach (var packet in _queue)
                {
                    packet.Cancel();
                }
                _queue.Clear();
            }
        }


        public SocketOptions GetOptions()
        {
            return _options;
        }

        public void SetOptions(SocketOptions options)
        {
            _options = options;
        }

        public async Task<InteractiveSocket> ConnectAsync()
        {
            if (_state == SocketState.Closing)
            {
                _state = SocketState.Refreshing;
                return this;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-Protocol-Version", "2.0");

            foreach (var item in _options.ExtraHeaders)
            {
                headers.Add(item.Key, item.Value.ToString());
            }

            var url = GetUrl();

            if (_options.AuthToken != null)
            {
                headers.Add("Authorization", "Bearer%20" + _options.AuthToken);
            }

            url += "?";
            foreach (var item in _options.QueryParams.Concat(headers))
            {
                url += $"{item.Key}={item.Value}&";
            }
            url = url.TrimEnd('&');

            _state = SocketState.Connecting;
            _socket = new System.Net.WebSockets.ClientWebSocket();
            //var options = _socket.Options;
            //options.KeepAliveInterval()
            
            await _socket.ConnectAsync(new Uri(url), CancellationToken.None);
            _state = SocketState.Connected;
            _open.OnNext(Unit.Default);

            return this;
        }

        public string GetUrl()
        {
            var addresses = _options.Urls;
            return _options.Urls.ElementAt(_endpointIndex++ % addresses.Count());
        }

        public async Task<object> SendAsync(Packet packet)
        {
            if (packet.GetState() == PacketState.Cancelled)
                throw new CancelledException();
            
            _queue.Add(packet);

            if (_state != SocketState.Connected)
            {
                var sendTask = packet.SendSubject.Amb(Observable.Timer(TimeSpan.FromMilliseconds(120*1000)).Select<long, Dictionary<string,object>>(x => null)).Where(x=> x != null).Take(1).ToTask();
                var cancelTask = packet.CancelObs.Amb(Observable.Timer(TimeSpan.FromMilliseconds(120 * 1000)).Select<long, Unit>(x => Unit.Default)).Take(1).ToTask();

                var firstFinishedTask = await Task.WhenAny(sendTask, cancelTask);
                if (firstFinishedTask == cancelTask)
                    throw new CancelledException();

                return await sendTask;
            }

            var timeout = packet.GetTimeout(_options.ReplyTimeout);
            var d = Observable.Return<Reply>(null);
            var replyTask = _reply.Amb(Observable.Timer(TimeSpan.FromMilliseconds(timeout)).Select<long,Reply>(x=> null)).Where(x => x != null && x.Id == packet.Id).Take(1).ToTask();
            var cancelTask2 = packet.CancelObs.Amb(Observable.Timer(TimeSpan.FromMilliseconds(timeout+1)).Select<long, Unit>(x => Unit.Default)).Take(1).ToTask();
            var closeTask = _close.Amb(Observable.Timer(TimeSpan.FromMilliseconds(timeout+1)).Select<long, object>(x => new object())).Take(1).ToTask();

            

            var race = Task.WhenAny(replyTask, cancelTask2, closeTask);//.ContinueWith(finishedTask =>

            _send.OnNext(race);
            packet.SetState(PacketState.Sending);
            _ = SendPacketInnerAsync(packet);
            

            var finishTask = await race;

            if (finishTask == replyTask)
            {
                _queue.TryRemove(packet);
                if (replyTask.Result.Error != null)
                    throw replyTask.Result.Error;

                packet.SendSubject.OnNext(replyTask.Result.Result);
                return replyTask.Result.Result;
            }
            else if (finishTask == cancelTask2)
                throw new CancelledException();
            else if (finishTask == closeTask)
            {
                if (!_queue.Contains(packet))
                    return null;

                packet.SetState(PacketState.Pending);
                return await SendAsync(packet);
            }
            else
                return null;
            
        }

        private Task SendPacketInnerAsync(Packet packet)
        {
            return SendRawAsync(packet.SetSequenceNumber(_lastSequenceNumber));
        }

        public Task ReplyAsync(Reply reply)
        {
            return SendRawAsync(reply);
        }

        private Task SendRawAsync(object reply)
        {
            string data;
            if (reply is Packet)
            {
                data = (reply as Packet).Serialize();
            }
            else
            {
                data = JsonSerializer.Serialize(reply);
            }

            //emit send payload
            _send.OnNext(data);

            Debug.WriteLine($"Starting SendTask: {data}");
            _sendTask = _sendTask.ContinueWith(t => _socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, CancellationToken.None)).Unwrap();
            //_sendTask = _socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, CancellationToken.None);
            return _sendTask;
        }

        private void ExtractMessage(string message)
        {
            JsonDocument jsonDoc;
            try
            {

                var imessage = System.Text.Json.JsonSerializer.Deserialize<Message>(message);
                _lastSequenceNumber = imessage.Seq;
                //var t = jsonDoc.RootElement.GetProperty("type").GetString();
                switch (imessage.Type)
                {
                    case "method":
                        _method.OnNext(Method.FromSocket(imessage));
                        break;
                    case "reply":
                        _reply.OnNext(Reply.FromSocket(imessage));
                        break;
                    default:
                        throw new MessageParseException($"Unknown message type {imessage.Type}");
                }

                //jsonDoc = JsonDocument.Parse(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Not valid JSON: {message}");
                return;
                //throw new MessageParseException("Message returned was not valid JSON");
            }

            //if (jsonDoc.RootElement.TryGetProperty("seq", out var seq))
            //{
            //    seq.TryGetInt32(out _lastSequenceNumber);
            //}

            //var imessage = System.Text.Json.JsonSerializer.Deserialize<IMessage>(message);

            //var t = jsonDoc.RootElement.GetProperty("type").GetString();
            //switch (imessage.Type)
            //{
            //    case "method":
            //        _method.OnNext(Method.FromSocket(imessage));
            //        break;
            //    case "reply":
            //        _reply.OnNext(Reply.FromSocket(imessage));
            //        break;
            //    default:
            //        throw new MessageParseException($"Unknown message type {imessage.Type}");
            //}
            
        }


        public Task<object> ExecuteAsync(string methodName, object parameters, bool discard = false)
        {
            var method = new Method(methodName, parameters, discard);
            return SendAsync(new Packet(method));
        }


    }
}
