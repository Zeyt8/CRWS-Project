using UnityEngine;
using UnityEngine.UI;
public class SpeedUp : MonoBehaviour
{

    public Button speedUp;
    public Button slowDown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 2f)
        {
            speedUp.interactable = false;
            slowDown.interactable = true;
        }
        if(Time.timeScale == 1f)
        {
            speedUp.interactable = true;
            slowDown.interactable = false;
        }
    }


    public void SpeedUpGamePlay()
    {
        Time.timeScale = 2f;
    }

    public void SlowDownGamePlay()
    {
        Time.timeScale = 1f;
    }



}
