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
    internal class Enemy2BehaviorSystem : IEcsRunSystem, IEcsInitSystem
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
        EcsPool<SpriteAnimation> SpriteAnimations = null!;

        [EcsPool]
        EcsPool<Enemy2> TestEnemies = null!;

        [EcsPool]
        EcsPool<Enemy> Enemies = null!;

        [EcsFilter(typeof(Transform), typeof(Enemy2), typeof(Enemy))]
        EcsFilter TestEnemyFilter = null!;

        [EcsPool]
        EcsPool<Transform> Transforms = null!;

        [EcsPool]
        EcsPool<Bullet> Bullets = null!;

        [EcsFilter(typeof(Bullet))]
        EcsFilter BulletFilter = null!;

        bool active = false;
        float timeAccumulator;

        Sprite sprite = null!;

        public void Init(EcsSystems systems)
        {
            sprite = new Sprite("SpriteSheets/Enemy-2.png");
            explosionBuffer = new AudioBuffer();
            explosionBuffer.Init("SFX/BombDrop.wav");
            for (int i = 0; i < explosionSources.Length; i++)
            {
                explosionSources[i] = new AudioSource();
                explosionSources[i].SetBuffer(explosionBuffer);
                explosionSources[i].SetVolume(0.15f);
            }
        }
        AudioSource[] explosionSources = new AudioSource[10];//10 simultaneous sounds
        AudioBuffer explosionBuffer = null!;
        int explosionIndex = 0;
        void PlaySound()
        {
            explosionSources[explosionIndex].Play();
            explosionIndex++;
            explosionIndex %= explosionSources.Length;
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
