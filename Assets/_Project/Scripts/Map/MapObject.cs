using UnityEngine;

public class MapObject : MonoBehaviour
{
    #region Variables
    private MeshRenderer _meshRenderer;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    #endregion

    #region Public Methods
    public void SelectRegion(Vector2 region)
    {
        _meshRenderer.material.SetVector("_SelectPosition", region);
    }
    #endregion
}
