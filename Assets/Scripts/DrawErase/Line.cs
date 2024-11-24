using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class Line : MonoBehaviour
{
    
    
    [SerializeField] public LineRenderer _renderer;
    [SerializeField] private PolygonCollider2D _collider;
    public ChalkManager _chalkManager = null;
    public GameObject dynamicLineParent;

    private readonly List<Vector2> _points = new List<Vector2>();
    void Start()
    {
        //transform.position -= transform.position;
    }

    public bool SetPosition(Vector2 _pos)
    {
        Vector2 pos = Vector2.zero;
        pos.x = _pos.x - transform.position.x;
        pos.y = _pos.y - transform.position.y;
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

            var guo = new GraphUpdateObject(_collider.bounds);
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo);

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

    public bool CanAppendWorldSpace(Vector2 _pos)
    {
        if (_renderer.positionCount == 0)
        {
            return true;
        }

        Vector2 pos = Vector2.zero;
        pos.x = _pos.x - transform.position.x;
        pos.y = _pos.y - transform.position.y;

        return Vector2.Distance(_renderer.GetPosition(_renderer.positionCount - 1), pos) > DrawManager.RESOLUTION;
    }

    public void destroy()
    {
        Destroy(gameObject);
        if (gameObject.tag == "BlueLine")
        {
            if (gameObject.transform.parent.childCount == 1)
            {
                Destroy(gameObject.transform.parent.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy the item after its collision triggered
        if (collision.gameObject.tag == "Eraser")
        {
            if(gameObject.tag == "BlueLine")
            {
                Transform _parent = gameObject.transform.parent;
                if(_parent.gameObject.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic)
                {
                    
                }
                else if (_parent.childCount == 1)
                {
                    Destroy(_parent.gameObject);
                }
                else
                {
                    GameObject _newParent = Instantiate(dynamicLineParent, gameObject.transform.position, Quaternion.identity, _parent.transform.parent);
                    List<Transform> children = new List<Transform>();
                    foreach(Transform child in _parent.transform)
                    {
                        children.Add(child);
                    }

                    bool afterCurrent = false;
                    foreach (Transform child in children)
                    {
                        if (afterCurrent)
                        {
                            child.parent = _newParent.transform;
                        }
                        if (child.transform == gameObject.transform)
                        {
                            afterCurrent = true;
                        }
                    }
                    if (_newParent.transform.childCount == 0)
                    {
                        Destroy(_newParent.gameObject);
                    }
                    else
                    {
                        _newParent.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    }
                    if (_parent.childCount == 1)
                    {
                        Destroy(_parent.gameObject);
                    }
                }

            }
            _chalkManager.ReplenishChalk(.1f);
            Destroy(gameObject);

            
        }
    }

}
