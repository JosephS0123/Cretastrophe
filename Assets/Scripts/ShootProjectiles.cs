using System.Buffers.Text;
using UnityEditor;
using UnityEngine;

public class ShootProjectiles : MonoBehaviour
{
    public GameObject projectilePrefab;  

    public enum shootingFrequency { accelerate, windup, constant, decelerate};
    public enum shootingType { single, widespread, narrowSpread, semicircleSpread };
    public enum shootingDensity {constant, decreasing, increasing}
    Projectile.projectileElement pElement;
    private shootingFrequency sFreq;
    private shootingType sType;
    private shootingDensity sDensity;
    public float minSingleShotfireRate = 12f;

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
    private bool isFireProjectile;

    // the max spread for a volley of projectiles (in degrees)
    private float getVolleyAngle()
    {
        if (sType == shootingType.narrowSpread) {
            return 40f;
        } else if (sType == shootingType.widespread) {
            return 65f;
        }else if (sType == shootingType.semicircleSpread) {
            return 180f;
        }
        return 0f;
    }


    // Used for any shotgun-based attack
    private void assignLaunchAngles()
    {
        if (!randomSpread) 
        {
            float angleBetweenShots = getVolleyAngle() / projectileCt;
            float angleStartPoint = sType == shootingType.semicircleSpread ? 0f : (sType == shootingType.widespread) ? 0f : 0f;
            float angleEndpoint = angleStartPoint + getVolleyAngle();

            // oscillate between start and end and converge towards the center when creating shotguns
            for (int i = 0; i < projectileCt; i++) {
                directions[i] = ((i%2 != 0) ? angleEndpoint : angleStartPoint)
                                + ((i%2==0) ? i/2 * angleBetweenShots : -i/2 * angleBetweenShots);
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
            print("degress = " + directions[0]);

            /* for now spawn projectile inside enemy */
            // Vector2 spawnPosition = enemyPosition + new Vector2 (enemyLookDir.x * .1f, 0);  
            Vector2 spawnPosition = enemyPosition;  
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            projScript.direction = directions[0];
            projScript.projectileA = projectileAttribute;
            projScript.projectileT = projectileType;
            projScript.speed = (projectileSpeed < minSingleShotfireRate) ? minSingleShotfireRate : projectileSpeed;
            projScript.pElement = pElement;
        
        } else if (!randomSpread) {
            assignLaunchAngles();

            for (int i = 0; i < projectileCt; i++)
            {
                // Vector2 spawnPosition = enemyPosition + new Vector2 (enemyLookDir.x * .1f, 0);  
                Vector2 spawnPosition = enemyPosition;  
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                Projectile projScript = projectile.GetComponent<Projectile>();
                projScript.projectileA = projectileAttribute;
                projScript.speed = projectileSpeed;
                projScript.projectileT = projectileType;
                projScript.pElement = pElement;

                /* Ensure symmetry and correct launch angles of projectiles */
                if (enemyLookDir.x > 0) {
                    projScript.direction = directions[i];
                } else {
                    projScript.direction = 180f - directions[i];
                }
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

        }
    }

    // initialize before behavior (1)
    public void setProjectileEnums(shootingFrequency shootFreq, shootingType shootType, Projectile.projectileType projectileType, shootingDensity sDensity = shootingDensity.constant, Projectile.projectileAttribute pAttribute=Projectile.projectileAttribute.nonsticky, Projectile.projectileElement pElement = Projectile.projectileElement.normal)
    {
        sFreq = shootFreq;
        sType = shootType;
        this.projectileType = projectileType;
        this.sDensity = sDensity;
        this.projectileAttribute = pAttribute;
        this.pElement = pElement;
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

        if (projectileType == Projectile.projectileType.blackhole) {
            fireRate = 5f;
            this.projectileSpeed = 2f;
        }
    }
}
