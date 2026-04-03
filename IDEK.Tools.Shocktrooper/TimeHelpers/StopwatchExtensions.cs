using System.Diagnostics;

namespace IDEK.Tools.TimeHelpers
{
    public static class StopwatchExtensions
    {
        public static float CalcElapsedSeconds(this Stopwatch stopwatch)
        {
            return stopwatch.ElapsedMilliseconds / 1000f;
        }
    }
}