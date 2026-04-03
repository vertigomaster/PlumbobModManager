using System;

namespace IDEK.Tools.TimeHelpers
{
    public class ContextualUnityTime : ContextualTime<ContextualUnityTime.Mode, float> 
    {
        [Flags]
        public enum Mode 
        {
            //default is scaled delta (frame) time.

            FrameTime = 0 << 1, //default, 0000
            FixedTime = 1 << 1, //0001

            Unscaled = 1 << 2, //0010
            Scaled = 0 << 2,

            Absolute = 1 << 3,
            Delta = 0 << 3,
        }

        public bool IsDeltaTime => mode.HasFlag(Mode.FrameTime) && mode.HasFlag(Mode.Delta);
        public bool IsFixedDeltaTime => mode.HasFlag(Mode.FixedTime) && mode.HasFlag(Mode.Delta);

        public static ContextualUnityTime AsDeltaTime(float deltaTime) => new() {
            timeValue = deltaTime, mode = Mode.Delta | Mode.FrameTime
        };

        public static ContextualUnityTime AsFixedDeltaTime(float fixedDeltaTime) => new() {
            timeValue = fixedDeltaTime, mode = Mode.Delta | Mode.FixedTime
        };
    }
}