using Cornerstone.Components;

namespace Cornerstone.Systems
{
    [EcsWrite("Default")]
    internal class LifetimeSystem : EcsSystem, IEcsRunSystem
    {
        readonly EcsWorld world;
        readonly EcsPool<Lifetime> Lifetimes;
        readonly EcsFilter LifetimeFilter;

        public LifetimeSystem(EcsSystems systems) : base(systems)
        {
            world = GetWorld();
            Lifetimes = GetPool<Lifetime>();
            LifetimeFilter = FilterInc<Lifetime>().End();
        }

        public void Run(float elapsed, int threadId)
        {
            float dt = elapsed;
            foreach (var entity in LifetimeFilter)
            {
                ref var lifetime = ref Lifetimes.Get(entity);
                if (lifetime.Time <= 0)
                {
                    world.DelEntity(entity);
                }
                lifetime.Time -= dt;
            }
        }
    }
}
