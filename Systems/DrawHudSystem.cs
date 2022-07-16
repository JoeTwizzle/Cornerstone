using Cornerstone.Components;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    [EcsRead("Default", typeof(Player))]
    internal class DrawHudSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;

        readonly EcsPool<Player> Players;

        readonly EcsFilter PlayerFilter;

        public DrawHudSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
            Players = GetPool<Player>();
            PlayerFilter = FilterInc<Player>().End();
        }

        public void Run(EcsSystems systems, float elapsed, int threadId)
        {
            var layer = game.ActiveLayer;
            foreach (var entity in PlayerFilter)
            {
                ref var player = ref Players.Get(entity);
                DrawHearts(layer, 10, player.HP, player.Armor, 1, 1);
            }
            DrawNumber(layer, (int)game.Score, 124, 1, Color4.Green);
        }
        public static void DrawHearts(DrawableBuffer<Color4> layer, int HeartCount, int fullHearts, int armor, int x, int y)
        {
            Color4 hpColor;
            Color4 fullHPColor = new Color4(255, 0, 77, 255);
            Color4 emptyHPColor = new Color4(55, 0, 33, 255);
            Color4 armorFullHPColor = new Color4(166, 166, 166, 255);
            Color4 armorEmptyHPColor = new Color4(99, 99, 99, 255);
            void DrawHeart()
            {
                layer.DrawPixel(x, y, hpColor);
                layer.DrawPixel(x + 2, y, hpColor);
                layer.DrawPixel(x, y + 1, hpColor);
                layer.DrawPixel(x + 2, y + 1, hpColor);
                layer.DrawPixel(x + 1, y + 1, hpColor);
                layer.DrawPixel(x + 1, y + 2, hpColor);
            }
            for (int i = 0; i < HeartCount; i++)
            {
                if (i < armor)//we got armor
                {
                    if (i < fullHearts)
                    {
                        hpColor = armorFullHPColor;
                    }
                    else
                    {
                        hpColor = armorEmptyHPColor;
                    }
                }
                else
                {
                    if (i < fullHearts)
                    {
                        hpColor = fullHPColor;
                    }
                    else
                    {
                        hpColor = emptyHPColor;
                    }
                }

                DrawHeart();
                x += 5;
            }
        }


        public static void DrawNumber(DrawableBuffer<Color4> layer, int number, int x, int y, Color4 color, LayoutDirection layoutDirection = LayoutDirection.LTR, BlendMode blendMode = BlendMode.Clip)
        {
            int digits = 0;
            int divisor = 1;
            while (MathF.Ceiling(number / divisor) > 0)
            {
                divisor *= 10;
                digits++;
            }
            if (layoutDirection == LayoutDirection.LTR)
            {
                for (int i = 0; i < digits; i++)
                {
                    int div = 1;
                    for (int j = 0; j < i; j++)
                    {
                        div *= 10;
                    }
                    DrawDigit(layer, (number / Math.Max(div, 1)) % 10, x, y, color, blendMode);
                    x -= 4;
                }
            }
            else
            {
                x += 4 * (digits - 1);
                for (int i = 0; i < digits; i++)
                {
                    int div = 1;
                    for (int j = 0; j < i; j++)
                    {
                        div *= 10;
                    }
                    DrawDigit(layer, (number / Math.Max(div, 1)) % 10, x, y, color, blendMode);
                    x -= 4;
                }
            }
        }

        public static void DrawDigit(DrawableBuffer<Color4> layer, int digit, int x, int y, Color4 color, BlendMode blendMode = BlendMode.Clip)
        {
            switch (digit)
            {
                case 0:
                    //###
                    //# #
                    //# #
                    //# #
                    //###
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x, y + 1, color, blendMode);
                    layer.DrawPixel(x + 2, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x, y + 3, color, blendMode);
                    layer.DrawPixel(x + 2, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                case 1:
                    // #
                    //##
                    // #
                    // #
                    //###
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x, y + 1, color, blendMode);
                    layer.DrawPixel(x + 1, y + 1, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                case 2:
                    //###
                    //  #
                    //###
                    //#
                    //###
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x + 2, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                case 3:
                    //##
                    //  #
                    //##
                    //  #
                    //##
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    break;
                case 4:
                    // ##
                    //###
                    //# # 
                    //###   
                    //  #
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x, y + 1, color, blendMode);
                    layer.DrawPixel(x + 1, y + 1, color, blendMode);
                    layer.DrawPixel(x + 2, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x, y + 3, color, blendMode);
                    layer.DrawPixel(x + 1, y + 3, color, blendMode);
                    layer.DrawPixel(x + 2, y + 3, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                case 5:
                    //###
                    //#
                    //### 
                    //  #   
                    //###
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                case 6:
                    //###
                    //#
                    //### 
                    //# #   
                    //###
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x, y + 3, color, blendMode);
                    layer.DrawPixel(x + 2, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                case 7:
                    //###
                    //  #
                    //### 
                    // #   
                    //# 
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x + 2, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    break;
                case 8:
                    //###
                    //# #
                    //### 
                    //# #   
                    //###
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x, y + 1, color, blendMode);
                    layer.DrawPixel(x + 2, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x, y + 3, color, blendMode);
                    layer.DrawPixel(x + 2, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                case 9:
                    //###
                    //# #
                    //### 
                    //  #   
                    //###
                    layer.DrawPixel(x, y, color, blendMode);
                    layer.DrawPixel(x + 1, y, color, blendMode);
                    layer.DrawPixel(x + 2, y, color, blendMode);
                    layer.DrawPixel(x, y + 1, color, blendMode);
                    layer.DrawPixel(x + 2, y + 1, color, blendMode);
                    layer.DrawPixel(x, y + 2, color, blendMode);
                    layer.DrawPixel(x + 1, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 2, color, blendMode);
                    layer.DrawPixel(x + 2, y + 3, color, blendMode);
                    layer.DrawPixel(x, y + 4, color, blendMode);
                    layer.DrawPixel(x + 1, y + 4, color, blendMode);
                    layer.DrawPixel(x + 2, y + 4, color, blendMode);
                    break;
                default:
                    break;
            }
        }
    }
}
