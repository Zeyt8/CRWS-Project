using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;

public class PlayerObject : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private int _startingMoney;
    #endregion

    #region Static Variables
    public static PlayerObject Instance { get; private set; }
    public static Action<int> OnMoneyChanged { get; set; } = delegate { };
    #endregion

    #region Variables
    private int _currentMoney;
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

    #region Private Methods
    private void OnSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(_inputHandler.MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, LayerMask.NameToLayer("WorldMap")))
        {
            MeshCollider mc = hit.collider as MeshCollider;
            mc.GetComponent<MapObject>().SelectRegion(hit.textureCoord);
        }

        if (Physics.Raycast(ray, out hit, 500, LayerMask.NameToLayer("SpawnArea")) && _currentMoney > 0)
        {
            Vector3 pos = hit.point;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = entityManager.CreateEntityQuery(typeof(UnitSpawner));

            Entity unitSpawnerEntity = query.GetSingletonEntity();
            UnitSpawner unitSpawner = entityManager.GetComponentData<UnitSpawner>(unitSpawnerEntity);

            unitSpawner.SpawnPosition = new float3(pos.x, -0.5f, pos.z);
            entityManager.SetComponentData(unitSpawnerEntity, unitSpawner);

            _currentMoney -= 10;
            OnMoneyChanged.Invoke(_currentMoney);
        }
    }
    #endregion
}
