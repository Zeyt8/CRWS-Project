using UnityEngine;
using UnityEngine.UI;  // Needed for UI components like Text

public class CanvasTextUpdater : MonoBehaviour
{
    public Text castleHealthText;  // Reference to the Text component
    public Text CurrentGoldText; 

    public Text RecruitUnit1CostText;
    public Text RecruitUnit2CostText; 
    public Text RecruitUnit3CostText;  

    public int CurrentGoldValue = 100;
    public int Unit1RecruitCost = 3;

    void Start()
    {
        // If no reference is set in the Inspector, find it on this GameObject
        if (castleHealthText == null)
        {
            castleHealthText = GetComponent<Text>();
        }

        // Change the text when the script starts
        castleHealthText.text = "Castle Health" + " ";

        CurrentGoldText.text = "Current Gold: " + CurrentGoldValue.ToString();
    }

    public void UpdateText(string newText)
    {
        // Method to update the text dynamically
        castleHealthText.text = newText;
    }

    public void RecruitUnit1()
    {
        UpdateCurrentGold(Unit1RecruitCost);
        // instantiate unit
    }


    public void UpdateCurrentGold(int cost)
    {
        // Check if the player has enough gold to spend
        if (CurrentGoldValue >= cost)
        {
            // Subtract the cost from CurrentGoldValue
            CurrentGoldValue -= cost;

            // Update the displayed gold text
            CurrentGoldText.text = "Current Gold: " + CurrentGoldValue.ToString();
            // add follow up to instantiate unit
            Debug.Log("Enough gold to recruit this unit!");
        }
        else
        {
            // Not enough gold, display a message or handle accordingly
            Debug.Log("Not enough gold to recruit this unit!");
        }
    }
}