
using ALAudio;
using TGELayerDraw;
using Cornerstone.Helpers;
using Cornerstone.Events;
using Cornerstone.UI;
using Cornerstone.Components;
using OpenTK.Mathematics;

namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    [EcsWrite("Default",typeof(Transform), typeof(Explosion), typeof(Player))]
    internal class ExplosionSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        readonly EcsWorld world;
        readonly EcsPool<Transform> Transforms;
        readonly EcsPool<Explosion> Explosions;
        readonly EcsPool<Player> Players;
        readonly EcsFilter ExplosionFilter;
        readonly EcsFilter PlayerFilter;

        float timeAccumulator;
        readonly AudioSource[] explosionSources = new AudioSource[2];//2 simultaneous sounds
        readonly AudioBuffer explosionBuffer;
        int explosionIndex = 0;

        public ExplosionSystem(EcsSystems systems) : base(systems)
        {
            Explosions = GetPool<Explosion>();
            Players = GetPool<Player>();
            PlayerFilter = FilterInc<Player>().End();
            ExplosionFilter = FilterInc<Transform>().Inc<Explosion>().End();
            game = GetSingleton<MyGame>();
            world = GetWorld();
            Transforms = GetPool<Transform>();

            explosionBuffer = new AudioBuffer();
            explosionBuffer.Init("SFX/Bomb.wav");
            for (int i = 0; i < explosionSources.Length; i++)
            {
                explosionSources[i] = new AudioSource();
                explosionSources[i].SetBuffer(explosionBuffer);
                explosionSources[i].SetVolume(0.15f);
            }
        }

        void PlaySound(float percentage)
        {
            explosionSources[explosionIndex].Play();
            explosionIndex++;
            explosionIndex %= explosionSources.Length;
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
