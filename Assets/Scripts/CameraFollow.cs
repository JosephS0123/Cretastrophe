using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Reference to the player's transform
    public float smoothSpeed = 0.125f;  // Smoothing speed for the camera movement
    public Vector2 minBoundary;  // Minimum boundary for camera position
    public Vector2 maxBoundary;  // Maximum boundary for camera position

    void LateUpdate()
    {
        if (player != null)
        {
            // Define the target position based on the player’s position
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);

            // Smoothly move the camera towards the target position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

            // Clamp the camera's position within the specified boundaries
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBoundary.x, maxBoundary.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBoundary.y, maxBoundary.y);

            // Set the camera position
            transform.position = smoothedPosition;
        }
    }
}
