using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField] private LineRenderer _renderer;
    [SerializeField] private EdgeCollider2D _collider;

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

        _collider.points = _points.ToArray();

        if(_renderer.positionCount > 1)
        {
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

}
