using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone
{
    public struct Animation
    {
        public float StartDelay { get; set; }
        public float Duration { get; set; }
        public float Value { get; private set; }
        public float Progress { get; private set; }
        public bool Loop { get; set; }
        public bool JustFinished { get; set; }
        public float TotalTime { get; set; }
        public bool IsWaiting => TotalTime < StartDelay;
        public bool IsPlaying => TotalTime >= StartDelay && TotalTime < StartDelay + Duration;
        public bool IsFinished => TotalTime >= StartDelay + Duration;
        public Easing.Function EasingFunction;

        public Animation(float startDelay, float duration, Easing.Function easingFunction, bool loop = false)
        {
            JustFinished = false;
            TotalTime = 0;
            Progress = 0;
            Value = 0;
            StartDelay = startDelay;
            Duration = duration;
            EasingFunction = easingFunction;
            Loop = loop;
        }

        public void Play(float dt)
        {
            if (IsWaiting)
            {
                Value = 0f;
            }
            if (IsPlaying)
            {
                Progress = MathF.Min(Progress + dt / Duration, 1f);
                Value = Easing.Interpolate(Progress, EasingFunction);
            }
            if (IsFinished)
            {
                Value = 1f;
            }
            if (TotalTime < StartDelay + Duration && TotalTime + dt >= StartDelay + Duration)
            {
                JustFinished = true;
            }
            else
            {
                JustFinished = false;
            }
            TotalTime += dt;

            if (Loop && IsFinished)
            {
                TotalTime = StartDelay;
            }
        }

        public void Reset()
        {
            TotalTime = 0;
            Progress = 0;
        }
    }
}
