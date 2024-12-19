using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private float _speed;
    [SerializeField] private float _zoomSpeed;
    #endregion

    #region Variables
    private CinemachinePositionComposer _cinemachinePositionComposer;
    #endregion

    #region Lifecycle

    private void Awake()
    {
        _cinemachinePositionComposer = GetComponentInChildren<CinemachinePositionComposer>();
    }

    private void OnEnable()
    {
        _inputHandler.OnCameraZoom += Zoom;
    }

    private void OnDisable()
    {
        _inputHandler.OnCameraZoom -= Zoom;
    }

    private void Update()
    {
        transform.position += new Vector3(_inputHandler.CameraPan.x, 0, _inputHandler.CameraPan.y) * Time.deltaTime * _speed;
    }
    #endregion

    #region Private Methods
    private void Zoom(float dir)
    {
        _cinemachinePositionComposer.CameraDistance += dir * _zoomSpeed;
    }
    #endregion
}
