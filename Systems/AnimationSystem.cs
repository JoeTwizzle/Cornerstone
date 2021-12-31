using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Di;
using System.Threading.Tasks;
using TGELayerDraw;
using Cornerstone.Helpers;
using Cornerstone.Events;
using Cornerstone.UI;
using Cornerstone.Components;
namespace Cornerstone.Systems
{
    internal class AnimationSystem : IEcsRunSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld]
        EcsWorld world = null!;

        [EcsPool]
        EcsPool<Transform> Transforms = null!;

        [EcsPool]
        EcsPool<SpriteAnimation> Animations = null!;

        [EcsFilter(typeof(SpriteAnimation), typeof(Transform))]
        EcsFilter AnimationFilter = null!;

        public void Run(EcsSystems systems)
        {
            float dt = game.DeltaTime;
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
