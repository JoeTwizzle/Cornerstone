using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading.Tasks;
using TGELayerDraw;
using Cornerstone.Helpers;
using Cornerstone.Events;
using Cornerstone.UI;
using Cornerstone.Components;
using OpenTK.Graphics.OpenGL4;

namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    internal class DrawBGSystem : EcsSystem, IEcsRunSystem
    {
        MyGame game;
        HardwareSprite hardwareSprite;

        public DrawBGSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
            hardwareSprite = new HardwareSprite("map-1.png");
        }

        public void Run(float elapsed, int threadId)
        {
            var layer = game.ActiveLayer;
            layer.DrawPartialSprite(0, 0, hardwareSprite, 0, 0, 128, 82, false, BlendMode.None);
        }
    }
}
