using UnityEngine;
using UnityEngine.UI; // Make sure to import UnityEngine.UI for the Legacy Text component

public class LevelComplete : MonoBehaviour
{
    public Text gemsCollectedText;  
    private int totalGems;    

    // This function will be called when the level is completed
    public void ShowLevelCompleteScreen()
    {
       
        int collectedGems = GameManager.diamondCount;
        int totalGems = GameManager.maxDiamondCount;

        
        gemsCollectedText.text = "Gems Collected: " + collectedGems + "/" + totalGems;
    }
}
