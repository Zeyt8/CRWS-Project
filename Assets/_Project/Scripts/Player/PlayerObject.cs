using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Collections.Generic;

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
        Ray ray = Camera.main.ScreenPointToRay(_inputHandler.MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, LayerMask.GetMask("WorldMap")))
        {
            MeshCollider mc = hit.collider as MeshCollider;
            mc.GetComponent<MapObject>().SelectRegion(hit.textureCoord);
        }

        if (Physics.Raycast(ray, out hit, 500, LayerMask.GetMask("SpawnArea")) && _currentMoney >= _unitCosts[(int)_unitToSpawn])
        {
            Vector3 pos = hit.point;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = entityManager.CreateEntityQuery(typeof(UnitSpawner));

            Entity unitSpawnerEntity = query.GetSingletonEntity();
            UnitSpawner unitSpawner = entityManager.GetComponentData<UnitSpawner>(unitSpawnerEntity);

            unitSpawner.UnitToSpawn = _unitToSpawn;
            unitSpawner.SpawnPosition = new float3(pos.x, -0.5f, pos.z);
            entityManager.SetComponentData(unitSpawnerEntity, unitSpawner);

            _currentMoney -= _unitCosts[(int)_unitToSpawn];
            OnMoneyChanged.Invoke(_currentMoney);
        }
    }
    #endregion
}
