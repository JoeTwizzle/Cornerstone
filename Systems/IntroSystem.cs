using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading.Tasks;
using TGELayerDraw;
using Cornerstone.Helpers;
using Cornerstone.Events;
using Cornerstone.UI;

namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    internal class IntroSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;

        Animation sineWaveAnimation = new Animation(0, 5.4f, Easing.Function.ExponentialEaseIn);
        Animation moveUpAnimation = new Animation(1f, 1.2f, Easing.Function.BounceEaseOut);
        Animation fillScreenAnimation = new Animation(0f, 1.5f, Easing.Function.ExponentialEaseOut);
        //Animation snowAnimation = new Animation(0f, 10f, Easing.Function.Linear, true);
        float cycle = 0;

        public IntroSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
        }

        public void Run(float elapsed, int threadId)
        {
            float dt = game.DeltaTime;
            if (game.KeyboardState.IsAnyKeyDown && !fillScreenAnimation.IsFinished)
            {
                dt += 10000;
            }
            var layer = game.ActiveLayer;
            float startingHeight = layer.Height / 2;
            sineWaveAnimation.Play(dt);
            if (sineWaveAnimation.IsPlaying)
            {
                int graphY = 0;
                int lastY;
                for (int x = 0; x < layer.Width; x++)
                {
                    var color = Color4.FromHsv(new Vector4(((((float)x) / layer.Width) + cycle) % 1f, 1, 1, 1));
                    int GetHeight(float n)
                    {
                        float amplitude = 100f;
                        amplitude *= 1f - sineWaveAnimation.Value;
                        return (int)(MathF.Sin(n / (MathF.PI * 2f)) * amplitude);
                    }
                    lastY = graphY;
                    graphY = GetHeight(x + game.Time * 100);
                    if (x == 0)
                    {
                        lastY = graphY;
                    }
                    layer.DrawLine(x - 1, layer.Height / 2 + lastY, x, layer.Height / 2 + graphY, color);
                    startingHeight = layer.Height / 2f;
                }
            }
            if (sineWaveAnimation.IsFinished)
            {
                if (moveUpAnimation.IsWaiting)
                {
                    int graphY = 0;
                    int lastY;
                    for (int x = 0; x < layer.Width; x++)
                    {
                        lastY = graphY;
                        graphY = (int)(MathF.Sin(x * 0.25f + game.Time * 50) * 10 * (1 - MathF.Abs(MyMathHelper.RemapRange(moveUpAnimation.StartDelay - moveUpAnimation.TotalTime, 1, 0, -1, 1))));
                        if (x == 0)
                        {
                            lastY = graphY;
                        }
                        var color = Color4.FromHsv(new Vector4(((((float)x) / layer.Width) + cycle) % 1f, 1, 1, 1));
                        layer.DrawLine(x - 1, layer.Height / 2 + lastY, x, layer.Height / 2 + graphY, color);
                    }
                }
                moveUpAnimation.Play(dt);
                if (moveUpAnimation.IsPlaying)
                {
                    for (int x = 0; x < layer.Width; x++)
                    {
                        var color = Color4.FromHsv(new Vector4(((((float)x) / layer.Width) + cycle) % 1f, 1, 1, 1));
                        var height = MyMathHelper.LerpUnclamped(startingHeight, 0, moveUpAnimation.Value);
                        layer.DrawPixelUnchecked(x, (int)height, color);
                    }
                }
            }
            if (moveUpAnimation.IsFinished && (fillScreenAnimation.IsPlaying || fillScreenAnimation.IsFinished))
            {
                fillScreenAnimation.Play(dt);
                for (int x = 0; x < layer.Width; x++)
                {
                    int height = (int)MyMathHelper.LerpUnclamped(0, layer.Height, fillScreenAnimation.Value);
                    for (int y = 0; y < height; y++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(layer.Width / 2, layer.Width / 2));
                        float brightness = MyMathHelper.LerpUnclamped(1, 0.5f, fillScreenAnimation.Value);
                        float v = MyMathHelper.LerpUnclamped(1, 0.35f, fillScreenAnimation.Value);
                        var hsva = new Vector4(((dist / (layer.Width / 2) + cycle) % 1f, brightness, v, 1));
                        var color = Color4.FromHsv(hsva);
                        layer.DrawPixel(x, y, color);
                    }
                }
            }
            if (fillScreenAnimation.JustFinished)
            {
                game.EnableGroupNextFrame("Menu");
            }
            if (fillScreenAnimation.IsFinished)
            {
                cycle += dt / 3f;
                cycle %= 1f;
            }
        }
    }
}
