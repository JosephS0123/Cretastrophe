using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class ChalkEater : MonoBehaviour
{
    private AIDestinationSetter astar;
    public float range = 5f;
    Collider2D _collider;
    public LayerMask chalkLayer;
    public Vector2 startPos;
    void Start()
    {
        startPos = transform.position;
        astar = gameObject.GetComponent<AIDestinationSetter>();
        _collider = gameObject.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, chalkLayer);
        float minDistance = range;
        Collider2D closest = null;
        foreach (Collider2D collider2D in colliders)
        {
            if (collider2D.GetComponent<Line>() != null)
            {
                float distance = Vector3.Distance(transform.position, collider2D.gameObject.transform.position);
                print(collider2D.gameObject.transform.position);
                if (distance < minDistance)
                {
                    //print("test");
                    minDistance = distance;
                    closest = collider2D;
                }
            }

        }

        if (closest != null)
        {
            astar.target = closest.transform;
        }
        else
        {
            astar.target = null;
        }

        //var guo = new GraphUpdateObject(GetComponent<Collider2D>().bounds);
        //guo.updatePhysics = true;
        //AstarPath.active.UpdateGraphs(guo);
    }

    private void getTarget()
    {

    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(0, 1, 0, .5f);
            Gizmos.DrawSphere(transform.position, range);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var guo = new GraphUpdateObject(_collider.bounds);
        guo.updatePhysics = true;
        AstarPath.active.UpdateGraphs(guo);
    }

    public void Respawn() 
    {
        transform.position = startPos;
        Start();
    }
}
