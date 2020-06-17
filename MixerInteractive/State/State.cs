using MixerInteractive.Methods;
using MixerInteractive.Wire;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private Subject<Group> _groupCreated = new Subject<Group>();
        private Subject<Tuple<string,string>> _groupDeleted = new Subject<Tuple<string, string>>();

        public IObservable<bool> OnReady => _ready.AsObservable();
        public IObservable<Scene> OnSceneCreated => _sceneCreated.AsObservable();
        public IObservable<string> OnSceneDeleted => _sceneDeleted.AsObservable();
        public IObservable<object> OnWorldUpdated => _worldUpdated.AsObservable();
        public IObservable<Participant> OnSelfUpdate => _selfUpdate.AsObservable();
        public IObservable<Participant> OnParticipantJoin => _participantJoin.AsObservable();
        public IObservable<Participant> OnParticipantLeave => _participantLeave.AsObservable();


        private Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        private Dictionary<string, Participant> _participants = new Dictionary<string, Participant>();
        private Dictionary<string, Group> _groups = new Dictionary<string, Group>();
        private Dictionary<string, object> _world = new Dictionary<string, object>();
       

        private ClockSync _clockSyncer;
        private long _clockDelta = 0;
        private ClientType _clientType;

        public Dictionary<string, Participant> Participants => _participants;
        public Dictionary<string, Group> Groups => _groups;

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
                            scene.OnControlsCreate((JsonElement)controls);
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
                            scene.OnControlsDelete((IEnumerable<ControlData>)controls);
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
                            scene.OnControlsUpdate((IEnumerable<ControlData>)controls);
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

            // Group Events
            _methodHandler.AddHandler("onGroupCreate", method => {
                var p = (JsonElement)method.Parameters;
                var arr = p.GetProperty("groups").GetRawText();
                var list = JsonSerializer.Deserialize<List<Group>>(arr);
                foreach (var group in list)
                {
                    OnGroupCreate(group);
                }
                return null;
            });

            _methodHandler.AddHandler("onGroupDelete", method => 
            {
                var p = (JsonElement)method.Parameters;
                OnGroupDelete(p.GetProperty("groupID").GetString(), p.GetProperty("reassignGroupID").GetString());
                return null;
            });
            
            _methodHandler.AddHandler("onGroupUpdate", method => 
            {
                var p = (JsonElement)method.Parameters;
                var arr = p.GetProperty("groups").GetRawText();
                var list = JsonSerializer.Deserialize<List<Group>>(arr);
                foreach (var group in list)
                {
                    OnGroupUpdate(group);
                }
                return null;
            });

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

        public IEnumerable<Scene> SynchronizeScenes(IEnumerable<SceneData> data)
        {
            var scenes = data.Select(x=> OnSceneCreate(x));

            return scenes;
        }

       
        public Group OnGroupCreate(IGroupData data)
        {
            if (_groups.TryGetValue(data.GroupID, out var group))
            {
                OnGroupUpdate(data);
                return group;
            }
                
            group = new Group(data);
            _groups.Add(data.GroupID, group);
            _groupCreated.OnNext(group);
            return group;
        }

        public void OnGroupUpdate(IGroupData data)
        {
            if (_groups.TryGetValue(data.GroupID, out var group))
            {
                group.Update(data);
            }
        }

        public void OnGroupDelete(string groupID, string reassignGroupID)
        {
            if (_groups.TryGetValue(groupID, out var targetGroup))
            {
                targetGroup.Destroy();
                _groups.Remove(groupID);
                _groupDeleted.OnNext(new Tuple<string, string>(groupID, reassignGroupID));
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


            _methodHandler.AddHandler("giveInput", method => 
            {
                var p = (JsonElement)method.Parameters;
                var controlID = p.GetProperty("input").GetProperty("controlID").GetString();
                
                var control = GetControl(controlID);
                if (control != null)
                {
                    var participantID = p.GetProperty("participantID").GetString();
                    if (_participants.TryGetValue(participantID, out var participant))
                    {
                        control.ReceiveInput(p, participant);
                    }
                }

                return null;
            });
        }

        public Control GetControl(string id)
        {
            Control result = null;
            foreach (var scene in _scenes.Values)
            {
                if (scene.Controls.TryGetValue(id, out var found))
                {
                    result = found;
                    break;
                }
            }
            return result;
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
                scene.OnControlsCreate(sceneData.Controls); 
            }

            _scenes.Add(sceneData.SceneID, scene);
            _sceneCreated.OnNext(scene);
            return scene;
        }
    }
}
