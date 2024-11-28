using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volcano : MonoBehaviour
{
    public GameObject lava;
    public GameObject blast;

    private Transform lavaTransform;
    
    private float lavaPosMin = -0.25f;
    private float lavaPosMax = 0.29f;
    private float lavaPosMinGlobal;
    private float lavaPosMaxGlobal;
    float percentRisen;
    public float riseTime;
    public float blastTime;
    private bool blasting;
    public float offset;

    void Start()
    {
        lavaTransform = lava.GetComponent<Transform>();
        lavaPosMin = (lavaPosMin * transform.localScale.y);
        lavaPosMax = (lavaPosMax * transform.localScale.y);
        blast.SetActive(false);
        blasting = false;

        blasting = true;
        StartCoroutine(blastWait(offset));
    }

    
    void Update()
    {
        if (!blasting)
        {
            percentRisen += Time.deltaTime * 1 / riseTime;
            float newValue = lavaPosMin + (lavaPosMax - lavaPosMin) * percentRisen;

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
        lava.SetActive(false);
        StartCoroutine(blastWait(blastTime));
    }

    private IEnumerator blastWait(float time)
    {
        yield return new WaitForSeconds(time);
        blasting = false;
        blast.SetActive(false);
        lava.SetActive(true);
    }
}
