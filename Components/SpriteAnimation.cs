using Leopotam.EcsLite;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGELayerDraw;

namespace Cornerstone.Components
{
    struct SpriteAnimation : IEcsAutoReset<SpriteAnimation>
    {
        public Sprite Sprite;
        public float AnimationAccumulator;
        public float AnimationFrameRate;
        public int AnimationFrame;
        public int AnimationFrameCount;
        public int FrameWidth;

        public void AutoReset(ref SpriteAnimation c)
        {
            c.FrameWidth = 5;
            c.AnimationAccumulator = 0;
            c.AnimationFrameRate = 1 / 60f;
            c.AnimationFrame = 0;
            c.AnimationFrameCount = 2;
        }
    }
}
