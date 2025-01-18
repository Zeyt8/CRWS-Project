using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct ProjectileSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach ((RefRW<ProjectileInstanceData> projectile, RefRW<LocalTransform> transform, Entity entity) in SystemAPI.Query<RefRW<ProjectileInstanceData>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            float3 forward = transform.ValueRO.Forward();
            transform.ValueRW.Position += forward * projectile.ValueRO.Speed * SystemAPI.Time.DeltaTime;
            projectile.ValueRW.Lifetime -= SystemAPI.Time.DeltaTime;
            if (projectile.ValueRO.Lifetime <= 0)
            {
                ecb.DestroyEntity(entity);
            }
        }

        state.Dependency = new CollisionJob
        {
            ProjectileInstanceLookup = SystemAPI.GetComponentLookup<ProjectileInstanceData>(true),
            HealthLookup = SystemAPI.GetComponentLookup<HealthData>(),
            CommandBuffer = ecb.AsParallelWriter()
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private struct CollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<ProjectileInstanceData> ProjectileInstanceLookup;
        [ReadOnly] public ComponentLookup<HealthData> HealthLookup;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(CollisionEvent collisionEvent)
        {
            if (ProjectileInstanceLookup.TryGetComponent(collisionEvent.EntityA, out ProjectileInstanceData projectile))
            {
                if (HealthLookup.HasComponent(collisionEvent.EntityB))
                {
                    CommandBuffer.AddComponent(collisionEvent.BodyIndexB, collisionEvent.EntityB, new DamageInstanceData
                    {
                        Value = projectile.Damage,
                        Type = projectile.DamageType
                    });
                }
                CommandBuffer.DestroyEntity(0, collisionEvent.EntityA);
            }
            if (ProjectileInstanceLookup.TryGetComponent(collisionEvent.EntityB, out projectile))
            {
                if (HealthLookup.HasComponent(collisionEvent.EntityA))
                {
                    CommandBuffer.AddComponent(collisionEvent.BodyIndexA, collisionEvent.EntityA, new DamageInstanceData
                    {
                        Value = projectile.Damage,
                        Type = projectile.DamageType
                    });
                }
                CommandBuffer.DestroyEntity(0, collisionEvent.EntityB);
            }
        }
    }
}
