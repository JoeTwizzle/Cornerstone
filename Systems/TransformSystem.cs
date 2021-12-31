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
    internal class TransformSystem : IEcsRunSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld]
        EcsWorld world = null!;

        [EcsPool]
        EcsPool<Transform> Transforms = null!;

        [EcsFilter(typeof(Transform))]
        EcsFilter TransformFilter = null!;

        [EcsPool("Events")]
        EcsPool<StartEvent> StartEvents = null!;

        [EcsFilter("Events", typeof(StartEvent))]
        EcsFilter StartEventFilter = null!;
        bool active;
        public void Run(EcsSystems systems)
        {
            foreach (var entity in StartEventFilter)
            {
                ref var startEvent = ref StartEvents.Get(entity);
                active = startEvent.State;
            }
            if (active)
            {
                float dt = game.DeltaTime;
                foreach (var entity in TransformFilter)
                {
                    ref var t = ref Transforms.Get(entity);
                    t.Position += t.Velocity * dt;
                }
            }
        }
    }
}
