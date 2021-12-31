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
using OpenTK.Graphics.OpenGL4;

namespace Cornerstone.Systems
{
    internal class GameSystem : IEcsRunSystem, IEcsInitSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld]
        EcsWorld world = null!;

        [EcsWorld("Events")]
        EcsWorld events = null!;

        [EcsPool("Events")]
        EcsPool<StartEvent> StartEvents = null!;

        [EcsFilter("Events", typeof(StartEvent))]
        EcsFilter StartEventFilter = null!;

        HardwareSprite hardwareSprite = null!;
        public void Init(EcsSystems systems)
        {
            hardwareSprite = new HardwareSprite("map-1.png");
        }
        bool active = false;
        public void Run(EcsSystems systems)
        {
            foreach (var entity in StartEventFilter)
            {
                active = StartEvents.Get(entity).State;
            }
            if (!active)
            {
                return;
            }
            var layer = game.ActiveLayer;
            layer.DrawPartialSprite(0, 0, hardwareSprite, 0, 0, 128, 82, false, BlendMode.None);
        }
    }
}
