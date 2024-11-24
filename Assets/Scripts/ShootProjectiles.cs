using System.Buffers.Text;
using UnityEditor;
using UnityEngine;

public class ShootProjectiles : MonoBehaviour
{
    public GameObject projectilePrefab;  

    public enum shootingFrequency { accelerate, windup, constant, decelerate};
    public enum shootingType { single, widespread, narrowSpread, semicircleSpread };
    public enum shootingDensity {constant, decreasing, increasing}

    private shootingFrequency sFreq;
    private shootingType sType;
    private shootingDensity sDensity;

    private Projectile.projectileType projectileType; // default is spike
    private Projectile.projectileAttribute projectileAttribute; /* TODO: */
    private bool randomSpread; // false by default
    private float fireRate = 2f; // Time between each shot (in seconds)
    private float projectileSpeed = 12f; // 6f is like the min a shot should travel if subjected to gravity

    private float lastFireTime = 0;       
    private int projectileCt = 0;
    public int maxProjectiles = 13;
    private int minProjectiles = 1;
    private float[] directions;

    private float fireRateDelta = 1f; // how much to increase/decrease shoot speed per shot
    public float maxFireRate = .4f; // cant shoot faster than 1 shot every .4 sec 
    public float minFireRate = 5f;
    private bool isIncreasingFirerate;

    private float getVolleyAngle()
    {
        if (sType == shootingType.narrowSpread) {
            return 40f;
        } else if (sType == shootingType.widespread) {
            return 65f;
        }else if (sType == shootingType.semicircleSpread) {
            return 140f;
        }
        return 0f;
    }


    private void assignLaunchAngles()
    {
        if (!randomSpread) 
        {
            float angleBetweenShots = getVolleyAngle() / projectileCt;
            float angleStartPoint = sType == shootingType.semicircleSpread ? 20f : (sType == shootingType.widespread) ? 12.5f : 25f;

            for (int i = 0; i < projectileCt; i++) {
                directions[i] = angleStartPoint + i * angleBetweenShots;
            }
        } else {
            /* assign random spread */
        }
    }

    /* 
        do it all
    */

    public void fireVolley(Vector2 enemyPosition, Vector2 enemyLookDir, Vector2 playerPos)
    {
        // projectileCt == 1
        if (sType == shootingType.single || projectileCt == 1 ) {
            Vector2 direction = playerPos - (Vector2)transform.position;
            float angleInRadians = Mathf.Atan2(direction.y, direction.x);
            directions[0] = angleInRadians * Mathf.Rad2Deg;

            /* for now spawn projectile inside enemy */
            // Vector2 spawnPosition = enemyPosition + new Vector2 (enemyLookDir.x * .1f, 0);  
            Vector2 spawnPosition = enemyPosition;  
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            projScript.direction = directions[0];
            projScript.speed = projectileSpeed * 2;
        
        } else if (!randomSpread) {
            assignLaunchAngles();

            for (int i = 0; i < projectileCt; i++)
            {
                // Vector2 spawnPosition = enemyPosition + new Vector2 (enemyLookDir.x * .1f, 0);  
                Vector2 spawnPosition = enemyPosition;  
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                Projectile projScript = projectile.GetComponent<Projectile>();

                /* Ensure symmetry and correct launch angles of projectiles */
                if (enemyLookDir.x > 0) {
                    projScript.direction = directions[i];
                } else {
                    projScript.direction = 180f - directions[i];
                }
                projScript.speed = projectileSpeed * 2;
            }
        } else {
            /* maybe implement random spread here */
            return;
        }

        if (sDensity == shootingDensity.constant) {
            return;
        } else if (sDensity == shootingDensity.decreasing && (projectileCt > minProjectiles)) {
            projectileCt--;
        } else if (sDensity == shootingDensity.increasing && (projectileCt < maxProjectiles)) {
            projectileCt++;
        }


    }


    private void tryUpdateFirerate()
    {
        if (sFreq == shootingFrequency.accelerate && (fireRate - fireRateDelta >= maxFireRate)) {
            fireRate -= fireRateDelta;
        } else if (sFreq == shootingFrequency.decelerate && (fireRate + fireRateDelta < minFireRate)) {
            fireRate += fireRateDelta;
        }
    }

