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
    internal class DisplayTextSystem:IEcsRunSystem
    {
        [EcsInject]
        MyGame game = null!;

        public void Run(EcsSystems systems)
        {
            var layer = game.ActiveLayer;
            layer.UpdateVisual();
            game.DisplaySprite(layer);
            game.TextRenderer.Display();
        }
    }
}
