using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapObject : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private Button _startButton;
    #endregion

    #region Variables
    private MeshRenderer _meshRenderer;
    private Texture2D _texture;
    private Color _currentRegionColor;
    private Dictionary<Color, bool> _regionsDefeated = new Dictionary<Color, bool>();
    #endregion

    #region Lifecycle
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _texture = _meshRenderer.material.GetTexture("_MainTex") as Texture2D;
    }
    #endregion

    #region Public Methods
    public void SelectRegion(Vector2 region)
    {
        _currentRegionColor = _texture.GetPixel((int)(region.x * _texture.width), (int)(region.y * _texture.height));
        if (!_regionsDefeated.ContainsKey(_currentRegionColor))
        {
            _regionsDefeated.Add(_currentRegionColor, false);
        }
        _startButton.interactable = !_regionsDefeated[_currentRegionColor];
        if (_regionsDefeated[_currentRegionColor])
        {
            return;
        }
        _meshRenderer.material.SetVector("_SelectPosition", region);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level1");
    }
    #endregion
}
