using UnityEngine;
using UnityEngine.UI;

public class chalkSelector_NEW : MonoBehaviour
{
    public RectTransform[] chalkUIs; // Array of chalk UI elements (Base, Fire, Ice)
    public DrawManager drawManager;  
    public Vector2 selectedSize = new Vector2(150, 150);
    public Vector2 deselectedSize = new Vector2(100, 100);
    public float scaleSpeed = 5f; // Speed of the transition

    private int selectedChalkIndex = 0; // Index of the currently selected chalk

    void Start()
    {
        // Initialize all chalks to deselected size except the first one
        for (int i = 0; i < chalkUIs.Length; i++)
        {
            chalkUIs[i].sizeDelta = (i == selectedChalkIndex) ? selectedSize : deselectedSize;
        }


    }

    void Update()
    {
        // Smoothly scale each chalk UI towards its target size
        for (int i = 0; i < chalkUIs.Length; i++)
        {
            Vector2 targetSize = (i == selectedChalkIndex) ? selectedSize : deselectedSize;
            chalkUIs[i].sizeDelta = Vector2.Lerp(chalkUIs[i].sizeDelta, targetSize, Time.deltaTime * scaleSpeed);
        }
    }

    public void OnChalkClick(int chalkIndex)
    {
        // Ignore clicks on already selected chalk
        if (chalkIndex == selectedChalkIndex) return;

        // Update the selected chalk index
        selectedChalkIndex = chalkIndex;
        drawManager.changeColor(selectedChalkIndex);

    }


    public int GetSelectedChalkIndex()
    {
        return selectedChalkIndex; // Provide the current selected chalk index
    }

    public void SetSelectedChalkIndex(int set)
    {
        selectedChalkIndex = set; //Set the chalk, used in pickup script.
        drawManager.changeColor(selectedChalkIndex);
    }
}
