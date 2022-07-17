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
    internal class ReflectionSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;

        public ReflectionSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
        }

        public void Run(float elapsed, int threadId)
        {
            var layer = game.ActiveLayer;
            for (int y = 81; y < 128; y++)
            {
                float yPercentage = (y - 81f) / 47f;
                float strength = yPercentage * yPercentage * yPercentage * yPercentage * yPercentage * 4;
                float c = MathF.Sin(y + yPercentage + game.Time * MathF.PI * 2 * 1.5f) + 0.5f;
                for (int x = 0; x < 128; x++)
                {
                    Color4 srcColor = layer.GetPixel((int)(x + c * strength), (int)(81 - (y - 81) * 1.7f), new Color4(15, 15, 15, 255));
                    srcColor.R *= 0.3f;
                    srcColor.G *= 0.4f;
                    srcColor.B *= 0.6f;
                    layer.DrawPixel(x, y, srcColor, BlendMode.None);
                }
            }
        }
    }
}
