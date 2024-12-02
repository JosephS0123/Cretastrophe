using UnityEngine;
using TMPro; 

public class LevelComplete : MonoBehaviour
{
    public TextMeshProUGUI gemsCollectedText; 
    private int totalGems = 3; // Total gems for the level

    
    public void ShowLevelCompleteScreen()
    {
        int collectedGems = GameManager.diamondCount; 
        //int totalGems = GameManager.maxDiamondCount; // Uncomment if fixed later

        // Update the TextMeshPro component with the collected gems text
        gemsCollectedText.text = $"Gems Collected: {collectedGems}/{totalGems}";
    }
}
