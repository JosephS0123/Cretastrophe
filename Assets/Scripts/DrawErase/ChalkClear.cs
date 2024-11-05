using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalkClear : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(deleteSelf());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator deleteSelf()
    {
        yield return new WaitForSeconds(.1f);
        Destroy(gameObject);
        
    }
}
