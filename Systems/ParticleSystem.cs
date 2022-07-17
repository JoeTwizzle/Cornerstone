using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading.Tasks;
using TGELayerDraw;
using Cornerstone.Helpers;
using Cornerstone.Events;


namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    internal class ParticleSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        float timeAccumulator = 0;

        Vector2[] wind = new Vector2[8 * 8];
        float gravity = 15f;
        float dragLimit = 60f;
        const int particleCount = 2048;
        Vector2[] positions = new Vector2[particleCount];
        Vector2[] prevPositions = new Vector2[particleCount];
        Vector2[] velocites = new Vector2[particleCount];
        Vector2 mousePosPrev;
        Vector2 mousePos;

        public ParticleSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
            for (int i = 0; i < 8; i++)
            {
                float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
                for (int j = 0; j < 8; j++)
                {
                    angle += Random.Shared.NextSingle();
                    wind[i * 8 + j] = new Vector2(MathF.Cos(angle), MathF.Sin(angle)).Normalized();
                }
            }
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = new Vector2(Random.Shared.NextSingle() * 128, Random.Shared.NextSingle() * -256);
            }
        }

        public void Run(float elapsed, int threadId)
        {
            float dt = elapsed;
            timeAccumulator += dt;
            var targetTimestepDuration = 1 / 144f;
            while (timeAccumulator >= targetTimestepDuration)
            {
                Simulate(targetTimestepDuration);
                timeAccumulator -= targetTimestepDuration;
            }
            var layer = game.ActiveLayer;
            for (int i = 0; i < positions.Length; i++)
            {
                var color = new Color4(207, 138, 159, 170);
                Vector2i particle = (Vector2i)positions[i];
                Vector2i prev = (Vector2i)prevPositions[i];

                layer.DrawPixel(particle, color, BlendMode.Add);
                layer.DrawLine(prev, particle, color, BlendMode.Add);
            }
        }

        void Simulate(float dt)
        {
            mousePosPrev = mousePos;
            mousePos = game.CursorPos;
            for (int i = 0; i < positions.Length; i++)
            {
                ref Vector2 velocity = ref velocites[i];
                ref Vector2 particle = ref positions[i];
                velocity += GetWindInSector(particle) * dt * 80f;
                velocity += Vector2.UnitY * gravity * dt;
                if (velocity.LengthSquared > dragLimit * dragLimit)
                {
                    velocity = velocity.Normalized() * dragLimit;
                }
                if (Vector2.DistanceSquared(particle, mousePos) < 5 * 5)
                {
                    velocity += (mousePos - mousePosPrev) * 10f;
                }
                prevPositions[i] = particle;
                particle += velocity * dt;
                if (particle.X < 0)
                {
                    particle.X += 128;
                    prevPositions[i] = particle;
                }
                if (particle.X >= 128)
                {
                    particle.X -= 128;
                    prevPositions[i] = particle;
                }
                if (particle.Y >= 128)
                {
                    velocity.X = 0;
                    velocity.Y = 0;
                    particle.Y = Random.Shared.NextSingle() * -128;
                    prevPositions[i] = particle;
                }
            }
        }

        Vector2 GetWindInSector(in Vector2 pos)
        {
            if (pos.X >= 0 && pos.Y >= 0 && pos.X < 128 && pos.Y < 128)
            {
                return wind[((int)pos.X / 16) + ((int)pos.Y / 16) * 8];
            }
            return Vector2.Zero;
        }
    }
}
