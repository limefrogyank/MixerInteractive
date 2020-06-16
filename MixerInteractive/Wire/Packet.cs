using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixerInteractive.Wire
{
    public class Packet
    {
        private PacketState _state = PacketState.Pending;
        private int _timeout = int.MinValue;
        private Method _method;

        private ISubject<Unit> _cancel = new Subject<Unit>();
        private ISubject<object> _send = new Subject<object>();

        [JsonIgnore] public IObservable<Unit> CancelObs => _cancel.AsObservable();
        [JsonIgnore] public ISubject<object> SendSubject => _send;

        public Packet(Method method)
        {
            _method = method;
        }

        [JsonPropertyName("id")]
        public int Id => _method.Id;

        public void Cancel()
        {

        }

        public Packet SetSequenceNumber(int x)
        {
            _method.Seq = x;
            return this;
        }

        public void SetTimeout(int duration)
        {
            _timeout = duration;
        }

        public int GetTimeout(int defaultTimeout)
        {
            if (_timeout == int.MinValue)
                return defaultTimeout;
            return _timeout;
        }


        public PacketState GetState()
        {
            return _state;
        }

        public void SetState(PacketState state)
        {
            if (state == _state)
                return;
            _state = state;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(_method);
        }
    }
}
