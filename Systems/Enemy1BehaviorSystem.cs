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
    internal class Enemy1BehaviorSystem : IEcsRunSystem, IEcsInitSystem
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
        EcsPool<Enemy1> TestEnemies = null!;

        [EcsPool]
        EcsPool<Enemy> Enemies = null!;

        [EcsPool]
        EcsPool<SpriteAnimation> SpriteAnimations = null!;

        [EcsPool]
        EcsPool<Transform> Transforms = null!;

        [EcsFilter(typeof(Enemy1), typeof(Transform), typeof(Enemy))]
        EcsFilter EnemyFilter = null!;

        [EcsPool]
        EcsPool<Bullet> Bullets = null!;

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
            foreach (var entity in EnemyFilter)
            {
                ref var transform = ref Transforms.Get(entity);
                ref var baseEnemy = ref Enemies.Get(entity);
                ref var testEnemy = ref TestEnemies.Get(entity);
                baseEnemy.TimeToNextShot -= dt;
                if (baseEnemy.TimeToNextShot <= 0)
                {
                    foreach (var playerEnt in PlayerFilter)
                    {
                        baseEnemy.TimeToNextShot = Random.Shared.NextSingle() * 0.3f + 0.3f;
                        ref Player player = ref Players.Get(playerEnt);
                        var bEnt = world.NewEntity();
                        ref var bullet = ref Bullets.Add(bEnt);
                        bullet.LifeTime = 10f;
                        bullet.Team = Team.Enemy;
                        bullet.PrevPosition = bullet.Position = transform.Position;
                        bullet.Velocity = (player.Position - transform.Position).Normalized() * 70;
                        bullet.Velocity += transform.Velocity * 0.3f;
                    }
                }
            }
        }
    }
}
