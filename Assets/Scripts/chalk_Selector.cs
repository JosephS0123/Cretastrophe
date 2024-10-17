using UnityEngine;
using UnityEngine.UI;

public class chalk_Selector : MonoBehaviour
{
    public RectTransform chalkUI;
    public GameObject drawManager;
    public Vector2 selectedSize = new Vector2(150, 150);
    public Vector2 deselectedSize = new Vector2(100, 100);
    public float scaleSpeed = 5f; // Speed of the transition

    private bool isSelected = true;
    private Vector2 targetSize;

    void Start()
    {
        // Initialize the chalk with the selected size
        targetSize = selectedSize;
        chalkUI.sizeDelta = targetSize;
    }

    void Update()
    {
        // Smoothly scale the chalk towards the target size
        chalkUI.sizeDelta = Vector2.Lerp(chalkUI.sizeDelta, targetSize, Time.deltaTime * scaleSpeed);
    }

    public void OnChalkClick()
    {
        // Toggle the selection state
        isSelected = !isSelected;

        if(!isSelected)
        {
            drawManager.SetActive(false); //Disables drawing
        }

        if(isSelected)
        {
            drawManager.SetActive(true); //Enables drawing
        }
        
        // Set the target size based on the new state
        targetSize = isSelected ? selectedSize : deselectedSize;
    }
}
