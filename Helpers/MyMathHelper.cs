using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Helpers
{
    public static class MyMathHelper
    {
        public static float LerpUnclamped(float start, float end, float t)
        {
            return start * (1 - t) + end * t;
        }
        public static float Lerp3(float a, float b, float c, float t)
        {
            if (t <= 0.5f)
            {
                return LerpUnclamped(a, b, t * 2f);
            }
            else
            {
                return LerpUnclamped(b * 2f, c, t);
            }
        }
        //Thanks celeste devs
        public static float Approach(float value, float target, float maxDelta)
        {
            return value > target ? MathF.Max(value - maxDelta, target) : MathF.Min(value + maxDelta, target);
        }

        public static float RemapRange(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
