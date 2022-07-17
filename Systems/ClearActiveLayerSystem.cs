using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading.Tasks;
using TGELayerDraw;

namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    internal class ClearActiveLayerSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;

        public ClearActiveLayerSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
        }

        public void Run(float elapsed, int threadId)
        {
            game.ActiveLayer.Clear(new Color4(0, 0, 0, 0));
        }
    }
}
