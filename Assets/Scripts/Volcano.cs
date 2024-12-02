using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volcano : MonoBehaviour
{
    public GameObject lava;
    public GameObject blast;
    public GameObject spout;

    private Transform lavaTransform;
    
    private float lavaPosMin = -0.25f;
    private float lavaPosMax = 0.29f;
    private float lavaPosMinGlobal;
    private float lavaPosMaxGlobal;
    float percentRisen;
    public float riseTime;
    public float blastTime;
    private bool blasting;
    public bool isActive;
    public float offset;
    public float blastSize;
    float cycleTime;

    void Start()
    {
        percentRisen = 0 - offset * 1/riseTime;
        lavaTransform = lava.GetComponent<Transform>();
        lavaPosMin = (lavaPosMin * transform.localScale.y);
        lavaPosMax = (lavaPosMax * transform.localScale.y);
        blast.SetActive(false);
        spout.SetActive(false);
        lava.SetActive(true);
        blasting = false;

        blast.GetComponent<SpriteRenderer>().size = new Vector2(0.62f, blastSize/transform.localScale.x);
        blast.transform.position = (blastSize/2) * transform.up + transform.position;
        spout.transform.position = (blastSize-.12f) * transform.up + transform.position;
    }

    
    void Update()
    {
        if (!blasting && isActive)
        {
            percentRisen += Time.deltaTime * 1 / riseTime;
            float newValue = lavaPosMin + (lavaPosMax - lavaPosMin) * Mathf.Clamp(percentRisen, 0, 1);

            Vector3 newPos = newValue * transform.up + transform.position;

            if (percentRisen >= 1)
            {
                percentRisen = 0;
                fireBlast();

            }

            lavaTransform.Translate((newPos - lavaTransform.position), Space.World);
        }

    }

    public void fireBlast()
    {
        blasting = true;
        blast.SetActive(true);
        spout.SetActive(true);
        //lava.SetActive(false);
        StartCoroutine(blastWait(blastTime));
    }

    private IEnumerator blastWait(float time)
    {
        yield return new WaitForSeconds(time);
        blasting = false;
        blast.SetActive(false);
        spout.SetActive(false);
        lava.SetActive(true);
    }

    void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
        float size = .3f;

        
        Vector3 globalWaypointPos = (blastSize - .12f) * transform.up + transform.position;
        Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
        Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
        
        
    }

    public void activate(bool active)
    {
        if(isActive)
        {
            percentRisen = 0;
            float newValue = lavaPosMin + (lavaPosMax - lavaPosMin) * Mathf.Clamp(percentRisen, 0, 1);

            Vector3 newPos = newValue * transform.up + transform.position;

            lavaTransform.Translate((newPos - lavaTransform.position), Space.World);
            blast.SetActive(false);
            spout.SetActive(false);
        }
        
        isActive = active;
    }
}
