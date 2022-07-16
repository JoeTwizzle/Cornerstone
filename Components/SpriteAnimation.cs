namespace Cornerstone.Components
{
    struct SpriteAnimation : IEcsInit<SpriteAnimation>
    {
        public Sprite Sprite;
        public float AnimationAccumulator;
        public float AnimationFrameRate;
        public int AnimationFrame;
        public int AnimationFrameCount;
        public int FrameWidth;

        public static void OnInit(ref SpriteAnimation c)
        {
            c.FrameWidth = 5;
            c.AnimationAccumulator = 0;
            c.AnimationFrameRate = 1 / 60f;
            c.AnimationFrame = 0;
            c.AnimationFrameCount = 2;
        }
    }
}
