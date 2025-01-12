using UnityEngine;
using TMPro;

public class MoneyView : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();    
    }

    private void OnEnable()
    {
        PlayerObject.OnMoneyChanged += UpdateText;
    }

    private void OnDisable()
    {
        PlayerObject.OnMoneyChanged -= UpdateText;
    }

    private void UpdateText(int currentMoney)
    {
        _text.text = $"{currentMoney.ToString()} gold";
    }
}
