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
using ALAudio;

namespace Cornerstone.Systems
{
    internal class BulletSystem : IEcsRunSystem, IEcsInitSystem
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

        [EcsPool]
        EcsPool<Player> Players = null!;

        [EcsFilter(typeof(Player))]
        EcsFilter PlayerFilter = null!;

        [EcsPool]
        EcsPool<Bullet> Bullets = null!;

        [EcsFilter(typeof(Bullet))]
        EcsFilter BulletFilter = null!;

        [EcsPool]
        EcsPool<Enemy> Enemies = null!;

        [EcsPool]
        EcsPool<Transform> Transforms = null!;

        [EcsPool]
        EcsPool<Explosion> Explosions = null!;

        [EcsPool]
        EcsPool<SpriteAnimation> SpriteAnimations = null!;

        [EcsPool]
        EcsPool<Lifetime> Lifetimes = null!;

        [EcsFilter(typeof(Enemy))]
        EcsFilter EnemyFilter = null!;

        Sprite explosionSprite = null!;
        AudioSource playerExplosionSource = null!;
        AudioBuffer playerExplosionBuffer = null!;
        AudioSource playerHitSource = null!;
        AudioBuffer playerHitBuffer = null!;
        AudioSource[] explosionSources = new AudioSource[10];//20 simultaneous sounds
        AudioBuffer explosionBuffer = null!;
        int explosionIndex = 0;
        public void Init(EcsSystems systems)
        {
            playerExplosionBuffer = new AudioBuffer();
            playerExplosionBuffer.Init("SFX/Death.wav");
            playerExplosionSource = new AudioSource();
            playerExplosionSource.SetBuffer(playerExplosionBuffer);
            playerHitBuffer = new AudioBuffer();
            playerHitBuffer.Init("SFX/Hit.wav");
            playerHitSource = new AudioSource();
            playerHitSource.SetBuffer(playerHitBuffer);
            explosionSprite = new Sprite("SpriteSheets/Explosion.png");
            explosionBuffer = new AudioBuffer();
            explosionBuffer.Init("SFX/Explosion.wav");
            for (int i = 0; i < explosionSources.Length; i++)
            {
                explosionSources[i] = new AudioSource();
                explosionSources[i].SetBuffer(explosionBuffer);
                explosionSources[i].SetVolume(0.25f);
            }
        }

        void PlaySound()
        {
            explosionSources[explosionIndex].Play();
            explosionIndex++;
            explosionIndex %= explosionSources.Length;
        }
        bool active = false;
        float timeAccumulator;
        public void Run(EcsSystems systems)
        {
            float dt = game.DeltaTime;
            timeAccumulator += dt;
            var targetTimestepDuration = 1 / 120f;
            while (timeAccumulator >= targetTimestepDuration)
            {
                Simulate(targetTimestepDuration, systems);
                timeAccumulator -= targetTimestepDuration;
            }

            var layer = game.ActiveLayer;
            foreach (var entity in BulletFilter)
            {
                ref var bullet = ref Bullets.Get(entity);

                var bColor = bullet.Team == 0 ? new Color4(1, 1, 1, 1f) : new Color4(1, 0, 0, 1f);
                if (bullet.BulletType.HasFlag(BulletType.Explosive))
                {
                    bColor = new Color4(0.3f, 0.3f, 1f, 1);
                }
                if (float.IsFinite(bullet.Position.X) && float.IsFinite(bullet.Position.Y))
                {
                    layer.DrawPixel((Vector2i)bullet.Position, bColor);
                    layer.DrawLine((Vector2i)bullet.Position, (Vector2i)bullet.PrevPosition, bColor);
                }
            }
        }

