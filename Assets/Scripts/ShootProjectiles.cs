using UnityEngine;

public class ShootProjectiles : MonoBehaviour
{
    public GameObject projectilePrefab;  
    private float fireRate = 2f; // Time between each shot (in seconds)
    private float projectileSpeed = 6f;  

    private float lastFireTime = 0;       
    private int projectileCt = 0;
    private float[] directions;
    
    public void FireProjectiles(Vector2 position, Vector2 lookDirection, Vector2 playerPos)
    {
        // Only fire if enough time has passed
        if (Time.time - lastFireTime >= fireRate)
        {
            lastFireTime = Time.time;

            if (projectilePrefab == null) 
            {
                Debug.LogError("Projectile prefab is missing!");
                return;
            }

            // Fire directly at player position
            if (projectileCt == 1) {
                Vector2 direction = playerPos - (Vector2)transform.position;
                float angleInRadians = Mathf.Atan2(direction.y, direction.x);
                directions[0] = angleInRadians * Mathf.Rad2Deg;

                Vector2 spawnPosition = position + new Vector2 (.1f * lookDirection.x, 0);  
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                Projectile projScript = projectile.GetComponent<Projectile>();
                projScript.direction = directions[0];
                projScript.speed = projectileSpeed * 2;
            } else {
                // Instantiate and fire projectiles
                for (int i = 0; i < projectileCt; i++)
                {
                    Vector2 spawnPosition = position + new Vector2 (.1f * lookDirection.x, 0);  
                    GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                    Projectile projScript = projectile.GetComponent<Projectile>();

                    /* Ensure symmetry and correct launch angles of projectiles */
                    if (lookDirection.x > 0) {
                        projScript.direction = directions[i];
                    } else {
                        projScript.direction = 180f - directions[i];
                    }
                    projScript.speed = projectileSpeed * 2;
                }
            }
        }
    }

    /* Assign the expected projectile spread for 2+ shots */
    public void setProjectileCount(int numProjectiles, string prefabName)
    {
        projectileCt = numProjectiles;
        directions = new float[projectileCt];
        projectilePrefab = Resources.Load<GameObject>("Prefabs/" + prefabName);

        float angleBetweenShots = 90f / (numProjectiles + 2);

        for (int i = 0; i < projectileCt; i++) {
            directions[i] = (i+1) * angleBetweenShots;
        }
    }
}
