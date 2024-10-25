using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    
    
    [SerializeField] public LineRenderer _renderer;
    [SerializeField] private PolygonCollider2D _collider;
    public ChalkManager _chalkManager = null;

    private readonly List<Vector2> _points = new List<Vector2>();
    void Start()
    {
        _collider.transform.position -= transform.position;
    }

    void Update()
    {
        
    }

    public bool SetPosition(Vector2 pos)
    {
        if (!CanAppend(pos)) return false;

        
        _points.Add(pos);
        _renderer.positionCount++;
        _renderer.SetPosition(_renderer.positionCount - 1, pos);

        //_collider.points = _points.ToArray();

        if(_renderer.positionCount > 1)
        {
            //List<Vector2> verts = new List<Vector2>();
            //verts.Add(_points[0]);

            Mesh mesh = new Mesh();
            _renderer.BakeMesh(mesh, true);
            //var boundary = EdgeHelpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();
            
            //print(boundary.Count);

            List<Vector2> verts = new List<Vector2>();

            
            foreach (Vector2 vertex in mesh.vertices)
            {
                //verts.Add(mesh.vertices[edge.v1]);
                
                if(!verts.Contains(vertex))
                {  
                    verts.Add(vertex);
                }
                
            }

            verts = ConvexHull.compute(verts);

            _collider.points = verts.ToArray();
            


            return true;

        }
        return false;
    }

    //public int GetPositionCount()
    //{
    //    return _renderer.positionCount;
    //}

    public bool CanAppend(Vector2 pos)
    {
        if(_renderer.positionCount == 0)
        {
            return true;
        }

        return Vector2.Distance(_renderer.GetPosition(_renderer.positionCount - 1), pos) > DrawManager.RESOLUTION;
    }

    public void destroy()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy the item after its collision triggered
        if (collision.gameObject.tag == "Eraser")
        {
            _chalkManager.ReplenishChalk(.1f);
            Destroy(gameObject);
        }
    }

}
