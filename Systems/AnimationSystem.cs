using Cornerstone.Components;
using OpenTK.Mathematics;
namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    [EcsRead("Default", typeof(Transform))]
    [EcsWrite("Default", typeof(SpriteAnimation))]
    internal class AnimationSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        readonly EcsPool<Transform> Transforms;
        readonly EcsPool<SpriteAnimation> Animations;
        readonly EcsFilter AnimationFilter;

        public AnimationSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
            Transforms = GetPool<Transform>();
            Animations = GetPool<SpriteAnimation>();
            AnimationFilter = FilterInc<SpriteAnimation>().Inc<Transform>().End();
        }

        public void Run(float elapsed, int threadId)
        {
            float dt = elapsed;
            var layer = game.ActiveLayer;
            foreach (var entity in AnimationFilter)
            {
                ref var animation = ref Animations.Get(entity);
                ref var transform = ref Transforms.Get(entity);

                animation.AnimationAccumulator += dt;
                if (animation.AnimationAccumulator >= animation.AnimationFrameRate)
                {
                    animation.AnimationAccumulator = 0;
                    animation.AnimationFrame = (animation.AnimationFrame + 1) % animation.AnimationFrameCount;
                }

                layer.DrawPartialSprite((Vector2i)transform.Position, animation.Sprite, animation.AnimationFrame * animation.FrameWidth, 0, animation.FrameWidth, 5, true, BlendMode.Clip);
            }
        }
    }
}
