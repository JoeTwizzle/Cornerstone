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

namespace Cornerstone.Systems
{
    [EcsRead("Default", typeof(Player), typeof(Enemy1), typeof(Transform))]
    [EcsWrite("Default", typeof(Enemy), typeof(Bullet))]
    internal class Enemy1BehaviorSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        readonly EcsWorld world;
        readonly EcsPool<Player> Players;
        readonly EcsFilter PlayerFilter;
        readonly EcsPool<Enemy1> TestEnemies;
        readonly EcsPool<Enemy> Enemies;
        readonly EcsPool<Transform> Transforms;
        readonly EcsFilter EnemyFilter;
        readonly EcsPool<Bullet> Bullets;

        float timeAccumulator;


        public Enemy1BehaviorSystem(EcsSystems systems) : base(systems)
        {
            world = GetWorld();
            game = GetSingleton<MyGame>();
            Players = GetPool<Player>();
            TestEnemies = GetPool<Enemy1>();
            Enemies = GetPool<Enemy>();
            Transforms = GetPool<Transform>();
            PlayerFilter = FilterInc<Player>().End();
            EnemyFilter = FilterInc<Transform>().Inc<Enemy1>().Inc<Enemy>().End();
            Bullets = GetPool<Bullet>();
        }

        public void Run(EcsSystems systems, float elapsed, int threadId)
        {
            float dt = game.DeltaTime;
            timeAccumulator += dt;
            var targetTimestepDuration = 1 / 120f;
            while (timeAccumulator >= targetTimestepDuration)
            {
                Simulate(targetTimestepDuration);
                timeAccumulator -= targetTimestepDuration;
            }
        }

        private void Simulate(float dt)
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
