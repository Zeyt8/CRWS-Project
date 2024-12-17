using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private InputHandler _inputHandler;
    #endregion

    #region Lifecycle

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

        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            MeshCollider mc = hit.collider as MeshCollider;
            mc.GetComponent<MapObject>().SelectRegion(hit.textureCoord);
        }
    }
    #endregion
}
