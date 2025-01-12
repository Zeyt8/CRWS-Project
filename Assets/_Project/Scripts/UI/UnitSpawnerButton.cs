using UnityEngine;

public class UnitSpawnerButton : MonoBehaviour
{
    #region Public Methods
    public void SpawnUnit(int unitType)
    {
        PlayerObject.Instance.SetUnit((UnitTypes)unitType);
    }
    #endregion
}
