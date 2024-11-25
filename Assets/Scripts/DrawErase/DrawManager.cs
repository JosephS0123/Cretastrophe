using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
    private Vector2 prevMousePos;
    private Camera _cam;
    [SerializeField] private Line _whiteLinePrefab;
    [SerializeField] private Line _redLinePrefab;
    [SerializeField] private Line _blueLinePrefab;
    [SerializeField] private GameObject _parent;
    [SerializeField] private ChalkManager _whiteChalkManager;
    [SerializeField] private ChalkManager _redChalkManager;
    [SerializeField] private ChalkManager _blueChalkManager;
    private ChalkManager _chalkManager;
    private Line _linePrefab;
    public GameObject eraser;
    private GameObject eraserInstance;
    public GameObject screenClear;
    private GameObject screenClearInstance;

    public const float RESOLUTION = .1f;
    public const float amountChalkUsed = .1f;

    public GameObject dynamicLineParent;
    private GameObject _currentDynamicParent;
    private Line _currentLine;
    private Line _prevLine;
    private bool isDynamic = false;

    public bool canDrawWhite;
    public bool canDrawRed;
    public bool canDrawBlue;
    void Start()
    {
        _cam = Camera.main;
        prevMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        //_linePrefab = _whiteLinePrefab;
        _chalkManager = _whiteChalkManager;
    }


    void Update()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        bool canDraw = drawZoneCheck(mousePos) && _linePrefab != null && (canDrawWhite || canDrawRed || canDrawBlue);

        if(Input.GetMouseButtonDown(0) && canDraw)
        {
            if (!isDynamic)
            {
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
            }
            else
            {
                _currentDynamicParent = Instantiate(dynamicLineParent, mousePos, Quaternion.identity, _parent.transform);
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _currentDynamicParent.transform);
            }
            _currentLine._chalkManager = _chalkManager;
            _currentLine.SetPosition(mousePos);
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDynamic)
            {
                drawStaticLine(mousePos, canDraw);
            }
            else
            {
                drawDynamicLine(mousePos, canDraw);
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
                if(isDynamic)
                {
                    _currentDynamicParent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                }
                _currentLine.destroy();
            }
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            Destroy(eraserInstance);
        }

        if(Input.GetKeyDown("1") && canDrawWhite)
        {
            _linePrefab = _whiteLinePrefab;
            _chalkManager = _whiteChalkManager;
            if(_currentLine != null && Input.GetMouseButton(0))
            {
                _currentLine.destroy();
                if (isDynamic)
                {
                    _currentDynamicParent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                }
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
                _currentLine._chalkManager = _chalkManager;
                _currentLine.SetPosition(mousePos);
            }
            isDynamic = false;
        }
        else if(Input.GetKeyDown("2") && canDrawRed)
        {
            _linePrefab = _redLinePrefab;
            _chalkManager = _redChalkManager;
            if (_currentLine != null && Input.GetMouseButton(0))
            {
                _currentLine.destroy();
                if (isDynamic)
                {
                    _currentDynamicParent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                }
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
                _currentLine._chalkManager = _chalkManager;
                _currentLine.SetPosition(mousePos);
            }
            isDynamic = false;
        }
        else if (Input.GetKeyDown("3") && canDrawBlue)
        {
            _linePrefab = _blueLinePrefab;
            _chalkManager = _blueChalkManager;
            isDynamic = true;
            if (_currentLine != null && Input.GetMouseButton(0))
            {
                _currentLine.destroy();
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
                _currentLine._chalkManager = _chalkManager;
                _currentLine.SetPosition(mousePos);
            }
        }

        if (Input.GetKeyDown("e"))
        {
            chalkClear();
        }

        

        prevMousePos = mousePos;
    }

    private void drawStaticLine(Vector2 mousePos, bool canDraw)
    {
        Vector2 nextPos = Vector2.MoveTowards(prevMousePos, mousePos, RESOLUTION);

        if (_currentLine == null && !_chalkManager.isEmpty() && canDraw)
        {
            _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
            _currentLine._chalkManager = _chalkManager;
            _currentLine.SetPosition(mousePos);
        }

        while (_currentLine != null && _currentLine.CanAppendWorldSpace(mousePos) && _chalkManager.chalkAmount > 0)
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

    private void drawDynamicLine(Vector2 mousePos, bool canDraw)
    {
        Vector2 nextPos = Vector2.MoveTowards(prevMousePos, mousePos, RESOLUTION);

        if (_currentLine == null && !_chalkManager.isEmpty() && canDraw)
        {
            _currentDynamicParent = Instantiate(dynamicLineParent, mousePos, Quaternion.identity, _parent.transform);
            _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _currentDynamicParent.transform);
            _currentLine._chalkManager = _chalkManager;
            _currentLine.SetPosition(mousePos);
        }

        while (_currentLine != null && _currentLine.CanAppendWorldSpace(mousePos) && _chalkManager.chalkAmount > 0)
        {
            canDraw = drawZoneCheck(nextPos);
            if (_chalkManager.isEmpty())
            {
                _currentLine.destroy();
                _currentDynamicParent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                break;
            }

            if (!canDraw)
            {
                nextPos = Vector2.MoveTowards(nextPos, mousePos, RESOLUTION);
                _currentLine.destroy();
                _currentDynamicParent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

                _currentDynamicParent = Instantiate(dynamicLineParent, mousePos, Quaternion.identity, _parent.transform);
                _currentLine = Instantiate(_linePrefab, nextPos, Quaternion.identity, _currentDynamicParent.transform);
                _currentLine._chalkManager = _chalkManager;
                _currentLine.SetPosition(nextPos);
            }
            else if (_currentLine.SetPosition(nextPos))
            {
                _chalkManager.ReduceChalk(amountChalkUsed);

                _currentLine = Instantiate(_linePrefab, nextPos, Quaternion.identity, _currentDynamicParent.transform);
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


    private bool drawZoneCheck(Vector2 curPos)
    {
        Ray ray = _cam.ScreenPointToRay(_cam.WorldToScreenPoint(curPos));
        RaycastHit2D[] hit = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);


        foreach (RaycastHit2D hit2D in hit)
        {
            if (hit2D)
            {
                if (hit2D.collider.tag == "NoDraw" || hit2D.collider.tag == "Eraser" || hit2D.collider.tag == "Ground")
                {
                    return false;
                }
            }


            
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(curPos, _whiteLinePrefab._renderer.startWidth - .03f);

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

    public void changeColor(String color)
    {
        if (color == "white")
        {
            _linePrefab = _whiteLinePrefab;
            _chalkManager = _whiteChalkManager;
        }
        else if (color == "red")
        {
            _linePrefab = _redLinePrefab;
            _chalkManager = _redChalkManager;
        }
        else if (color == "blue")
        {
            _linePrefab = _blueLinePrefab;
            _chalkManager = _blueChalkManager;
        }
    }
}
