using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Collections.Generic;
using Unity.Physics;
using Unity.Collections;
using System.Xml.Linq;

public class PlayerObject : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private int _startingMoney;
    [SerializeField] private List<int> _unitCosts = new List<int>();
    #endregion

    #region Static Variables
    public static PlayerObject Instance { get; private set; }
    public static Action<int> OnMoneyChanged { get; set; } = delegate { };
    #endregion

    #region Variables
    private int _currentMoney;
    private UnitTypes _unitToSpawn = UnitTypes.TwoHandedSword;
    #endregion

    #region Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        _currentMoney = _startingMoney;
    }

    private void OnEnable()
    {
        _inputHandler.OnSelect += OnSelect;
    }

    private void OnDisable()
    {
        _inputHandler.OnSelect -= OnSelect;
    }
    #endregion

    #region Public Methods
    public void SetUnit(UnitTypes unitType)
    {
        _unitToSpawn = unitType;
    }
    #endregion

    #region Private Methods
    private void OnSelect()
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        CollisionWorld collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(_inputHandler.MousePosition);
        RaycastInput input = new RaycastInput()
        {
            Start = ray.origin,
            End = ray.origin + ray.direction * 500,
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << 3
            }
        };

        if (collisionWorld.CastRay(input, out Unity.Physics.RaycastHit h) && _currentMoney >= _unitCosts[(int)_unitToSpawn])
        {
            Vector3 pos = h.Position;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = entityManager.CreateEntityQuery(typeof(UnitSpawner));

            UnitSpawner unitSpawner = entityManager.GetComponentData<UnitSpawner>(h.Entity);

            unitSpawner.UnitToSpawn = _unitToSpawn;
            unitSpawner.SpawnPosition = new float3(pos.x, pos.y, pos.z);
            entityManager.SetComponentData(h.Entity, unitSpawner);

            _currentMoney -= _unitCosts[(int)_unitToSpawn];
            OnMoneyChanged.Invoke(_currentMoney);
        }
    }
    #endregion
}
