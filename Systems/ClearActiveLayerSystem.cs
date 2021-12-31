using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Di;
using System.Threading.Tasks;
using TGELayerDraw;

namespace Cornerstone.Systems
{
    internal class ClearActiveLayerSystem : IEcsRunSystem
    {
        [EcsInject]
        MyGame game = null!;
        public void Run(EcsSystems systems)
        {
            game.ActiveLayer.Clear(new Color4(0, 0, 0, 0));
        }
    }
}
