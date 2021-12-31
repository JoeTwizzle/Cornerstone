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
    internal class LifetimeSystem : IEcsRunSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld]
        EcsWorld world = null!;

        [EcsPool]
        EcsPool<Lifetime> Lifetimes = null!;

        [EcsFilter(typeof(Lifetime))]
        EcsFilter LifetimeFilter = null!;

        public void Run(EcsSystems systems)
        {
            float dt = game.DeltaTime;
            foreach (var entity in LifetimeFilter)
            {
                ref var lifetime = ref Lifetimes.Get(entity);
                if (lifetime.Time <= 0)
                {
                    world.DelEntity(entity);
                }
                lifetime.Time -= dt;
            }
        }
    }
}
