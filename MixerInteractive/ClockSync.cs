using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixerInteractive
{
    public class ClockSync
    {
        private ClockSyncOptions _clockSyncOptions;
        private List<long> _deltas = new List<long>();
        private long _cachedDelta = int.MinValue;
        private long _expectedTime;
        private System.Timers.Timer _timer;
        private Task _syncing;
        private ISubject<long> _delta = new Subject<long>();

        public IObservable<long> DeltaObs => _delta.AsObservable();

        public ClockSyncerState State { get; set; } = ClockSyncerState.Stopped;

        public ClockSync(ClockSyncOptions clockSyncOptions)
        {
            _clockSyncOptions = clockSyncOptions;
        }

        public async void Start()
        {
            State = ClockSyncerState.Started;
            _deltas.Clear();

            await Sync();
            _expectedTime = DateTime.Now.Ticks + _clockSyncOptions.CheckInterval;
            _timer = new System.Timers.Timer(_clockSyncOptions.CheckInterval);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void Stop()
        {
            State = ClockSyncerState.Stopped;

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= Timer_Elapsed;
                _timer = null;
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var now = DateTime.Now.Ticks;
            var diff = Math.Abs(now - _expectedTime);
            if (diff > _clockSyncOptions.Threshold && _syncing == null)
            {
                _ = Sync();
            }
            _expectedTime = DateTime.Now.Ticks + _clockSyncOptions.CheckInterval;
        }

        private async Task Sync()
        {
            State = ClockSyncerState.Synchronizing;
            var sampleTasks = new List<Task<long>>();

            for (var i=0; i<_clockSyncOptions.SampleSize; i++)
            {
                sampleTasks.Add(SendWithDelay<long>(i * _clockSyncOptions.SampleDelay, Sample));
            }
            _syncing = Task.WhenAll(sampleTasks).ContinueWith(samples =>
            {
                if (State != ClockSyncerState.Synchronizing)
                    return;
                State = ClockSyncerState.Idle;
                _delta.OnNext(GetDelta());
            });

            await _syncing;
            _syncing = null;
        }

        Task<T> SendWithDelay<T>(int delay, Func<Task<T>> func)
        {
            var task =  Task.Delay(delay).ContinueWith(x=> func.Invoke());
            return task.Unwrap();
        }

        private async Task<long> Sample()
        {
            if (State == ClockSyncerState.Stopped)
                return int.MinValue;

            var transmitTime = DateTime.Now;
            try
            {
                var result = await _clockSyncOptions.SampleFunc();

            }  
            catch (Exception err)
            {
                if (State != ClockSyncerState.Stopped)
                    throw err;
            }
            return long.MinValue;
        }

        public long GetDelta(bool forceCalculation=false)
        {
            if (_cachedDelta == long.MinValue || forceCalculation) {
                _cachedDelta = CalculateDelta();
            }
            return _cachedDelta;
        }

        private long CalculateDelta()
        {
            if (_deltas.Count == 0)
                return 0;

            if (_deltas.Count == 1)
                return _deltas[0];

            var sorted = _deltas.OrderBy(x => x);
            var midPoint = (int)Math.Floor((double)sorted.Count() / 2);

            if (sorted.Count() % 2 != 0)
                return sorted.ElementAt(midPoint);
            else
                return (sorted.ElementAt(midPoint+1) + sorted.ElementAt(midPoint))/ 2;
        }

        private long ProcessResponse(long transmitTime, long serverTime)
        {
            var receiveTime = DateTime.Now.Ticks;
            var rtt = receiveTime - transmitTime;
            var delta = (serverTime - rtt / 2 - transmitTime);
            return AddDelta(delta);
        }

        private long AddDelta(long delta)
        {
            // Add new one
            _deltas.Add(delta);

            // Re-calculate delta with this number
            return GetDelta(true);
    }
}
}
