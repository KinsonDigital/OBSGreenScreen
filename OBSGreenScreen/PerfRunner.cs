using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OBSGreenScreen
{
    public class PerfRunner
    {
        public event EventHandler<PerfFinishedEventArgs> PerfRunFinished;
        private List<double> _times = new();
        private Stopwatch _timer = new ();

        public int TotalSamples { get; set; } = 1000;

        public bool IsRunning => _timer.IsRunning;

        public int CurrentSample => _times.Count;

        public double AveragePerf => !_times.Any() ? 0 : _times.Average();

        public bool Enabled { get; set; } = false;

        public void Start()
        {
            if (!Enabled)
            {
                return;
            }

            if (_timer.IsRunning is false)
            {
                _timer.Start();
            }
        }

        public void Stop()
        {
            if (!Enabled)
            {
                return;
            }
            _timer.Stop();
        }

        public void Reset()
        {
            if (!Enabled)
            {
                return;
            }
            _timer.Reset();
        }

        public void Record()
        {
            if (!Enabled)
            {
                return;
            }

            _times.Add(_timer.Elapsed.TotalMilliseconds);

            if (_times.Count >= TotalSamples)
            {
                var averageResult = _times.Average();
                PerfRunFinished?.Invoke(this, new PerfFinishedEventArgs(averageResult));

                _times.Clear();
                _timer.Reset();
            }
        }
    }
}
