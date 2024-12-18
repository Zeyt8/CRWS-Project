using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
partial struct UnitAnimateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach ((UnitGameObjectPrefab playerGameObjectPrefab, Entity entity) in
                     SystemAPI.Query<UnitGameObjectPrefab>().WithNone<UnitAnimatorReference>().WithEntityAccess())
        {
            GameObject newCompanionGameObject = Object.Instantiate(playerGameObjectPrefab.Value);
            UnitAnimatorReference newAnimatorReference = new UnitAnimatorReference
            {
                Value = newCompanionGameObject.GetComponent<Animator>()
            };
            ecb.AddComponent(entity, newAnimatorReference);
        }

        foreach ((LocalTransform transform, UnitAnimatorReference animatorReference, Movement mov) in
                 SystemAPI.Query<LocalTransform, UnitAnimatorReference, Movement>())
        {
            animatorReference.Value.transform.position = transform.Position;
            animatorReference.Value.transform.rotation = transform.Rotation;
            animatorReference.Value.SetBool("IsMoving", mov.IsMoving);
            animatorReference.Value.SetFloat("Velocity", mov.Velocity);
        }

        foreach ((UnitAnimatorReference animatorReference, Entity entity) in
                 SystemAPI.Query<UnitAnimatorReference>().WithNone<UnitGameObjectPrefab, LocalTransform>().WithEntityAccess())
        {
            Object.Destroy(animatorReference.Value.gameObject);
            ecb.RemoveComponent<UnitAnimatorReference>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
