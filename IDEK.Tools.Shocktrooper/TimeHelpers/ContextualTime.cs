namespace IDEK.Tools.TimeHelpers
{
    public class ContextualTime<TMode, TTimeValue> where TTimeValue : unmanaged
    {
        public TMode mode;
        public TTimeValue timeValue;
    }
}