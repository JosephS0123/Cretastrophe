using System.Collections;
using System.Collections.Generic;
using Pathfinding;
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
        var guo = new GraphUpdateObject(GetComponent<Collider2D>().bounds);
        guo.updatePhysics = true;
        if (AstarPath.active != null) { AstarPath.active.UpdateGraphs(guo); }
        Destroy(gameObject);
        
    }
}
