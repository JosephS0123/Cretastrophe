using UnityEngine;
using UnityEngine.UI; // Make sure to import UnityEngine.UI for the Legacy Text component

public class LevelComplete : MonoBehaviour
{
    public Text gemsCollectedText;  
    private int totalGems = 3;    

    // This function will be called when the level is completed
    public void ShowLevelCompleteScreen()
    {
       
        int collectedGems = GameManager.diamondCount;
        //int totalGems = GameManager.maxDiamondCount; This doesn't work lol but will fix after demo

        
        gemsCollectedText.text = "Gems Collected: " + collectedGems + "/" + totalGems;
    }
}
