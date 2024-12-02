using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    Collider2D _collider;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    int fromWaypointIndex;
    float percentBetweenWayPoints;

    void Start()
    {
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
        _collider = GetComponent<Collider2D>();
    }

    
    void Update()
    {
        Vector3 velocity = CalculatePlatformMovement();
        transform.Translate(velocity, Space.World);

        var guo = new GraphUpdateObject(_collider.bounds);
        guo.updatePhysics = true;
        if (AstarPath.active != null) { AstarPath.active.UpdateGraphs(guo); }
    }

    Vector3 CalculatePlatformMovement()
    {
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWayPoints += Time.deltaTime * speed / distanceBetweenWaypoints;

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], percentBetweenWayPoints);

        if (percentBetweenWayPoints >= 1)
        {
            percentBetweenWayPoints = 0;
            fromWaypointIndex++;

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
        }

        return newPos - transform.position;
    }

    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
