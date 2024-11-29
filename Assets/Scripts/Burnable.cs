using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    public GameObject fire;
    public LayerMask burnableLayer;
    public float fireSpreadRange;
    public float burnTime;
    public float timeToBurn;

    private bool onFire = false;
    private float heatLevel = 0;
    void Start()
    {
        fire.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(onFire)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, fireSpreadRange, burnableLayer);
            foreach (Collider2D collider in colliders)
            {
                Burnable _burnable = collider.GetComponent<Burnable>();
                if(_burnable != null)
                {
                    _burnable.HeatUp();
                }
            }
        }
    }

    public void HeatUp()
    {
        heatLevel += Time.deltaTime;
        if(heatLevel >= timeToBurn)
        {
            SetOnFire();
        }
    }
    
    public void SetOnFire()
    {
        onFire = true;
        fire.SetActive(true);
        StartCoroutine(burnWait(burnTime));

    }

    private IEnumerator burnWait(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
