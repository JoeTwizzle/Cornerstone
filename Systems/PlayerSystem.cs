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
using ALAudio;

namespace Cornerstone.Systems
{
    [EcsRead("Events", typeof(ResetGameEvent))]
    [EcsWrite("Default", typeof(Player), typeof(Bullet))]
    internal class PlayerSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        readonly EcsWorld world;
        readonly EcsWorld events;
        readonly EcsPool<ResetGameEvent> ResetEvents;
        readonly EcsFilter ResetEventFilter;

        readonly EcsPool<Player> Players;
        readonly EcsFilter PlayerFilter;
        readonly EcsPool<Bullet> Bullets;

        float timeAccumulator;
        readonly AudioSource playerJumpSource;
        readonly AudioBuffer playerJumpBuffer;
        readonly AudioSource[] shotSources = new AudioSource[10];//10 simultaneous sounds
        readonly AudioBuffer shotBuffer;
        int shotIndex = 0;

        float gravity = 200f;
        float airTime = 0;

        public PlayerSystem(EcsSystems systems) : base(systems)
        {
            ResetEventFilter = FilterInc<ResetGameEvent>("Events").End();
            ResetEvents = GetPool<ResetGameEvent>("Events");
            events = GetWorld("Events");
            Players = GetPool<Player>();
            PlayerFilter = FilterInc<Player>().End();
            game = GetSingleton<MyGame>();
            world = GetWorld();
            Bullets = GetPool<Bullet>();


            shotBuffer = new AudioBuffer();
            shotBuffer.Init("SFX/Shoot.wav");
            for (int i = 0; i < shotSources.Length; i++)
            {
                shotSources[i] = new AudioSource();
                shotSources[i].SetBuffer(shotBuffer);
                shotSources[i].SetVolume(0.25f);
            }
            playerJumpBuffer = new AudioBuffer();
            playerJumpBuffer.Init("SFX/jump.wav");
            playerJumpSource = new AudioSource();
            playerJumpSource.SetBuffer(playerJumpBuffer);
            playerJumpSource.SetVolume(0.3f);
        }

        void PlaySound()
        {
            shotSources[shotIndex].SetPitch(Random.Shared.NextSingle() * 0.2f + 0.9f);
            shotSources[shotIndex].Play();
            shotIndex++;
            shotIndex %= shotSources.Length;
        }

        public void Run(float elapsed, int threadId)
        {
            foreach (var entity in ResetEventFilter)
            {
                game.Score = 0;
                ResetEvents.Del(entity);
                foreach (var oldPlayer in PlayerFilter)
                {
                    Players.Del(oldPlayer);
                }
                var playerEnt = world.NewEntity();
                Players.Add(playerEnt);
            }

            float dt = elapsed;
            var targetTimestepDuration = 1 / 120f;

            timeAccumulator += dt;
            while (timeAccumulator >= targetTimestepDuration)
            {
                Simulate(targetTimestepDuration, systems);
                timeAccumulator -= targetTimestepDuration;
            }

            var layer = game.ActiveLayer;
            foreach (var entity in PlayerFilter)
            {
                ref var player = ref Players.Get(entity);
                Color4 color = player.PlayerState switch
                {
                    PlayerState.Idle => Color4.Yellow,
                    PlayerState.Normal => Color4.Green,
                    PlayerState.Stunned => Color4.Red,
                    PlayerState.Crouching => Color4.Pink,
                    _ => Color4.Cyan,
                };
                //var dirToCursor = (game.CursorPos - player.Position).Normalized();
                //////velocity
                //float dist = Vector2.Distance(game.CursorPos, player.Position)*0.3f;
                //float t = 5;
                //while (t < dist-5)
                //{
                //    t += 10;
                //    t = Math.Min(t, dist);
                //    layer.DrawLine((Vector2i)(player.Position + dirToCursor * t), (Vector2i)(player.Position + dirToCursor * (t - 5)), new Color4(0.7f, 0.7f, 0.7f, 1f));
                //}
                //layer.DrawLine((Vector2i)(player.Position + dirToCursor * 5f), (Vector2i)player.Position, new Color4(0.7f, 0.7f, 0.7f, 1f));
                //player
                layer.DrawPixel((Vector2i)player.Position, color);
                layer.DrawLine((Vector2i)(player.Position - player.Velocity * targetTimestepDuration), (Vector2i)player.Position, color);
            }
        }

