using MixerInteractive.Methods;
using MixerInteractive.Wire;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MixerInteractive.State
{
    public class State
    {
        public bool IsReady { get; set; }

        private Client _client;

        private MethodHandlerManager _methodHandler = new MethodHandlerManager();

        private StateFactory _stateFactory = new StateFactory();

        private Subject<bool> _ready = new Subject<bool>();
        private Subject<Scene> _sceneCreated = new Subject<Scene>();
        private Subject<string> _sceneDeleted = new Subject<string>();
        private Subject<object> _worldUpdated = new Subject<object>();

        private Subject<Participant> _selfUpdate = new Subject<Participant>();
        private Subject<Participant> _participantJoin = new Subject<Participant>();
        private Subject<Participant> _participantLeave = new Subject<Participant>();

        public IObservable<bool> OnReady => _ready.AsObservable();
        public IObservable<Scene> OnSceneCreated => _sceneCreated.AsObservable();
        public IObservable<string> OnSceneDeleted => _sceneDeleted.AsObservable();
        public IObservable<object> OnWorldUpdated => _worldUpdated.AsObservable();
        public IObservable<Participant> OnSelfUpdate => _selfUpdate.AsObservable();
        public IObservable<Participant> OnParticipantJoin => _participantJoin.AsObservable();
        public IObservable<Participant> OnParticipantLeave => _participantLeave.AsObservable();


        private Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        private Dictionary<string, Participant> _participants = new Dictionary<string, Participant>();
        private Dictionary<string, object> _world = new Dictionary<string, object>();

        private ClockSync _clockSyncer;
        private long _clockDelta = 0;
        private ClientType _clientType;

        public Dictionary<string, Participant> Participants => _participants;

        public State(ClientType clientType)
        {
            _clientType = clientType;
            _clockSyncer = new ClockSync(new ClockSyncOptions { SampleFunc = () => _client.GetTime() });

            _methodHandler.AddHandler("onReady", method =>
            {
                var p = (JsonElement)method.Parameters;
                var isReady = p.GetProperty("isReady").GetBoolean();
                //if (p.TryGetValue("isReady", out var isReady))
                {
                    IsReady = (bool)isReady;
                    _ready.OnNext(IsReady);
                }
                return null;
            });

            // Scene Events
            _methodHandler.AddHandler("onSceneCreate", method => 
            {
                var p = (Dictionary<string, object>)method.Parameters;
                if (p.TryGetValue("scenes", out var scenes))
                {
                    foreach (var sceneData in (IEnumerable<SceneData>)scenes)
                    {
                        OnSceneCreate(sceneData);
                    }
                }
                return null;
            });
            _methodHandler.AddHandler("onSceneDelete", method => 
            {
                var p = (Dictionary<string, object>)method.Parameters;
                if (p.TryGetValue("sceneID", out var sceneID) && p.TryGetValue("reassignSceneID", out var reassignSceneID))
                {
                    OnSceneDelete((string)sceneID, reassignSceneID);
                }
                return null;
            });
            _methodHandler.AddHandler("onSceneUpdate", method => 
            {
                var p = (Dictionary<string, object>)method.Parameters;
                if (p.TryGetValue("scenes", out var scenes))
                {
                    foreach (var sceneData in (IEnumerable<SceneData>)scenes)
                    {
                        OnSceneUpdate(sceneData);
                    }
                }
                return null;
            });

            // Control Events
            _methodHandler.AddHandler("onControlCreate", method => 
            {
                var p = (Dictionary<string, object>)method.Parameters;
                if (p.TryGetValue("sceneID", out var sceneID))
                {
                    if (_scenes.TryGetValue((string)sceneID, out Scene scene))
                    {
                        if (p.TryGetValue("controls", out var controls))
                        {
                            scene.OnControlsCreated((IEnumerable<IControlData>)controls);
                        }
                    }
                }
                return null;
            });

            _methodHandler.AddHandler("onControlDelete", method => 
            {
                var p = (Dictionary<string, object>)method.Parameters;
                if (p.TryGetValue("sceneID", out var sceneID))
                {
                    if (_scenes.TryGetValue((string)sceneID, out Scene scene))
                    {
                        if (p.TryGetValue("controls", out var controls))
                        {
                            scene.OnControlsDeleted((IEnumerable<IControlData>)controls);
                        }
                    }
                }
                return null;
            });

            _methodHandler.AddHandler("onControlUpdate", method => 
            {
                var p = (Dictionary<string, object>)method.Parameters;
                if (p.TryGetValue("sceneID", out var sceneID))
                {
                    if (_scenes.TryGetValue((string)sceneID, out Scene scene))
                    {
                        if (p.TryGetValue("controls", out var controls))
                        {
                            scene.OnControlsUpdated((IEnumerable<IControlData>)controls);
                        }
                    }
                }
                return null;
            });

            _methodHandler.AddHandler("onWorldUpdate", method =>
            {
                var newWorld = (Dictionary<string, object>)method.Parameters;
                if (newWorld.ContainsKey("scenes"))
                    newWorld.Remove("scenes");


                return null;
            });

            //// Group Events
            //this.methodHandler.addHandler('onGroupCreate', res => {
            //    res.params.groups.forEach(group => this.onGroupCreate(group));
            //});
            //this.methodHandler.addHandler('onGroupDelete', res => {
            //    this.onGroupDelete(res.params.groupID, res.params.reassignGroupID);
            //});
            //this.methodHandler.addHandler('onGroupUpdate', res => {
            //    res.params.groups.forEach(group => this.onGroupUpdate(group));
            //});

            _clockSyncer.DeltaObs.Subscribe(delta =>
            {
                _clockDelta = delta;
            });

            if (_clientType == ClientType.GameClient)
            {
                AddGameClientHandlers();
            }
            else
            {
                AddParticipantHandlers();
            }

        }

        private void AddParticipantHandlers()
        {
            _methodHandler.AddHandler("onParticipantUpdate", method =>
            {
                var p = (JsonElement)method.Parameters;
                var arr = p.GetProperty("participants").GetRawText();
                var list = JsonSerializer.Deserialize<List<Participant>>(arr);
                _selfUpdate.OnNext(list[0]);
                return null;
            });

            _methodHandler.AddHandler("onParticipantJoin", method =>
            {
                var p = (JsonElement)method.Parameters;
                var arr = p.GetProperty("participants").GetRawText();
                var list = JsonSerializer.Deserialize<List<Participant>>(arr);
                _selfUpdate.OnNext(list[0]);
                return null;
            });
        }

        private void AddGameClientHandlers()
        {            
            _methodHandler.AddHandler("onParticipantJoin", method =>
            {
                var p = (JsonElement)method.Parameters;
                var arr = p.GetProperty("participants").GetRawText();
                var list = JsonSerializer.Deserialize<List<Participant>>(arr);
                foreach (var participant in list)
                {
                    _participants.Add(participant.SessionID, participant);
                    _participantJoin.OnNext(participant);
                }
                return null;
            });

            _methodHandler.AddHandler("onParticipantLeave", method =>
            {
                var p = (JsonElement)method.Parameters;
                var arr = p.GetProperty("participants").GetRawText();
                var list = JsonSerializer.Deserialize<List<Participant>>(arr);
                foreach (var participant in list)
                {
                    _participants.Remove(participant.SessionID);
                    _participantLeave.OnNext(participant);
                }
                return null;
            });

            _methodHandler.AddHandler("onParticipantUpdate", method =>
            {
                var p = (JsonElement)method.Parameters;
                var arr = p.GetProperty("participants").GetRawText();
                var list = JsonSerializer.Deserialize<List<Participant>>(arr);
                foreach (var participant in list)
                {
                    _participants[participant.SessionID] = participant;  //not sure if entire participant is sent or if just updated fields. This may not work.
                }
                return null;
            });

        }

        public void Reset()
        {
            foreach (var scene in _scenes.Values)
            {
                scene.Destroy();
            }
            _scenes.Clear();
            _clockDelta = 0;
            IsReady = false;
            _participants.Clear();
            //groups.Clear();
        }

        public void SetClient(Client client)
        {
            _client = client;
            _client.OpenObs.Subscribe(_ => _clockSyncer.Start());
            _client.CloseObs.Subscribe(close => _clockSyncer.Stop());
            _stateFactory.SetClient(client);
        }

        public Reply ProcessMethod(Method method)
        {
            try
            {
                return _methodHandler.Handle(method);
            }
            catch (Exception e)
            {
                if (e is InteractiveError.Base)
                {
                    return Reply.FromError(method.Id, (InteractiveError.Base)e);
                }
                throw e;
            }
        }

        public void OnWorldUpdate(Dictionary<string, object> data)
        {
            foreach (var i in data)
            {
                if (_world.ContainsKey(i.Key))
                {
                    _world[i.Key] = i.Value;
                }
                else
                {
                    _world.Add(i.Key, i.Value);
                }
            }
            _worldUpdated.OnNext(_world);
        }

        public Dictionary<string, object> World => _world;

        public void OnSceneDelete(string sceneID, object reassignSceneID)
        {
            if (_scenes.TryGetValue(sceneID, out var scene))
            {
                scene.Destroy();
                _scenes.Remove(sceneID);
                _sceneDeleted.OnNext(sceneID);
            }
        }

        public void OnSceneUpdate(SceneData sceneData)
        {
            if (_scenes.TryGetValue(sceneData.SceneID, out var scene))
            {
                scene.Update(sceneData);
            }
            
        }

        public Scene OnSceneCreate(SceneData sceneData)
        {
            if (_scenes.TryGetValue(sceneData.SceneID, out var scene))
            {
                OnSceneUpdate(sceneData);
                return scene;
            }

            scene = _stateFactory.CreateScene(sceneData);
            if (sceneData.Controls != null)
            {
                scene.OnControlsCreated(sceneData.Controls);
            }

            _scenes.Add(sceneData.SceneID, scene);
            _sceneCreated.OnNext(scene);
            return scene;
        }
    }
}
