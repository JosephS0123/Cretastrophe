using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class Line : MonoBehaviour
{
    
    
    [SerializeField] public LineRenderer _renderer;
    [SerializeField] private PolygonCollider2D _collider;
    public ChalkManager _chalkManager = null;
    public GameObject dynamicLineParent;
    private bool isErased = false;
    public bool touchedLava = false;
    public float heatLevel = 0;
    public bool inLava = false;
    public bool bluePrevCheck = true;

    private readonly List<Vector2> _points = new List<Vector2>();
    void Start()
    {
        //transform.position -= transform.position;
    }

    private void Update()
    {
        if(inLava)
        {
            HeatUp();
        }
    }

    public bool SetPosition(Vector2 _pos)
    {
        Vector2 pos = Vector2.zero;
        pos.x = _pos.x - transform.position.x;
        pos.y = _pos.y - transform.position.y;

        if (!CanAppend(pos))
        {
            return false;
        }

        
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
        return Vector2.Distance(_renderer.GetPosition(_renderer.positionCount - 1), pos) - DrawManager.RESOLUTION > float.Epsilon;
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

        return Vector2.Distance(_renderer.GetPosition(_renderer.positionCount - 1), pos) - DrawManager.RESOLUTION > float.Epsilon;
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
        if ((collision.gameObject.tag == "Eraser" || collision.gameObject.tag == "BallDespawn") && !isErased)
        {
            if(gameObject.tag == "BlueLine")
            {
                Transform _parent = gameObject.transform.parent;
                Vector3 _velocity = _parent.GetComponent<Rigidbody2D>().velocity;
                int newChildren = 0, oldChildren = 0;
                if(_parent.gameObject.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic)
                {
                    GameObject _newParent = Instantiate(dynamicLineParent, gameObject.transform.position, Quaternion.identity, _parent.transform.parent);
                    List<Transform> children = new List<Transform>();
                    DrawManager _drawManager = _chalkManager.gameObject.GetComponent<DrawManager>();
                    _drawManager.updateDynamicParent(_newParent);
                    newChildren++;
                    foreach (Transform child in _parent.transform)
                    {
                        children.Add(child);
                    }

                    bool afterCurrent = false;
                    foreach (Transform child in children)
                    {
                        if (afterCurrent)
                        {
                            child.parent = _newParent.transform;
                            newChildren++;
                        }
                        else
                        {
                            if (child.transform == gameObject.transform)
                            {
                                afterCurrent = true;
                            }
                            oldChildren++;
                        }
                    }

                    if (newChildren == 0)
                    {
                        Destroy(_newParent.gameObject);
                    }
                    
                     _parent.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    
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
                            newChildren++;
                        }
                        else
                        {
                            if (child.transform == gameObject.transform)
                            {
                                afterCurrent = true;
                            }
                            oldChildren++;
                        }
                    }

                    if (newChildren == 0)
                    {
                        Destroy(_newParent.gameObject);
                    }
                    else
                    {
                        _newParent.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                        _newParent.gameObject.GetComponent<Rigidbody2D>().velocity = _velocity;
                    }

                }

                _parent = gameObject.transform.parent;
                if (oldChildren == 1)
                {
                    Destroy(_parent.gameObject);
                }
            }
            _chalkManager.ReplenishChalk(.1f);
            gameObject.transform.parent = null;
            Destroy(gameObject);
            isErased = true;
        }
        else if (collision.CompareTag("Lava") && !touchedLava)
        {
            inLava = true;
            HeatUp();
        }
    }

    


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Lava"))
        {
            touchedLava = false;
            inLava = false;
        }
    }

    /*private void OnCollisionStay2D(Collision2D collision)
    {
        if (inLava && collision.collider.CompareTag("BlueLine") && (gameObject.CompareTag("White") || gameObject.CompareTag("Red")))
        {
            _chalkManager.ReplenishChalk(.1f);
            Destroy(gameObject);
        }
    }*/

    private void HeatUp()
    {
        if (heatLevel >= 0.5f)
        {
            if (gameObject.CompareTag("White"))
            {
                DrawManager _drawManager = _chalkManager.gameObject.GetComponent<DrawManager>();
                _drawManager.createLine(1, gameObject.GetComponent<Line>());
                _chalkManager.ReplenishChalk(.1f);
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("BlueLine"))
            {
                DrawManager _drawManager = _chalkManager.gameObject.GetComponent<DrawManager>();
                Transform _parent = gameObject.transform.parent;
                if (_parent.gameObject.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic)
                {
                    List<Transform> children = new List<Transform>();
                    bool allInLava = true;
                    foreach (Transform child in _parent.transform)
                    {
                        children.Add(child);
                        if (!child.gameObject.GetComponent<Line>().inLava)
                        {
                            allInLava = false;
                        }
                    }
                    if (allInLava)
                    {
                        foreach (Transform child in children)
                        {
                            _drawManager.createLine(2, child.gameObject.GetComponent<Line>());
                            child.gameObject.GetComponent<Line>().EraseDynamic();
                        }
                    }
                    else
                    {
                        foreach (Transform child in children)
                        {
                            _drawManager.createLine(2, child.gameObject.GetComponent<Line>());
                            child.gameObject.GetComponent<Line>().EraseDynamic();
                        }
                    }
                }
                else
                {
                    _drawManager.createLine(2, gameObject.GetComponent<Line>());
                    EraseDynamic();
                }
            }
        }
        else
        {
            Rigidbody2D _rb = gameObject.transform.parent.GetComponent<Rigidbody2D>();
            if(gameObject.CompareTag("BlueLine") && _rb != null && (_rb.bodyType == RigidbodyType2D.Dynamic))
            {
                heatLevel += Time.deltaTime * 1.5f;
            }
            else
            {
                heatLevel += Time.deltaTime;
            }
        }
    }

    private void EraseDynamic()
    {
        Transform _parent = gameObject.transform.parent;
        Vector3 _velocity = _parent.GetComponent<Rigidbody2D>().velocity;
        int newChildren = 0, oldChildren = 0;
        if (_parent.gameObject.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic)
        {
            GameObject _newParent = Instantiate(dynamicLineParent, gameObject.transform.position, Quaternion.identity, _parent.transform.parent);
            List<Transform> children = new List<Transform>();
            DrawManager _drawManager = _chalkManager.gameObject.GetComponent<DrawManager>();
            _drawManager.updateDynamicParent(_newParent);
            newChildren++;
            foreach (Transform child in _parent.transform)
            {
                children.Add(child);
            }

            bool afterCurrent = false;
            foreach (Transform child in children)
            {
                if (afterCurrent)
                {
                    child.parent = _newParent.transform;
                    newChildren++;
                }
                else
                {
                    if (child.transform == gameObject.transform)
                    {
                        afterCurrent = true;
                    }
                    oldChildren++;
                }
            }

            if (newChildren == 0)
            {
                Destroy(_newParent.gameObject);
            }

            _parent.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
        else if (_parent.childCount == 1)
        {
            Destroy(_parent.gameObject);
        }
        else
        {
            GameObject _newParent = Instantiate(dynamicLineParent, gameObject.transform.position, Quaternion.identity, _parent.transform.parent);
            List<Transform> children = new List<Transform>();
            foreach (Transform child in _parent.transform)
            {
                children.Add(child);
            }

            bool afterCurrent = false;
            foreach (Transform child in children)
            {
                if (afterCurrent)
                {
                    child.parent = _newParent.transform;
                    newChildren++;
                }
                else
                {
                    if (child.transform == gameObject.transform)
                    {
                        afterCurrent = true;
                    }
                    oldChildren++;
                }
            }

            if (newChildren == 0)
            {
                Destroy(_newParent.gameObject);
            }
            else
            {
                _newParent.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                _newParent.gameObject.GetComponent<Rigidbody2D>().velocity = _velocity;
            }

        }

        _parent = gameObject.transform.parent;
        if (oldChildren == 1)
        {
            Destroy(_parent.gameObject);
        }
    
        _chalkManager.ReplenishChalk(.1f);
        gameObject.transform.parent = null;
        Destroy(gameObject);
        isErased = true;
    }

}