        void Simulate(float dt, EcsSystems systems)
        {
            foreach (var entity in PlayerFilter)
            {
                ref var player = ref Players.Get(entity);
                if (player.HP <= 0)
                {
                    ResetEvents.Add(events.NewEntity());
                }
                player.InvincibleTimer -= dt;
                var kb = game.KeyboardState;
                if (player.PlayerState == PlayerState.Idle)
                {
                    if (kb.IsKeyDown(Keys.LeftShift))
                    {
                        player.PlayerState = PlayerState.Crouching;
                    }
                    if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Space))
                    {
                        player.PlayerState = PlayerState.Normal;
                    }
                }
                player.TimeSinceLastShot += dt;
                if (player.TimeSinceLastShot >= player.FireRate && game.MouseState.IsButtonDown(MouseButton.Left))
                {
                    PlaySound();
                    player.TimeSinceLastShot = 0;
                    var bEnt = world.NewEntity();
                    ref var bullet = ref Bullets.Add(bEnt);
                    bullet.LifeTime = 10f;
                    bullet.PrevPosition = bullet.Position = player.Position;
                    Vector2 dir = (game.CursorPos - player.Position);
                    float dist = Vector2.Distance(game.CursorPos, player.Position);
                    float strength = MathHelper.Lerp(100, 180, Math.Clamp(dir.Length, 0, 50) / 50f);
                    bullet.Velocity = dir.Normalized() * strength;
                    bullet.Velocity += player.Velocity * 0.3f;
                }

                float moveSpeed = player.MoveLevel switch
                {
                    0 => 80,
                    1 => 90,
                    2 => 100,
                    3 => 110,
                    _ => 120,
                };
                float maxMoveSpeed = player.MoveLevel switch
                {
                    0 => 40f,
                    1 => 45f,
                    2 => 50f,
                    3 => 55f,
                    _ => 60f,
                };
                float maxCrouchSpeed = player.MoveLevel switch
                {
                    0 => 20f,
                    1 => 25f,
                    2 => 30f,
                    3 => 35f,
                    _ => 40f,
                };
                if (player.PlayerState == PlayerState.Normal)
                {
                    if (kb.IsKeyDown(Keys.LeftShift))
                    {
                        player.PlayerState = PlayerState.Crouching;
                    }
                }
                else if (player.PlayerState == PlayerState.Crouching)
                {
                    if (!kb.IsKeyDown(Keys.LeftShift))
                    {
                        player.PlayerState = PlayerState.Normal;
                    }
                }
                if (player.Position.X < -10)
                {
                    player.Position.X = -10;
                    player.Velocity.X = 80f;
                }
                if (player.Position.X > 138)
                {
                    player.Position.X = 138;
                    player.Velocity.X = -80f;
                }
                if (IsGrounded(ref player))
                {
                    airTime = 0;
                    player.Velocity.Y = 0;
                    player.Position.Y = 80;
                    if (kb.IsKeyDown(Keys.Space))
                    {
                        if (player.PlayerState == PlayerState.Crouching)
                        {
                            playerJumpSource.SetPitch(0.8f);
                            player.Velocity.Y -= player.JumpLevel switch
                            {
                                0 => 120,
                                1 => 130,
                                2 => 145,
                                3 => 150,
                                _ => 160,
                            };
                        }
                        if (player.PlayerState == PlayerState.Normal)
                        {
                            playerJumpSource.SetPitch(1f);
                            player.Velocity.Y -= player.JumpLevel switch
                            {
                                0 => 80,
                                1 => 85,
                                2 => 98,
                                3 => 112,
                                _ => 120,
                            };
                        }
                        playerJumpSource.Play();
                    }
                }
                else
                {
                    airTime += dt;
                    float gravMod = (1 + airTime) * dt;
                    if (kb.IsKeyDown(Keys.Space))
                    {
                        player.Velocity.Y += gravity * gravMod;
                    }
                    else
                    {
                        if (player.Velocity.Y < 0)
                        {
                            player.Velocity.Y += gravity * gravMod * 1.2f;
                        }
                        else
                        {
                            player.Velocity.Y += gravity * gravMod * 1.8f;
                        }
                    }
                }
                float xMoveSpeed = 0;
                if (kb.IsKeyDown(Keys.A))
                {
                    xMoveSpeed = -moveSpeed;
                }
                if (kb.IsKeyDown(Keys.D))
                {
                    xMoveSpeed = moveSpeed;
                }
                float maxSpeed = player.PlayerState switch
                {
                    PlayerState.Normal => maxMoveSpeed,
                    PlayerState.Crouching => maxCrouchSpeed,
                    _ => 0,
                };

                if (MathF.Abs(player.Velocity.X) > maxSpeed)
                {
                    player.Velocity.X = MyMathHelper.Approach(player.Velocity.X, maxSpeed, 1.5f * moveSpeed * dt);
                }
                else
                {
                    player.Velocity.X = MyMathHelper.Approach(player.Velocity.X, xMoveSpeed, moveSpeed * dt);
                }
                player.Position += player.Velocity * dt;
            }
        }

        bool IsGrounded(ref Player player)
        {
            return player.Position.Y > 80;
        }

    }
}
