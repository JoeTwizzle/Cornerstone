using Cornerstone.Components;

namespace Cornerstone.Systems
{
    [EcsWrite("Default", typeof(Transform))]
    internal class TransformSystem : EcsSystem, IEcsRunSystem
    {
        EcsPool<Transform> Transforms;
        EcsFilter TransformFilter;

        public TransformSystem(EcsSystems systems) : base(systems)
        {
            TransformFilter = FilterInc<Transform>().End();
            Transforms = GetPool<Transform>();
        }

        public void Run(EcsSystems systems, float elapsed, int threadId)
        {
            float dt = elapsed;
            foreach (var entity in TransformFilter)
            {
                ref var t = ref Transforms.Get(entity);
                t.Position += t.Velocity * dt;
            }
        }
    }
}
