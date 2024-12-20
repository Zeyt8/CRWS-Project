using UnityEngine;
using Unity.Entities;

public class UnitGameObjectPrefab : IComponentData
{
    public GameObject Value;
}

public class UnitAnimatorReference : ICleanupComponentData
{
    public Animator Value;
}

public class UnitAnimationAuthoring : MonoBehaviour
{
    public GameObject UnitGameObjectPrefab;

    public class Baker : Baker<UnitAnimationAuthoring>
    {
        public override void Bake(UnitAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new UnitGameObjectPrefab { Value = authoring.UnitGameObjectPrefab });
        }
    }
}
