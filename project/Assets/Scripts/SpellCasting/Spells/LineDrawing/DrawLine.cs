using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private float lineThickness = 10f;
    [SerializeField] private float lineUpdateDistance = 0.25f;
    
    private GameObject _currentLine;
    private LineRenderer _lineRenderer;
    private EdgeCollider2D _edgeCollider;
    private List<Vector3> _fingerPositions = new List<Vector3>();

    public LineRenderer LineRenderer => _lineRenderer;

    public Action<List<Vector3>> OnLineFinished;

    public void SetEnabled(bool isEnabled)
    {
        this.isEnabled = isEnabled;
    }

    private bool isEnabled = false;

    // Update is called once per frame
    private void Update()
    {
        if (!isEnabled) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            CreateLine();
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 tempFingerPose = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
            if (Vector2.Distance(tempFingerPose, _fingerPositions[^1]) > lineUpdateDistance)
            {
                UpdateLine(tempFingerPose);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnLineFinished?.Invoke(_fingerPositions);
            
            // cleanup drawn lines
            Destroy(_currentLine);
            _fingerPositions.Clear();
        }
#else
        if (Input.touchCount > 0)
        {
            // There is at least one touch
            Touch touch = Input.GetTouch(0);
        
            // You can get the position of the touch in screen coordinates
            Vector2 touchPosition = touch.position;
        
            // You can also get the phase of the touch (e.g. began, moved, ended)
            TouchPhase touchPhase = touch.phase;
        
            // You can use the phase to perform actions based on the touch input
            if (touchPhase == TouchPhase.Began)
            {
                // The touch just began
                Debug.Log("Touch began at position: " + touchPosition);
                
                CreateLine();
        
            }
            else if (touchPhase == TouchPhase.Moved)
            {
                // The touch moved
                Debug.Log("Touch moved to position: " + touchPosition);
                
                Vector2 tempFingerPose = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
                if (Vector2.Distance(tempFingerPose, _fingerPositions[^1]) > lineUpdateDistance)
                {
                    UpdateLine(tempFingerPose);
                }
            }
            else if (touchPhase == TouchPhase.Ended)
            {
                // The touch ended
                Debug.Log("Touch ended at position: " + touchPosition);
                
                OnLineFinished?.Invoke(_fingerPositions);
            
                // cleanup drawn lines
                Destroy(_currentLine);
                _fingerPositions.Clear();
            }
        }
        
#endif
    }

    private void CreateLine()
    {
        _currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        _lineRenderer = _currentLine.GetComponent<LineRenderer>();
        _edgeCollider = _currentLine.GetComponent<EdgeCollider2D>();
        _fingerPositions.Clear();

        _lineRenderer.widthMultiplier = lineThickness;

        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        _fingerPositions.Add(mousePos);
        _fingerPositions.Add(mousePos);
        
        _lineRenderer.SetPosition(0, _fingerPositions[0]);
        _lineRenderer.SetPosition(1, _fingerPositions[1]);

        // _edgeCollider.points = _fingerPositions.ToArray();
    }

    void UpdateLine(Vector2 newFingerPose)
    {
        _fingerPositions.Add(newFingerPose);
        _lineRenderer.positionCount++;
        _lineRenderer.SetPosition(_lineRenderer.positionCount -1, newFingerPose);
        // _edgeCollider.points = _fingerPositions.ToArray();
    }

    public void Button_ResetDrawing()
    {
        Destroy(_currentLine);
        _fingerPositions.Clear();

    }
}
