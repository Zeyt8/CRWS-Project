using TMPro;
using UnityEngine;

public class UnitCountView : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LevelManager.UnitCountUpdated += UpdateText;
    }

    private void OnDisable()
    {
        LevelManager.UnitCountUpdated -= UpdateText;
    }

    private void UpdateText(int alliedUnits, int enemyUnits)
    {
        _text.text = $"Own Units: {alliedUnits.ToString()}\nEnemyUnits: {enemyUnits.ToString()}";
    }
}