    public void FireProjectiles(Vector2 enemyPosition, Vector2 enemyLookDir, Vector2 playerPos)
    {
        // Only fire if enough time has passed
        if (Time.time - lastFireTime >= fireRate)
        {
            lastFireTime = Time.time;

            // update firerate after every consecutive shot
            if (sFreq != shootingFrequency.constant) {
                tryUpdateFirerate();
            }


            if (projectilePrefab == null) 
            {
                Debug.LogError("Projectile prefab is missing! Please attach it to ShootProjectiles Script!");
                return;
            }

            fireVolley(enemyPosition, enemyLookDir, playerPos);

            // switch (sType) 
            // {
            //     case shootingType.single:
            //         fireVolley(enemyPosition, enemyLookDir, playerPos);
            //         break;
            //     case shootingType.narrowSpread:
            //         fireVolley(enemyPosition, enemyLookDir, playerPos);
            //         break;
            //     case shootingType.widespread:
            //         fireVolley(enemyPosition, enemyLookDir, playerPos);
            //         break;
            //     case shootingType.semicircleSpread:
            //         fireVolley(enemyPosition, enemyLookDir, playerPos);
            //         break;

            //     default:
            //         break;
            // }


        // /*TODO: DELETE BELOW HERE // REFERENCE CODE*/
        //     // Fire directly at player position
        //     if (projectileCt == 1) {
        //         Vector2 direction = playerPos - (Vector2)transform.position;
        //         float angleInRadians = Mathf.Atan2(direction.y, direction.x);
        //         directions[0] = angleInRadians * Mathf.Rad2Deg;

        //         Vector2 spawnPosition = enemyPosition + new Vector2 (.1f * enemyLookDir.x, 0);  
        //         GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        //         Projectile projScript = projectile.GetComponent<Projectile>();
        //         projScript.direction = directions[0];
        //         projScript.speed = projectileSpeed * 2;
        //     } else {
        //         // Instantiate and fire projectiles
        //         for (int i = 0; i < projectileCt; i++)
        //         {
        //             Vector2 spawnPosition = enemyPosition + new Vector2 (.1f * enemyLookDir.x, 0);  
        //             GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        //             Projectile projScript = projectile.GetComponent<Projectile>();

        //             /* Ensure symmetry and correct launch angles of projectiles */
        //             if (enemyLookDir.x > 0) {
        //                 projScript.direction = directions[i];
        //             } else {
        //                 projScript.direction = 180f - directions[i];
        //             }
        //             projScript.speed = projectileSpeed * 2;
        //         }
        //         projectileCt--;
        //     }
        }
    }

    // initialize before behavior (1)
    public void setProjectileEnums(shootingFrequency shootFreq, shootingType shootType, Projectile.projectileType projectileType, shootingDensity sDensity = shootingDensity.constant)
    {
        sFreq = shootFreq;
        sType = shootType;
        this.projectileType = projectileType;
        this.sDensity = sDensity;
        // this.projectileAttribute = projectileAttribute;

    }

    // initialize after enums (2)

    public void setProjectileBehavior(float baseFireRate, int numProjectiles, float projectileSpeed, string prefabName, bool randomSpread=false)
    {
        fireRate = baseFireRate;
        projectileCt = numProjectiles;
        this.projectileSpeed = projectileSpeed;
        projectilePrefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
        this.randomSpread = randomSpread;
        
        if (sDensity == shootingDensity.constant) {
            directions = new float[projectileCt];
        } else {
            directions = new float[maxProjectiles];
        }
    }

    /* Assign the expected projectile spread for 2+ shots */
    // public void setProjectileCount(int numProjectiles, string prefabName, float projectileSpd)
    // {
    //     projectileSpeed = projectileSpd;
    //     projectileCt = numProjectiles;
    //     directions = new float[projectileCt];
    //     projectilePrefab = Resources.Load<GameObject>("Prefabs/" + prefabName);

    //     float angleBetweenShots = 60f / (numProjectiles + 2);

    //     for (int i = 0; i < projectileCt; i++) {
    //         directions[i] = i * .8f * angleBetweenShots;
    //     }
    // }
}
