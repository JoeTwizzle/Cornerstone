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

namespace Cornerstone.Systems
{
    internal class MainMenuSystem : IEcsRunSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld("Events")]
        EcsWorld world = null!;
        [EcsPool("Events")]
        EcsPool<ResetGameEvent> ResetEvents = null!;
        Animation bounce = new Animation(0, 0.5f, Easing.Function.BounceEaseOut);
        public void Run(EcsSystems systems)
        {
            Color4 color = new Color4(29, 43, 83, 180);
            Color4 outlineColor = new Color4(0.1f, 0.4f, 0.8f, 0.4f);

            var layer = game.ActiveLayer;
            int layerHalfWidth = layer.Width / 2;
            int layerHalfHeight = layer.Height / 2;
            int width = 19;
            int height = 9;
            Color4 c = new Color4(41, 173, 255, 255);
            Panel panel = Panel.FromCenter(layerHalfWidth, layerHalfHeight, width, height, color, outlineColor);
            if (panel.IsInside(game.CursorPos))
            {
                c = panel.OutlineColor = new Color4(255, 0, 77, 255);
                if (game.MouseState.IsButtonDown(MouseButton.Left) && !game.MouseState.WasButtonDown(MouseButton.Left))
                {
                    game.DisableGroup("Menu");
                    game.DisableGroup("Intro");
                    game.EnableGroupNextFrame("GameCore");
                    game.EnableGroupNextFrame("Game");
                    game.EnableGroupNextFrame("Pauseable");
                    var ent = world.NewEntity();
                    ResetEvents.Add(ent);
                }
            }
            float bounceScale = MyMathHelper.Lerp3(1, 0.7f, 0, bounce.Value) * 90f;
            panel.PosY -= (int)bounceScale;
            panel.Draw(layer, BlendMode.Alpha);
            int x = layerHalfWidth - width / 2 + 2;
            int y = layerHalfHeight - height / 2 + 2 - (int)bounceScale;

            //P
            layer.DrawLine(x, y, x, y + 5, c);
            layer.DrawPixel(x + 1, y, c);
            layer.DrawPixel(x + 2, y, c);
            layer.DrawPixel(x + 2, y + 1, c);
            layer.DrawPixel(x + 2, y + 2, c);
            layer.DrawPixel(x + 1, y + 2, c);
            //L
            x += 4;
            layer.DrawLine(x, y, x, y + 5, c);
            layer.DrawPixel(x + 2, y + 4, c);
            layer.DrawPixel(x + 1, y + 4, c);
            //A
            x += 4;
            layer.DrawLine(x, y, x, y + 5, c);
            layer.DrawLine(x + 2, y, x + 2, y + 5, c);
            layer.DrawPixel(x + 1, y, c);
            layer.DrawPixel(x + 1, y + 2, c);
            //Y
            x += 4;
            layer.DrawPixel(x + 0, y + 0, c);
            layer.DrawPixel(x + 2, y + 0, c);
            layer.DrawPixel(x + 0, y + 1, c);
            layer.DrawPixel(x + 2, y + 1, c);
            layer.DrawPixel(x + 0, y + 2, c);
            layer.DrawPixel(x + 2, y + 2, c);
            layer.DrawPixel(x + 1, y + 2, c);
            layer.DrawPixel(x + 1, y + 3, c);
            layer.DrawPixel(x + 1, y + 4, c);
            bounce.Play(game.DeltaTime);
            game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.92f * game.GameArea.Y), "MUSIC BY KUBBI", Color4.LightSeaGreen, TextLayout.RightAlign);
            game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.95f * game.GameArea.Y), "ALBUM EMBER", Color4.LightSeaGreen, TextLayout.RightAlign);
            //game.textRenderer.TextScale = new Vector2(32);
            //game.textRenderer.DrawText(game.textRenderer.GetScreenPos(layer.Width / 2, layer.Height / 2), "Play", c, TextLayout.CenterAlign);
        }
    }
}
