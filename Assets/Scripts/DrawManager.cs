using System.Collections;
using System.Collections.Generic;
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


    public const float RESOLUTION = .04f;
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
        

        if(Input.GetMouseButtonDown(0))
        {
            _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
            _currentLine.SetPosition(mousePos);
        }

        if(Input.GetMouseButton(0))
        {
            Vector2 nextPos = Vector2.MoveTowards(prevMousePos, mousePos, RESOLUTION);

            if (_currentLine == null && !_chalkManager.isEmpty())
            {
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
                _currentLine.SetPosition(mousePos);
            }

            while (_currentLine != null && _currentLine.CanAppend(mousePos) && _chalkManager.chalkAmount > 0)
            {
                _chalkManager.ReduceChalk(amountChalkUsed);
                if(_chalkManager.isEmpty())
                {
                    _currentLine.destroy();
                    break;
                }

                if (_currentLine.SetPosition(nextPos))
                {
                    _currentLine = Instantiate(_linePrefab, nextPos, Quaternion.identity, _parent.transform);
                    _currentLine.SetPosition(nextPos);
                }
                nextPos = Vector2.MoveTowards(nextPos, mousePos, RESOLUTION);
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            if(_currentLine != null)
            {
                _currentLine.destroy();
            }
        }

        if(Input.GetKeyDown("1"))
        {
            _linePrefab = _whiteLinePrefab;
            _chalkManager = _whiteChalkManager;
            if(_currentLine != null && Input.GetMouseButton(0))
            {
                _currentLine.destroy();
                _currentLine = Instantiate(_linePrefab, mousePos, Quaternion.identity, _parent.transform);
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
                _currentLine.SetPosition(mousePos);
            }
        }

        prevMousePos = mousePos;
    }
}