        void Simulate(float dt, EcsSystems systems)
        {
            foreach (var entity in BulletFilter)
            {
                ref var bullet = ref Bullets.Get(entity);
                if (bullet.Team == Team.Enemy)
                {
                    bullet.Velocity.Y += 100 * dt;
                }
                else
                {
                    bullet.Velocity.Y += 50 * dt;
                    //bullet.Velocity += (game.MouseState.Position - game.MouseState.PreviousPosition) * 0.1f;
                }

                bullet.PrevPosition = bullet.Position;
                bullet.Position += bullet.Velocity * dt;
                bullet.LifeTime -= dt;
                if (bullet.Position.Y > 81 && bullet.BulletType.HasFlag(BulletType.Explosive))
                {
                    var ent = world.NewEntity();
                    ref var explosion = ref Explosions.Add(ent);
                    ref var transform = ref Transforms.Add(ent);
                    transform.Position = bullet.Position;
                    explosion.Duration = 0.8f;
                    explosion.team = Team.Enemy;
                    explosion.Size = Random.Shared.Next(3, 10);
                    //BOOM
                }
                if (bullet.LifeTime <= 0 || bullet.Position.Y > 81)
                {
                    Bullets.Del(entity);
                }
            }
            foreach (var playerEntity in PlayerFilter)
            {
                ref var player = ref Players.Get(playerEntity);
                var playerPos = (Vector2i)player.Position;
                foreach (var entity in BulletFilter)
                {
                    ref var bullet = ref Bullets.Get(entity);
                    if (bullet.Team == Team.Enemy && player.InvincibleTimer <= 0 && ((Vector2i)bullet.Position) == playerPos)
                    {
                        playerHitSource.SetPitch(Random.Shared.NextSingle() * 0.5f + 0.75f);
                        playerHitSource.Play();
                        if (player.Armor > 0)
                        {
                            player.Armor--;
                        }
                        else
                        {
                            player.HP--;
                            if (player.HP <= 0)
                            {
                                playerExplosionSource.Play();
                            }
                        }

                        player.InvincibleTimer = 0.5f;
                        Bullets.Del(entity);
                        if (bullet.BulletType.HasFlag(BulletType.Explosive))
                        {
                            var ent = world.NewEntity();
                            ref var explosion = ref Explosions.Add(ent);
                            ref var transform = ref Transforms.Add(ent);
                            transform.Position = bullet.Position;
                            explosion.Duration = 0.8f;
                            explosion.team = Team.Enemy;
                            explosion.Size = Random.Shared.Next(3, 10);
                        }
                        break;
                    }
                }
            }
            foreach (var playerBulletEnt in BulletFilter)
            {
                ref var bullet = ref Bullets.Get(playerBulletEnt);
                var bulletPos = (Vector2i)bullet.Position;
                foreach (var entity in EnemyFilter)
                {
                    ref var enemy = ref Enemies.Get(entity);
                    ref var transform = ref Transforms.Get(entity);
                    Vector2i ePos = (Vector2i)transform.Position;
                    Box2i hb = new Box2i(enemy.HitBox.Min.X + ePos.X, enemy.HitBox.Min.Y + ePos.Y, enemy.HitBox.Max.X + ePos.X, enemy.HitBox.Max.Y + ePos.Y);

                    if (bullet.Team == Team.Player && hb.Contains(bulletPos))
                    {
                        enemy.HP--;
                        world.DelEntity(playerBulletEnt);
                        game.Score += 580;
                        if (enemy.HP <= 0)
                        {
                            PlaySound();
                            game.Score += (ulong)(935 * Random.Shared.Next(1, 5));
                            var explosion = world.NewEntity();
                            ref var expAnimimation = ref SpriteAnimations.Add(explosion);
                            ref var expTransform = ref Transforms.Add(explosion);
                            ref var expLifetime = ref Lifetimes.Add(explosion);
                            expTransform.Position = transform.Position;
                            expTransform.Velocity = transform.Velocity;
                            expLifetime.Time = 0.5f;
                            expAnimimation.Sprite = explosionSprite;
                            expAnimimation.AnimationFrameCount = 8;
                            expAnimimation.FrameWidth = 9;
                            expAnimimation.AnimationFrameRate = 1 / 15f;

                            world.DelEntity(entity);
                        }
                        break;
                    }
                }
            }
        }
    }
}
