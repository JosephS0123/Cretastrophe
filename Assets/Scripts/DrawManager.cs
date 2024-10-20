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


    public const float RESOLUTION = .2f;
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
        bool canDraw = drawZoneCheck();

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

                if (_chalkManager.isEmpty() || !canDraw)
                {
                    _currentLine.destroy();
                    break;
                }

                if (_currentLine.SetPosition(nextPos))
                {
                    _chalkManager.ReduceChalk(amountChalkUsed);
                    _currentLine = Instantiate(_linePrefab, nextPos, Quaternion.identity, _parent.transform);
                    _currentLine._chalkManager = _chalkManager;
                    _currentLine.SetPosition(nextPos);
                }
                nextPos = Vector2.MoveTowards(nextPos, mousePos, RESOLUTION);
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

        

        prevMousePos = mousePos;
    }

    private bool drawZoneCheck()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);


        if (hit)
        {
            if (hit.collider.tag == "NoDraw" || hit.collider.tag == "Player")
            {
                return false;
            }
        }
        

        return true;
    }
}
