using System;

namespace OBSGreenScreen
{
    public class PerfFinishedEventArgs : EventArgs
    {
        public PerfFinishedEventArgs(double averagePerformance)
        {
            AveragePerformance = averagePerformance;
        }

        public double AveragePerformance { get; }
    }
}
