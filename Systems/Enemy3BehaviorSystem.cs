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
    internal class Enemy3BehaviorSystem : IEcsRunSystem, IEcsInitSystem
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
        EcsPool<Enemy3> Enemies3 = null!;

        [EcsPool]
        EcsPool<Enemy> Enemies = null!;

        [EcsFilter(typeof(Transform), typeof(Enemy3), typeof(Enemy))]
        EcsFilter TestEnemyFilter = null!;

        [EcsPool]
        EcsPool<Transform> Transforms = null!;

        [EcsPool]
        EcsPool<Bullet> Bullets = null!;

        [EcsFilter(typeof(Bullet))]
        EcsFilter BulletFilter = null!;

        bool active = false;
        float timeAccumulator;

        public void Init(EcsSystems systems)
        {
            
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in StartEventFilter)
            {
                active = StartEvents.Get(entity).State;
            }
            float dt = game.DeltaTime;
            if (active)
            {
                timeAccumulator += dt;
                var targetTimestepDuration = 1 / 120f;
                while (timeAccumulator >= targetTimestepDuration)
                {
                    Simulate(targetTimestepDuration, systems);
                    timeAccumulator -= targetTimestepDuration;
                }
            }
        }

        private void Simulate(float dt, EcsSystems systems)
        {
            foreach (var entity in TestEnemyFilter)
            {
                ref var transform = ref Transforms.Get(entity);
                ref var baseEnemy = ref Enemies.Get(entity);
                ref var testEnemy = ref Enemies3.Get(entity);
                baseEnemy.TimeToNextShot -= dt;
                if (baseEnemy.TimeToNextShot <= 0)
                {
                    foreach (var playerEnt in PlayerFilter)
                    {
                        baseEnemy.TimeToNextShot = Random.Shared.NextSingle() * 0.3f + 0.1f;
                        ref Player player = ref Players.Get(playerEnt);
                        var bEnt = world.NewEntity();
                        ref var bullet = ref Bullets.Add(bEnt);
                        bullet.LifeTime = 10f;
                        bullet.Team = Team.Enemy;
                        bullet.PrevPosition = bullet.Position = transform.Position;
                        var dirToPlayer = (player.Position - transform.Position).Normalized();
                        bullet.Velocity = new Vector2(Random.Shared.NextSingle() * 2 - 1, -3) * 30;
                        bullet.Velocity += transform.Velocity;
                    }
                }
            }
        }
    }
}
