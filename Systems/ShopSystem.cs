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
    internal class ShopSystem : IEcsRunSystem, IEcsInitSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld]
        EcsWorld world = null!;

        [EcsWorld("Events")]
        EcsWorld events = null!;

        [EcsPool("Events")]
        EcsPool<StartEvent> StartEvents = null!;

        [EcsFilter("Events", typeof(StartEvent))]
        EcsFilter StartEventFilter = null!;

        [EcsPool("Events")]
        EcsPool<ResetGameEvent> ResetEvents = null!;

        [EcsFilter("Events", typeof(ResetGameEvent))]
        EcsFilter ResetEventFilter = null!;

        [EcsPool]
        EcsPool<Player> Players = null!;

        [EcsFilter(typeof(Player))]
        EcsFilter PlayerFilter = null!;

        [EcsPool("Events")]
        EcsPool<ShopEvent> ShopEvents = null!;

        [EcsFilter("Events", typeof(ShopEvent))]
        EcsFilter ShopEventFilter = null!;
        
        Sprite shopBG = null!;
        public void Init(EcsSystems systems)
        {
            shopBG = new Sprite("ShopBG.png");
        }
        
        //Animation drawTextAnim = new Animation(0, 0.5f, Easing.Function.Linear, false);
        Color4 hoverColor = new Color4(255, 0, 77, 255);
        Color4 outlineColor = new Color4(0.1f, 0.4f, 0.8f, 0.4f);
        Color4 bgColor = new Color4(29, 43, 83, 180);
        Color4 inActiveColor = new Color4(32, 47, 54, 255);
        public void Run(EcsSystems systems)
        {
            game.DisableGroup("Pauseable");
            var layer = game.ActiveLayer;
            string Wa = "SUPPLIES";
            layer.DrawSprite(0, 0, shopBG);
            //layer.FillBox(0, 0, layer.Width, layer.Height, Color4.Black, BlendMode.None);
            var chars = Wa.AsSpan();
            var slice = chars.Slice(0, Math.Clamp((int)(1f * chars.Length), 0, chars.Length));
            game.TextRenderer.DrawText(new Vector2(game.GameArea.X *0.5f, -0.015f * game.GameArea.Y), slice, Color4.White, TextLayout.CenterAlign);
            foreach (var ent in PlayerFilter)
            {
                ref var player = ref Players.Get(ent);
                DrawHudSystem.DrawHearts(layer, 10, player.HP, player.Armor, 1, 1);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, -0.015f * game.GameArea.Y), "POINTS " + game.Score, Color4.Aqua, TextLayout.RightAlign);
                //------------HP-----------
                ulong hpUpgradeCost = 50000;
                game.TextRenderer.DrawText(new Vector2(0.065f * game.GameArea.X, .049f * game.GameArea.Y), "HEAl " + hpUpgradeCost, BuyColor(player.HP < 10, hpUpgradeCost), TextLayout.LeftAlign);
                Panel hpPanel = new Panel(1, 5, 7, 7, bgColor, outlineColor);
                if (PanelHover(ref hpPanel, player.HP < 10, hpUpgradeCost))
                {
                    if (MousePressed())
                    {
                        game.Score -= hpUpgradeCost;
                        player.HP++;
                        if (player.HP > 10)
                        {
                            player.HP = 10;
                        }
                    }
                }
                DrawPanel(hpPanel);
                //------------Tears-----------
                Panel shotPanel = new Panel(100, 30, 7, 7, bgColor, outlineColor);
                ulong shotUpgradeCost = player.ShootLevel switch
                {
                    0 => 150000,
                    1 => 800000,
                    2 => 1000000,
                    3 => 1500000,
                    _ => 2000000,
                };
                if (PanelHover(ref shotPanel, player.ShootLevel < 4, shotUpgradeCost))
                {
                    if (MousePressed())
                    {
                        player.ShootLevel++;
                        player.FireRate = player.ShootLevel switch
                        {
                            0 => 1 / 3f,
                            1 => 1 / 4.5f,
                            2 => 1 / 5f,
                            3 => 1 / 6.25f,
                            _ => 1 / 7f,
                        };
                        game.Score -= shotUpgradeCost;
                    }
                }
                DrawPanel(shotPanel);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.18f * game.GameArea.Y), "FIRE RATE " + shotUpgradeCost, BuyColor(player.ShootLevel < 4, shotUpgradeCost), TextLayout.RightAlign);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.21f * game.GameArea.Y), "LV " + (player.ShootLevel + 1), BuyColor(player.ShootLevel < 4, shotUpgradeCost), TextLayout.RightAlign);
                //------------MOVE SPEED-------------
                Panel movePanel = new Panel(100, 42, 7, 7, bgColor, outlineColor);
                ulong moveUpgradeCost = player.MoveLevel switch
                {
                    0 => 50000,
                    1 => 250000,
                    2 => 500000,
                    3 => 700000,
                    _ => 1000000,
                };
                if (PanelHover(ref movePanel, player.MoveLevel < 4, moveUpgradeCost))
                {
                    if (MousePressed())
                    {
                        player.MoveLevel++;
                        game.Score -= moveUpgradeCost;
                    }
                }
                DrawPanel(movePanel);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.275f * game.GameArea.Y), "MOVE SPEED " + moveUpgradeCost, BuyColor(player.MoveLevel < 4, moveUpgradeCost), TextLayout.RightAlign);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.305f * game.GameArea.Y), "LV " + (player.MoveLevel + 1), BuyColor(player.MoveLevel < 4, moveUpgradeCost), TextLayout.RightAlign);
                //------------JUMP STRENGTH-------------
                Panel jumpPanel = new Panel(100, 55, 7, 7, bgColor, outlineColor);
                ulong jumpUpgradeCost = player.JumpLevel switch
                {
                    0 => 80000,
                    1 => 120000,
                    2 => 250000,
                    3 => 350000,
                    _ => 500000,
                };
                if (PanelHover(ref jumpPanel, player.JumpLevel < 4, jumpUpgradeCost))
                {
                    if (MousePressed())
                    {
                        player.JumpLevel++;
                        game.Score -= jumpUpgradeCost;
                    }
                }
                DrawPanel(jumpPanel);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.38f * game.GameArea.Y), "JUMP STRENGTH " + jumpUpgradeCost, BuyColor(player.JumpLevel < 4, jumpUpgradeCost), TextLayout.RightAlign);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.41f * game.GameArea.Y), "LV " + (player.JumpLevel + 1), BuyColor(player.JumpLevel < 4, jumpUpgradeCost), TextLayout.RightAlign);

                //------------Armor-------------
                Panel armorPanel = new Panel(100, 68, 7, 7, bgColor, outlineColor);
                ulong armorUpgradeCost = 25000;
                if (PanelHover(ref armorPanel, player.Armor < 10, armorUpgradeCost))
                {
                    if (MousePressed())
                    {
                        player.Armor++;
                        game.Score -= armorUpgradeCost;
                    }
                }
                DrawPanel(armorPanel);
                game.TextRenderer.DrawText(new Vector2(1f * game.GameArea.X, 0.48f * game.GameArea.Y), "ARMOR " + armorUpgradeCost, BuyColor(player.Armor < 10, armorUpgradeCost), TextLayout.RightAlign);

                Panel closePanel = new Panel(108, 120, 19, 7, bgColor, outlineColor);
                if (PanelHover(ref closePanel, true, 0))
                {
                    if (MousePressed())
                    {
                        game.EnableGroup("Game");
                        game.DisableGroup("Shop");
                        game.EnableGroupNextFrame("Pauseable");
                    }
                }
                closePanel.Draw(layer, BlendMode.Alpha);
                layer.DrawLine(closePanel.PosX + 2, closePanel.PosY + 3, closePanel.PosX + 17, closePanel.PosY + 3, closePanel.OutlineColor, BlendMode.Alpha);
                layer.DrawPixel(closePanel.PosX + 15, closePanel.PosY + 2, closePanel.OutlineColor, BlendMode.Alpha);
                layer.DrawPixel(closePanel.PosX + 15, closePanel.PosY + 4, closePanel.OutlineColor, BlendMode.Alpha);
                layer.DrawPixel(closePanel.PosX + 14, closePanel.PosY + 2, closePanel.OutlineColor, BlendMode.Alpha);
                layer.DrawPixel(closePanel.PosX + 14, closePanel.PosY + 4, closePanel.OutlineColor, BlendMode.Alpha);
                layer.DrawPixel(closePanel.PosX + 14, closePanel.PosY + 1, closePanel.OutlineColor, BlendMode.Alpha);
                layer.DrawPixel(closePanel.PosX + 14, closePanel.PosY + 5, closePanel.OutlineColor, BlendMode.Alpha);
            }

            bool MousePressed()
            {
                if (game.MouseState.IsButtonDown(MouseButton.Left) && !game.MouseState.WasButtonDown(MouseButton.Left))
                {
                    PauseGameSystem.PauseInSource.Play();
                    return true;
                }
                return false;
            }

            bool PanelHover(ref Panel panel, bool active, ulong cost)
            {
                if (!active)
                {
                    panel.OutlineColor = inActiveColor;
                    return false;
                }
                if (panel.IsInside(game.CursorPos) && game.Score >= cost)
                {
                    panel.OutlineColor = hoverColor;
                    return true;
                }
                else
                {
                    panel.OutlineColor = outlineColor;
                    return false;
                }
            }

            void DrawPanel(Panel panel)
            {
                var c = panel.OutlineColor;
                panel.Draw(layer, BlendMode.Alpha);
                layer.DrawPixel(panel.PosX + 3, panel.PosY + 2, c, BlendMode.Alpha);
                layer.DrawPixel(panel.PosX + 3, panel.PosY + 3, c, BlendMode.Alpha);
                layer.DrawPixel(panel.PosX + 3, panel.PosY + 4, c, BlendMode.Alpha);
                layer.DrawPixel(panel.PosX + 2, panel.PosY + 3, c, BlendMode.Alpha);
                layer.DrawPixel(panel.PosX + 4, panel.PosY + 3, c, BlendMode.Alpha);
            }

            Color4 BuyColor(bool available, ulong cost)
            {
                if (!available)
                {
                    return inActiveColor;
                }
                return game.Score >= cost ? Color4.Green : Color4.Red;
            }
        }
    }
}
