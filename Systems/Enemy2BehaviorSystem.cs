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
    [EcsRead("Default", typeof(Enemy2), typeof(Transform))]
    [EcsWrite("Default", typeof(Enemy), typeof(Bullet))]
    internal class Enemy2BehaviorSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        readonly EcsWorld world;
        readonly EcsPool<Enemy2> TestEnemies;
        readonly EcsPool<Enemy> Enemies;
        readonly EcsFilter TestEnemyFilter;
        readonly EcsPool<Transform> Transforms;
        readonly EcsPool<Bullet> Bullets;

        float timeAccumulator;
        readonly AudioSource[] explosionSources = new AudioSource[10];//10 simultaneous sounds
        AudioBuffer explosionBuffer;
        int explosionIndex = 0;

        public Enemy2BehaviorSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
            world= GetWorld();
            TestEnemies = GetPool<Enemy2>();
            Enemies = GetPool<Enemy>();
            TestEnemyFilter = FilterInc<Transform>().Inc<Enemy2>().Inc<Enemy>().End();
            Transforms = GetPool<Transform>();
            Bullets = GetPool<Bullet>();

            explosionBuffer = new AudioBuffer();
            explosionBuffer.Init("SFX/BombDrop.wav");
            for (int i = 0; i < explosionSources.Length; i++)
            {
                explosionSources[i] = new AudioSource();
                explosionSources[i].SetBuffer(explosionBuffer);
                explosionSources[i].SetVolume(0.15f);
            }
        }

        void PlaySound()
        {
            explosionSources[explosionIndex].Play();
            explosionIndex++;
            explosionIndex %= explosionSources.Length;
        }
        public void Run(EcsSystems systems, float elapsed, int threadId)
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
                ref var testEnemy = ref TestEnemies.Get(entity);
                baseEnemy.TimeToNextShot -= dt;
                if (baseEnemy.TimeToNextShot <= 0)
                {
                    PlaySound();
                    baseEnemy.TimeToNextShot = Random.Shared.NextSingle() * 1.3f + 1.3f;
                    var bEnt = world.NewEntity();
                    ref var bullet = ref Bullets.Add(bEnt);
                    bullet.LifeTime = 10f;
                    bullet.BulletType = BulletType.Explosive;
                    bullet.Team = Team.Enemy;
                    bullet.PrevPosition = bullet.Position = transform.Position;
                    bullet.Velocity = transform.Velocity;
                }
            }
        }
    }
}
