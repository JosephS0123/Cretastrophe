using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
    private Vector2 prevMousePos;
    private Camera _cam;
    [SerializeField] private Line _whiteLinePrefab;
    [SerializeField] private Line _redLinePrefab;
    [SerializeField] private GameObject _parent;
    [SerializeField] private ChalkManager _whiteChalkManager;
    [SerializeField] private ChalkManager _redChalkManager;
    private ChalkManager _chalkManager;
    private Line _linePrefab;
    public GameObject eraser;
    private GameObject eraserInstance;
    public GameObject screenClear;
    private GameObject screenClearInstance;

    public LayerMask noDrawMask;
    public const float RESOLUTION = .1f;
    public const float amountChalkUsed = .1f;

    private Line _currentLine;
    void Start()
    {
        _cam = Camera.main;
        prevMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        _linePrefab = _whiteLinePrefab;
        _chalkManager = _whiteChalkManager;
    }

    
    void Update()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        bool canDraw = drawZoneCheck(mousePos);

        if(Input.GetMouseButtonDown(0) && canDraw)
        {
            _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
            _currentLine._chalkManager = _chalkManager;
            _currentLine.SetPosition(mousePos);
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 nextPos = Vector2.MoveTowards(prevMousePos, mousePos, RESOLUTION);

            if (_currentLine == null && !_chalkManager.isEmpty() && canDraw)
            {
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
                _currentLine._chalkManager = _chalkManager;
                _currentLine.SetPosition(mousePos);
            }

            while (_currentLine != null && _currentLine.CanAppend(mousePos) && _chalkManager.chalkAmount > 0)
            {
                canDraw = drawZoneCheck(nextPos);
                if (_chalkManager.isEmpty())
                {
                    _currentLine.destroy();
                    break;
                }

                if (!canDraw)
                {
                    nextPos = Vector2.MoveTowards(nextPos, mousePos, RESOLUTION);
                    _currentLine.destroy();
                    _currentLine = Instantiate(_linePrefab, nextPos, Quaternion.identity, _parent.transform);
                    _currentLine._chalkManager = _chalkManager;
                    _currentLine.SetPosition(nextPos);
                }
                else if (_currentLine.SetPosition(nextPos))
                {
                    _chalkManager.ReduceChalk(amountChalkUsed);
                    _currentLine = Instantiate(_linePrefab, nextPos, Quaternion.identity, _parent.transform);
                    _currentLine._chalkManager = _chalkManager;
                    _currentLine.SetPosition(nextPos);
                    nextPos = Vector2.MoveTowards(nextPos, mousePos, RESOLUTION);
                }
                else
                {
                    nextPos = Vector2.MoveTowards(nextPos, mousePos, RESOLUTION);
                }
                
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            eraserInstance = Instantiate(eraser, mousePos, Quaternion.identity);
        }

        if(Input.GetMouseButtonUp(0))
        {
            if(_currentLine != null)
            {
                _currentLine.destroy();
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Destroy(eraserInstance);
        }

        if(Input.GetKeyDown("1"))
        {
            _linePrefab = _whiteLinePrefab;
            _chalkManager = _whiteChalkManager;
            if(_currentLine != null && Input.GetMouseButton(0))
            {
                _currentLine.destroy();
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
                _currentLine._chalkManager = _chalkManager;
                _currentLine.SetPosition(mousePos);
            }
        }
        else if(Input.GetKeyDown("2"))
        {
            _linePrefab = _redLinePrefab;
            _chalkManager = _redChalkManager;
            if (_currentLine != null && Input.GetMouseButton(0))
            {
                _currentLine.destroy();
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
                _currentLine._chalkManager = _chalkManager;
                _currentLine.SetPosition(mousePos);
            }
        }

        if(Input.GetKeyDown("e"))
        {
            chalkClear();
        }

        

        prevMousePos = mousePos;
    }

    private bool drawZoneCheck(Vector2 curPos)
    {
        Ray ray = _cam.ScreenPointToRay(_cam.WorldToScreenPoint(curPos));
        RaycastHit2D[] hit = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);


        foreach (RaycastHit2D hit2D in hit)
        {
            if (hit2D)
            {
                if (hit2D.collider.tag == "NoDraw")
                {
                    return false;
                }
            }


            
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(curPos, _linePrefab._renderer.startWidth - .03f);

        foreach (Collider2D collider2D in colliders)
        {
            if (collider2D)
            {
                if (collider2D.tag == "Player")
                {
                    return false;
                }
            }



        }


        return true;
    }

    public void chalkClear()
    {
        screenClearInstance = Instantiate(screenClear, Vector3.zero, Quaternion.identity);
    }

}
