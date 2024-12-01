using System.Collections.Generic;
using UnityEngine;

public class DoorOpenRelock : MonoBehaviour
{
    private HashSet<int> activeButtons = new HashSet<int>();
    private SpriteRenderer[] tileRenderers;
    private Collider2D doorCollider;

    public Color activeColor = new Color(0.5f, 0.7f, 1f, 0.5f); 
    public Color inactiveColor = new Color(0.5f, 0.7f, 1f, 1f);

    private void Start()
    {
        // Get all SpriteRenderers in this door's hierarchy
        tileRenderers = GetComponentsInChildren<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // Initialize all tiles to the inactive color
        SetTileColors(inactiveColor);
    }

    public void ButtonPressed(int buttonIndex)
    {
        activeButtons.Add(buttonIndex);
        UpdateDoorState();
    }

    public void ButtonReleased(int buttonIndex)
    {
        activeButtons.Remove(buttonIndex);
        UpdateDoorState();
    }

    private void UpdateDoorState()
    {
        if (activeButtons.Count > 0)
        {
            // Door is "active" (open)
            SetTileColors(activeColor);
            if (doorCollider != null)
            {
                doorCollider.enabled = false; // Disable collision
            }
        }
        else
        {
            // Door is "inactive" (closed)
            SetTileColors(inactiveColor);
            if (doorCollider != null)
            {
                doorCollider.enabled = true; // Enable collision
            }
        }
    }

    private void SetTileColors(Color color)
    {
        if (tileRenderers == null || tileRenderers.Length == 0) return;

        foreach (var renderer in tileRenderers)
        {
            renderer.color = color;
        }
    }
}
