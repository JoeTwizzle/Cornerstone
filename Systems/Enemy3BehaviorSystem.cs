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
    [EcsRead("Default", typeof(Player), typeof(Enemy3), typeof(Transform))]
    [EcsWrite("Default", typeof(Enemy), typeof(Bullet))]
    internal class Enemy3BehaviorSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        readonly EcsWorld world;
        readonly EcsPool<Player> Players;
        readonly EcsFilter PlayerFilter;
        readonly EcsPool<Enemy3> Enemies3;
        readonly EcsPool<Enemy> Enemies;
        readonly EcsFilter TestEnemyFilter;
        readonly EcsPool<Transform> Transforms;
        readonly EcsPool<Bullet> Bullets;

        float timeAccumulator;

        public Enemy3BehaviorSystem(EcsSystems systems) : base(systems)
        {
            Players = GetPool<Player>();
            PlayerFilter = FilterInc<Player>().End();
            game = GetSingleton<MyGame>();
            world = GetWorld();
            Enemies3 = GetPool<Enemy3>();
            Enemies = GetPool<Enemy>();
            TestEnemyFilter = FilterInc<Transform>().Inc<Enemy3>().Inc<Enemy>().End();
            Transforms = GetPool<Transform>();
            Bullets = GetPool<Bullet>();
        }


        public void Run(float elapsed, int threadId)
        {
            float dt = game.DeltaTime;
            timeAccumulator += dt;
            var targetTimestepDuration = 1 / 120f;
            while (timeAccumulator >= targetTimestepDuration)
            {
                Simulate(targetTimestepDuration, systems);
                timeAccumulator -= targetTimestepDuration;
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
