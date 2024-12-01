using UnityEngine;

public class DoorOpenRelockInverse : MonoBehaviour
{
    private SpriteRenderer[] tileRenderers;
    private Collider2D doorCollider;

    public Color activeColor = new Color(0.5f, 0.7f, 1f, 1f); // Fully opaque light blue when closed
    public Color inactiveColor = new Color(0.5f, 0.7f, 1f, 0.2f); // Semi-transparent light blue when open

    private bool isActive = false; // Door starts as open (inactive)

    private void Start()
    {
        tileRenderers = GetComponentsInChildren<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // Ensure the door starts as open (inactive) with transparency
        SetDoorState(isActive);
    }

    public void SetDoorState(bool active)
    {
        isActive = active;

        if (active)
        {
            // Door is closed (active), fully visible
            SetTileColors(activeColor); // Full visibility (opaque)
            if (doorCollider != null)
            {
                doorCollider.enabled = true; // Enable collision (door is closed)
            }
        }
        else
        {
            // Door is open (inactive), semi-transparent
            SetTileColors(inactiveColor); // Semi-transparent (open)
            if (doorCollider != null)
            {
                doorCollider.enabled = false; // Disable collision (door is open)
            }
        }
    }

    private void SetTileColors(Color color)
    {
        if (tileRenderers == null || tileRenderers.Length == 0) return;

        foreach (var renderer in tileRenderers)
        {
            renderer.color = color; // Set the transparency here
        }
    }
}
