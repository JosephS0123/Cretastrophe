using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static int diamondCount = 0;
    public static int keyCount = 0;


    public static void AddDiamond()
    {
        diamondCount++;  
        Debug.Log("Diamonds Collected: " + diamondCount);
    }

    public static void addKey()
    {
        keyCount++;
        Debug.Log("Keys Collected: " + keyCount);
    }

    public static void removeKey()
    {
        keyCount--;
        Debug.Log("Keys Remaining: " + keyCount);
    }
}
