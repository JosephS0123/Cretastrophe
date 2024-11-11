using UnityEngine;

public class ShootProjectiles : MonoBehaviour
{
    public GameObject projectilePrefab;  
    private float fireRate = 2f; // Time between each shot (in seconds)
    private float projectileSpeed = 6f;  

    private float lastFireTime = 0;       
    private int projectileCt = 0;
    private Vector2[] directions;
    
    public void FireProjectiles(Vector2 position, Vector2 lookDirection)
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

            // Instantiate and fire projectiles
            for (int i = 0; i < projectileCt; i++)
            {
                Vector2 spawnPosition = position + new Vector2 (.32f * lookDirection.x, 0);  
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                Projectile projScript = projectile.GetComponent<Projectile>();
                projScript.direction = directions[i] * new Vector2(-lookDirection.x, 1);  
                projScript.speed = projectileSpeed;
            }
        }
    }

    public void setProjectileCount(int numProjectiles, string prefabName)
    {
        projectileCt = numProjectiles;
        directions = new Vector2[projectileCt];
        projectilePrefab = Resources.Load<GameObject>("Prefabs/" + prefabName);

        for (int i = 0; i < projectileCt; i++) {
            directions[i] = new Vector2(transform.right.x, (projectileCt-i) * 2f / projectileCt);
        }
    }
}
