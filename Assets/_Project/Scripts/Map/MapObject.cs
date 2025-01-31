using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapObject : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Button _startButton;
    [SerializeField] LevelsFinished _levelFinished;
    #endregion

    #region Variables
    private Image _image;
    private Texture2D _texture;
    private Color _currentRegionColor;
    private Color regionColor;
    private int levelIndex;
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
        Time.timeScale = 1.0f;
        Vector2 mousePos = _inputHandler.MousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;
        regionColor = _texture.GetPixel((int)(mousePos.x * _texture.width), (int)(mousePos.y * _texture.height));

        if (regionColor == Color.red || regionColor == Color.blue || regionColor == Color.green)
        {
            _currentRegionColor = regionColor;
        }



        if (regionColor == Color.red)
            levelIndex = 1;
        else if (regionColor == Color.green)
            levelIndex = 2;
        else if (regionColor == Color.blue)
            levelIndex = 3;
        else if (regionColor == Color.cyan)
            levelIndex = 4;
        else if (regionColor == Color.gray)
            levelIndex = 5;
        else if (regionColor == Color.magenta)
            levelIndex = 6;
        else if (regionColor == Color.black)
            levelIndex = 7;



        if (_levelFinished.IsLevelCompleted(levelIndex))
        {
            //Add pop up to say level is done;
            Debug.Log("LEvel complete");
            _startButton.interactable = false;

        }
        else
        {
            _startButton.interactable = true;
            _image.material.SetVector("_SelectPosition", mousePos);
        }

        
    }

    public void StartGame()
    {
        if (_currentRegionColor == Color.red)
        {
            SceneManager.LoadScene("Level1");
        }
        else if (_currentRegionColor == Color.green)
        {
            SceneManager.LoadScene("Level2");
        }
        else if (_currentRegionColor == Color.blue)
        {
            SceneManager.LoadScene("Level3");
        }
        else if (_currentRegionColor == Color.cyan)
        {
            SceneManager.LoadScene("Level4");
        }
        else if (_currentRegionColor == Color.gray)
        {
            SceneManager.LoadScene("Level5");
        }
        else if (_currentRegionColor == Color.magenta)
        {
            SceneManager.LoadScene("Level6");
        }
        else if (_currentRegionColor == Color.black)
        {
            SceneManager.LoadScene("Level7");
        }
    }
    #endregion
}
