using UnityEngine;

[ExecuteInEditMode]
public class VFXPreview : MonoBehaviour
{
    [SerializeField] private float _distance;
    [SerializeField] private float _speed;

    private Vector3 _startPos;

    private void OnEnable()
    {
        _startPos = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = _startPos + Quaternion.Euler(0, _speed * Time.time, 0) * Vector3.forward * _distance;
    }
}
