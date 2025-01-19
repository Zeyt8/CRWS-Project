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
    private Color regionColor;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        _image = GetComponent<Image>();
        _texture = _image.material.GetTexture("_RegionTex") as Texture2D;
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
        regionColor = _texture.GetPixel((int)(mousePos.x * _texture.width), (int)(mousePos.y * _texture.height));

        if (regionColor == Color.red || regionColor == Color.blue || regionColor == Color.green)
        {
            _currentRegionColor = regionColor;
        }


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

        if (_currentRegionColor == Color.green)
        {
            Debug.Log("Level 1");
            SceneManager.LoadScene("Level1");
        }
        else if (_currentRegionColor == Color.red)
        {
            Debug.Log("Level 2");
            SceneManager.LoadScene("Level2");
        }
        else if (_currentRegionColor == Color.blue)
        {
            Debug.Log("Level 3");
            SceneManager.LoadScene("Level3");
        }
    }
    #endregion
}
