using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Di;
using System.Threading.Tasks;
using TGELayerDraw;
using Cornerstone.UI;
using Cornerstone.Components;
using Cornerstone.Events;
using ALAudio;

namespace Cornerstone.Systems
{
    enum LayoutDirection
    {
        RTL,
        LTR
    }
    internal class CursorSystem : IEcsRunSystem, IEcsInitSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld("Events")]
        EcsWorld Events = null!;

        [EcsFilter("Events", typeof(StartEvent))]
        EcsFilter StartEventFilter = null!;

        [EcsPool]
        EcsPool<Player> Players = null!;

        [EcsPool("Events")]
        EcsPool<StartEvent> StartEvents = null!;

        [EcsFilter(typeof(Player))]
        EcsFilter PlayerFilter = null!;
        AudioSource bgmSource = null!;
        AudioBuffer bgmBuffer = null!;
        public static AudioSource PauseInSource = null!;
        AudioBuffer PauseInBuffer = null!;
        AudioSource PauseOutSource = null!;
        AudioBuffer PauseOutBuffer = null!;
        bool canPlayMusic = false;
        public void Init(EcsSystems systems)
        {
            bgmBuffer = new AudioBuffer();
            bgmSource = new AudioSource();
            try
            {
                bgmBuffer.Init("SFX/Kubbi.wav");
                canPlayMusic = true;
            }
            catch
            {
                canPlayMusic = false;
            }
            if (canPlayMusic)
            {
                bgmSource.SetBuffer(bgmBuffer);
                bgmSource.SetLoop(true);
            }



            PauseInBuffer = new AudioBuffer();
            PauseInSource = new AudioSource();
            PauseInBuffer.Init("SFX/PauseIn.wav");
            PauseInSource.SetBuffer(PauseInBuffer);

            PauseOutBuffer = new AudioBuffer();
            PauseOutSource = new AudioSource();
            PauseOutBuffer.Init("SFX/PauseOut.wav");
            PauseOutSource.SetBuffer(PauseOutBuffer);

            game.CursorVisible = false;
            if (canPlayMusic)
            {
                bgmSource.Play();
            }
        }
        public static bool boop = false;
        bool once = true;
        bool state = false;
        Vector2i prevCursorPos;
        public void Run(EcsSystems systems)
        {
            foreach (var ent in StartEventFilter)
            {
                state = StartEvents.Get(ent).State;
                if (state)
                {
                    if (once)
                    {
                        once = false;
                    }
                    else
                    {
                        PauseOutSource.Play();
                    }
                }
                else
                {
                    PauseInSource.Play();
                }
                StartEvents.Del(ent);
            }
            var c = Color4.FromHsv(new Vector4(game.Time * 0.2f % 1f, 1, 1, 1));
            var layer = game.ActiveLayer;
            if (state)
            {
                foreach (var entity in PlayerFilter)
                {
                    ref var player = ref Players.Get(entity);
                    DrawHearts(layer, 10, player.HP, player.Armor, 1, 1);
                }
                DrawNumber(layer, (int)game.Score, 124, 1, Color4.Green);
            }
            layer.DrawLine(prevCursorPos, game.CursorPos, c);
            layer.DrawPixel(game.CursorPos, c);
            prevCursorPos = game.CursorPos;

            if ((game.KeyboardState.IsKeyPressed(Keys.Escape) && !ShopSystem.active) || boop)
            {
                boop = false;
                StartEvents.Add(Events.NewEntity()).State = !state;
            }
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
