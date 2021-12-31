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
    internal class EnemySpawnSystem : IEcsRunSystem, IEcsInitSystem
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

        [EcsPool]
        EcsPool<Enemy1> Enemies1 = null!;

        [EcsPool]
        EcsPool<Enemy2> Enemies2 = null!;

        [EcsPool]
        EcsPool<Enemy3> Enemies3 = null!;

        [EcsPool]
        EcsPool<Enemy> Enemies = null!;

        [EcsPool]
        EcsPool<Transform> Transforms = null!;

        [EcsPool]
        EcsPool<SpriteAnimation> Visuals = null!;

        [EcsFilter(typeof(Transform), typeof(Enemy))]
        EcsFilter EnemyFilter = null!;

        [EcsPool]
        EcsPool<Bullet> Bullets = null!;

        [EcsFilter(typeof(Bullet))]
        EcsFilter BulletFilter = null!;

        bool active = false;
        float enemy1Timer = 0;
        float enemy2Timer = 0;
        float enemy3Timer = 0;
        int currentWave = 0;

        Sprite enemy1Sprite = null!;
        Sprite enemy2Sprite = null!;
        Sprite enemy3Sprite = null!;

        public void Init(EcsSystems systems)
        {
            enemy1Sprite = new Sprite("SpriteSheets/Enemy-1.png");
            enemy2Sprite = new Sprite("SpriteSheets/Enemy-2.png");
            enemy3Sprite = new Sprite("SpriteSheets/Enemy-3.png");
        }
        float gracePeriod = 5;
        float waveLength = 60;
        public void Run(EcsSystems systems)
        {
            foreach (var entity in ResetEventFilter)
            {
                enemy1Timer = 0;
                enemy2Timer = 0;
                enemy3Timer = 0;
                waveLength = 90;
                currentWave = 0;
                gracePeriod = 5;
                foreach (var enemy in EnemyFilter)
                {
                    world.DelEntity(enemy);
                }
                foreach (var bullet in BulletFilter)
                {
                    world.DelEntity(bullet);
                }
            }
            foreach (var entity in StartEventFilter)
            {
                active = StartEvents.Get(entity).State;
            }
            if (!active)
            {
                return;
            }
            if (gracePeriod < 0 && waveLength - game.DeltaTime <= 0)
            {
                foreach (var entity in EnemyFilter)
                {
                    world.DelEntity(entity);
                }
                foreach (var entity in BulletFilter)
                {
                    world.DelEntity(entity);
                }
                var ent = events.NewEntity();
                ShopEvents.Add(ent).Enter = true;
                active = false;
                var ent2 = events.NewEntity();
                StartEvents.Add(ent2).State = false;
            }
            if (waveLength <= 0)
            {
                gracePeriod = 5;
            }
            gracePeriod -= game.DeltaTime;
            if (gracePeriod <= 0)
            {
                waveLength -= game.DeltaTime;
                if (waveLength <= 0)
                {
                    gracePeriod = 5;
                    waveLength = 90;
                    currentWave++;
                }

                foreach (var entity in EnemyFilter)
                {
                    ref var enemy = ref Transforms.Get(entity);
                    if (enemy.Position.X < -10 || enemy.Position.X > 190 || enemy.Position.Y < -10 || enemy.Position.Y > 190)
                    {
                        world.DelEntity(entity);
                    }
                }
                if (gracePeriod <= 0)
                {
                    TrySpawnEnemy1();
                    TrySpawnEnemy2();
                    TrySpawnEnemy3();
                }
            }
            else
            {
                game.textRenderer.TextScale = new Vector2(40, 40);
                game.textRenderer.DrawText(new Vector2(game.GameArea.X / 2f, game.GameArea.Y / 3f), "DAY " + (currentWave + 1), Color4.White, TextLayout.CenterAlign);
            }
        }
        void TrySpawnEnemy1()
        {
            enemy1Timer -= game.DeltaTime;
            if (enemy1Timer <= 0)
            {
                int amount = Random.Shared.Next(1, 2);
                for (int i = 0; i < amount; i++)
                {
                    Vector2 moveDir = Vector2.Zero;
                    switch (currentWave)
                    {
                        case 0:
                            enemy1Timer = 2;
                            moveDir.X = Random.Shared.NextSingle() * 17 + 13;
                            moveDir.Y = Random.Shared.NextSingle() * 2.5f;
                            break;
                        case 1:
                            enemy1Timer = 2;
                            moveDir.X = Random.Shared.NextSingle() * 17 + 16;
                            moveDir.Y = Random.Shared.NextSingle() * 2.5f;
                            break;
                        case 2:
                            enemy1Timer = 2;
                            moveDir.X = Random.Shared.NextSingle() * 17 + 16;
                            moveDir.Y = Random.Shared.NextSingle() * 2.5f;
                            break;
                        case 3:
                            enemy1Timer = 1.9f;
                            moveDir.X = Random.Shared.NextSingle() * 27 + 13;
                            moveDir.Y = ((Random.Shared.NextSingle() * 2) - 1) * 2.5f;
                            break;
                        default:
                            enemy1Timer = 1;
                            moveDir.X = Random.Shared.NextSingle() * 17 + 16;
                            moveDir.Y = Random.Shared.NextSingle() * 2.5f;
                            break;
                    }

                    var ent = world.NewEntity();
                    ref var baseEnemy = ref Enemies.Add(ent);
                    ref var testEnemy = ref Enemies1.Add(ent);
                    ref var visual = ref Visuals.Add(ent);
                    ref var transform = ref Transforms.Add(ent);
                    visual.AnimationFrameRate = 1 / 20f;
                    visual.FrameWidth = 5;
                    visual.Sprite = enemy1Sprite;
                    transform.Position.X = -4;
                    transform.Position.Y = Random.Shared.Next(3, 40);
                    transform.Velocity = moveDir;
                }
            }
        }
        void TrySpawnEnemy2()
        {
            enemy2Timer -= game.DeltaTime;
            if (enemy2Timer <= 0)
            {
                Vector2 moveDir = Vector2.Zero;
                switch (currentWave)
                {
                    case 0:
                        return;
                    case 1:
                        enemy2Timer = 2;
                        moveDir.X = Random.Shared.NextSingle() * 13 + 16;
                        moveDir.Y = Random.Shared.NextSingle() * 2.5f;
                        break;
                    case 2:
                        enemy2Timer = 3;
                        moveDir.X = Random.Shared.NextSingle() * 13 + 13;
                        moveDir.Y = ((Random.Shared.NextSingle() * 2) - 1) * 3.5f;
                        break;
                    case 3:
                        enemy2Timer = 2;
                        moveDir.X = Random.Shared.NextSingle() * 13 + 13;
                        moveDir.Y = ((Random.Shared.NextSingle() * 2) - 1) * 3f;
                        break;
                    default:
                        enemy2Timer = 1;
                        moveDir.X = Random.Shared.NextSingle() * 13 + 13;
                        moveDir.Y = ((Random.Shared.NextSingle() * 2) - 1) * 4f;
                        break;
                }

                var ent = world.NewEntity();
                ref var baseEnemy = ref Enemies.Add(ent);
                ref var enemy2 = ref Enemies2.Add(ent);
                ref var transform = ref Transforms.Add(ent);
                ref var visual = ref Visuals.Add(ent);
                baseEnemy.HitBox = new Box2i(-3, -3, 4, 3);
                baseEnemy.HP = 2;
                visual.AnimationFrameRate = 1 / 20f;
                visual.FrameWidth = 7;
                visual.Sprite = enemy2Sprite;
                transform.Position.X = -4;
                transform.Position.Y = Random.Shared.Next(3, 40);
                transform.Velocity = moveDir;
            }
        }

        void TrySpawnEnemy3()
        {
            enemy3Timer -= game.DeltaTime;
            if (enemy3Timer <= 0)
            {
                Vector2 moveDir = Vector2.Zero;
                switch (currentWave)
                {
                    case 0:
                        return;
                    case 1:
                        return;
                    case 2:
                        return;
                    case 3:
                        enemy3Timer = 6f;
                        moveDir.X = Random.Shared.NextSingle() * 7 + 7;
                        break;
                    case 4:
                        enemy3Timer = 6f;
                        moveDir.X = Random.Shared.NextSingle() * 10 + 7;
                        break;
                    case 5:
                        enemy3Timer = 6f;
                        moveDir.X = Random.Shared.NextSingle() * 10 + 8;
                        break;
                    default:
                        enemy3Timer = 5.5f;
                        moveDir.X = Random.Shared.NextSingle() * 10 + 8;
                        break;
                }

                var ent = world.NewEntity();

                ref var baseEnemy = ref Enemies.Add(ent);
                ref var testEnemy = ref Enemies3.Add(ent);
                ref var transform = ref Transforms.Add(ent);
                ref var visual = ref Visuals.Add(ent);
                baseEnemy.HP = 4;
                visual.Sprite = enemy3Sprite;
                visual.AnimationFrameRate = 1 / 15f;
                visual.FrameWidth = 9;
                transform.Position.X = -4;
                transform.Position.Y = 78;
                transform.Velocity = moveDir;
            }
        }
    }
}
