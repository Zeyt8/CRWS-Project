using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapObject : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Button _startButton;
    #endregion

    #region Variables
    private Image _image;
    private Texture2D _texture;
    private Color _currentRegionColor;
    private Dictionary<Color, bool> _regionsDefeated = new Dictionary<Color, bool>();
    #endregion

    #region Lifecycle
    private void Awake()
    {
        _image = GetComponent<Image>();
        _texture = _image.material.GetTexture("_MainTex") as Texture2D;
    }

    private void OnEnable()
    {
        _inputHandler.OnSelect += SelectRegion;
    }

    private void OnDisable()
    {
        _inputHandler.OnSelect -= SelectRegion;
    }
    #endregion

    #region Public Methods
    public void SelectRegion()
    {
        Vector2 mousePos = _inputHandler.MousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;
        _currentRegionColor = _texture.GetPixel((int)(mousePos.x * _texture.width), (int)(mousePos.y * _texture.height));
        if (!_regionsDefeated.ContainsKey(_currentRegionColor))
        {
            _regionsDefeated.Add(_currentRegionColor, false);
        }
        _startButton.interactable = !_regionsDefeated[_currentRegionColor];
        if (_regionsDefeated[_currentRegionColor])
        {
            return;
        }
        _image.material.SetVector("_SelectPosition", mousePos);
    }

    public void StartGame()
    {
        if (_currentRegionColor == Color.red)
        {
            SceneManager.LoadScene("Level1");
        }
        else if (_currentRegionColor == Color.blue)
        {
            SceneManager.LoadScene("Level2");
        }
        else if (_currentRegionColor == Color.green)
        {
            SceneManager.LoadScene("Level3");
        }
        else if (_currentRegionColor == Color.cyan)
        {
            SceneManager.LoadScene("Level4");
        }
    }
    #endregion
}
