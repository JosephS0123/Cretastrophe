using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoBlast : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Burnable _burnable = collision.gameObject.GetComponent<Burnable>();
        if (_burnable != null)
        {
            _burnable.SetOnFire();
        }
    }
}
