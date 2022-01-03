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
    internal class DrawBGSystem : IEcsRunSystem, IEcsInitSystem
    {
        [EcsInject]
        MyGame game = null!;
        HardwareSprite hardwareSprite = null!;
        public void Init(EcsSystems systems)
        {
            hardwareSprite = new HardwareSprite("map-1.png");
        }
        public void Run(EcsSystems systems)
        {
            var layer = game.ActiveLayer;
            layer.DrawPartialSprite(0, 0, hardwareSprite, 0, 0, 128, 82, false, BlendMode.None);
        }
    }
}
