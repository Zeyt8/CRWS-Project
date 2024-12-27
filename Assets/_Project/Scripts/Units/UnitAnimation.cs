using Latios.Mimic.Addons.Mecanim;
using Unity.Burst;
using Unity.Entities;

partial struct UnitAnimation : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((MecanimAspect anim, RefRO<MovementData> mov) in SystemAPI.Query<MecanimAspect, RefRO<MovementData>>())
        {
            anim.SetBool("IsMoving", mov.ValueRO.IsMoving);
            anim.SetFloat("Velocity", mov.ValueRO.CurrentVelocity);
        }
    }
}
