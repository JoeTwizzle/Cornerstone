using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using ALAudio;
using TGELayerDraw;
using Cornerstone.Helpers;
using Cornerstone.Events;
using Cornerstone.UI;
using Cornerstone.Components;
using OpenTK.Mathematics;

namespace Cornerstone.Systems
{
    internal class ExplosionSystem : IEcsRunSystem, IEcsInitSystem
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
        EcsPool<Transform> Transforms = null!;

        [EcsPool]
        EcsPool<Explosion> Explosions = null!;

        [EcsPool]
        EcsPool<Player> Players = null!;

        [EcsFilter(typeof(Transform), typeof(Explosion))]
        EcsFilter ExplosionFilter = null!;

        [EcsFilter(typeof(Player))]
        EcsFilter PlayerFilter = null!;

        bool enabled = false;
        float timeAccumulator;
        AudioSource[] explosionSources = new AudioSource[2];//2 simultaneous sounds
        AudioBuffer explosionBuffer = null!;
        int explosionIndex = 0;
        void PlaySound(float percentage)
        {

            explosionSources[explosionIndex].Play();
            explosionIndex++;
            explosionIndex %= explosionSources.Length;
        }
        public void Init(EcsSystems systems)
        {
            explosionBuffer = new AudioBuffer();
            explosionBuffer.Init("SFX/Bomb.wav");
            for (int i = 0; i < explosionSources.Length; i++)
            {
                explosionSources[i] = new AudioSource();
                explosionSources[i].SetBuffer(explosionBuffer);
                explosionSources[i].SetVolume(0.15f);
            }
        }

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
            foreach (var entity in ExplosionFilter)
            {
                ref var exp = ref Explosions.Get(entity);
                ref var transform = ref Transforms.Get(entity);
                float percentage = 1.0f - MathF.Abs((exp.Time / exp.Duration) * 2.0f - 1f);
                layer.FillCircle((Vector2i)transform.Position, (int)(percentage * exp.Size), Color4.Red);
            }
        }

        private void Simulate(float dt, EcsSystems systems)
        {
            foreach (var entity in ExplosionFilter)
            {
                ref var exp = ref Explosions.Get(entity);
                if (exp.Time == 0)
                {
                    float percentage = 1.0f - MathF.Abs((exp.Time / exp.Duration) * 2.0f - 1f);
                    PlaySound(percentage);
                }
                exp.Time += dt;
                ref var transform = ref Transforms.Get(entity);
                float radius = ((exp.Time / exp.Duration) - 0.5f) * 2.0f * exp.Size;
                foreach (var playerEnt in PlayerFilter)
                {
                    ref var player = ref Players.Get(playerEnt);
                    if (player.InvincibleTimer <= 0 && Vector2.DistanceSquared(player.Position, transform.Position) <= radius * radius)
                    {
                        player.HP--;
                        player.InvincibleTimer = 0.5f;
                    }
                }
                if (exp.Time >= exp.Duration)
                {
                    world.DelEntity(entity);
                }
            }
        }
    }
}
