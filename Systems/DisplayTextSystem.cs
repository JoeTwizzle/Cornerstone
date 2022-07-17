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

namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    internal class DisplayTextSystem : EcsSystem, IEcsRunSystem
    {
        MyGame game;

        public DisplayTextSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
        }

        public void Run(float elapsed, int threadId)
        {
            var layer = game.ActiveLayer;
            
        }
    }
}
