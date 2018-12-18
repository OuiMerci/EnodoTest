using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingTool : MonoBehaviour {

    #region Fields
    static private DrawingTool _instance;
    private Cadastre _cadastre;
    private LineRenderer _line;
    private Pool<PolyPoint> _pointsPool;
    private List<PolyPoint> _pointList; //Temporary list for currently drawn points
    private List<PolySegment> _segmentList; // Temporary list for currently drawn segments
    private List<Vector3> _linePositionList; // used to draw the line
    #endregion

    #region Fields
    static public DrawingTool Instance
    {
        get { return _instance; }
    }
    #endregion

    #region Methods
    void Awake () {
		if(_instance == null)
        {
            _instance = this;
        }
	}

    private void Start()
    {
        _cadastre = Cadastre.Instance;
        _pointList = new List<PolyPoint>();
        _segmentList = new List<PolySegment>();
        _line = GetComponent<LineRenderer>();
        _linePositionList = new List<Vector3>();
        _pointsPool = _cadastre.PointsPool;
    }

    // Update is called once per frame
    void Update () {
	}

    /// <summary>
    /// Check if the mouse click was close enough to an existing point to assume that the player was clicking on it.
    /// We can only snap to an existing point of the cadastre and to the first one of the polygon we are currently drawing.
    /// </summary>
    /// <param name="hit">The clicked position</param>
    /// <param name="result">Either returns and existing point, or an new intialized PolyPoint</param>
    /// <returns></returns>
    private bool TrySnap(Vector2 hit, out PolyPoint result, out bool invalidPoint)
    {
        result = null;
        invalidPoint = false;
        float dist;

        if(_pointList.Count > 0)
        {
            // Try to snap the first of the polygon we are drawing
            dist = Vector2.Distance(hit, _pointList[0].asVector2);

            if (dist < Cadastre.SNAP_DISTANCE)
            {
                //Debug.Log("Snapping to first point, trying to close polygon");
                result = _pointList[0];
                return true;
            }
            else if (_pointList.Count > 1) // Just a protection, for the case where the player clicks too close from an invalid point
            {
                // We check all the points from the currently drawn polygon
                for(int i = 1; i < _pointList.Count; i++)
                {
                    dist = Vector2.Distance(hit, _pointList[i].asVector2);

                    if (dist < Cadastre.SNAP_DISTANCE)
                    {
                        Debug.Log("This point is invalid : non-closing point of the currently drawn polygon");
                        result = _pointList[i];
                        invalidPoint = true;
                        return true;
                    }
                }
            }
        }
        
        // Try to snap to existing points of the cadastre's polygons
        foreach (Polygon polygon in _cadastre.Polygons)
        {
            foreach (PolyPoint existingPoint in polygon.Points)
            {
                dist = Vector2.Distance(hit, existingPoint.asVector2);
                if(dist < Cadastre.SNAP_DISTANCE)
                {
                    //Debug.Log("Existing point found, snapping to point " + existingPoint.ID);
                    result = existingPoint;
                    return true;
                }
            }
        }
        
        result = _pointsPool.GetFromQueue();
        result.InitPoint(hit);
        return false;
    }

    private PolyPoint GetPreviousPoint()
    {
        if(_pointList.Count > 0)
        {
            return _pointList[_pointList.Count - 1];
        }
        else
        {
            Debug.LogError("There is no point in the list, you shouldn't try to access it.");
            return null;
        }
    }

    /// <summary>
    /// Test if the segment sent as parameter intersects with another one
    /// </summary>
    /// <param name="a">Start point of the new segment</param>
    /// <param name="b">End point of the new segment</param>
    /// <returns>resturns true if an intersection was found, returns false otherwise</returns>
    private bool IsNewIntersection(PolyPoint a, PolyPoint b)
    {
        //Check if the new point doesn't create a intersecting segment, we don't test the last segment because is is always has at least a point in common
        for (int i = 0; i < _segmentList.Count - 1; i++)
        {
            PolySegment seg = _segmentList[i];

            if (seg.IsInterecting(a.asVector2, b.asVector2))
            {
                // An intersection was detected
                Debug.LogWarning("Cannot create this new segment, it would intersect with segment : " + seg.ID);
                return true;
            }
        }

        //Check for segments from existing polygons
        foreach(Polygon polygon in _cadastre.Polygons)
        {
            foreach(PolySegment seg in polygon.Segments)
            {
                if(seg.IsInterecting(a.asVector2, b.asVector2))
                {
                    // An intersection was detected
                    Debug.LogWarning("Cannot create this new segment, it would intersect with segment : " + seg.ID);
                    return true;
                }
            }
        }

        // No intersection was detected
        return false;
    }

    private void TryAddNewSegment(PolyPoint nextPoint, bool snapped)
    {
        // Get the previous point we drew
        PolyPoint previousPoint = _pointList[_pointList.Count - 1];

        // Check if the new segment we're drawing will intersect with another one
        if (IsNewIntersection(previousPoint, nextPoint) || IsInsideExistingPoly(previousPoint, nextPoint))
        {
            // An intersection was found, if this point isn't used elsewhere, send it back to the pool
            if (snapped == false) // If we snapped on this point it means it is used by another polygon
                _pointsPool.AddToPool(nextPoint);
        }
        else
        {
            //No intersection was detected, we can create this segment and add this point to our temporary list
            PolySegment newSeg = new PolySegment(previousPoint, nextPoint);
            _pointList.Add(nextPoint);
            _segmentList.Add(newSeg);
            UpdateLinePositions(new Vector3(nextPoint.X, nextPoint.Y, Polygon.LINE_Z_OFFSET));
        }
    }

    /// <summary>
    /// Tests if the middle of the segment is inside of an existing polygon.
    /// </summary>
    /// <param name="a"> Start point of the segment</param>
    /// <param name="b">End point of the segment</param>
    /// <returns>True if the segment is inside an existing polygon</returns>
    private bool IsInsideExistingPoly(PolyPoint a, PolyPoint b)
    {
        float middleX = (a.X + b.X) / 2.0f;
        float middleY = (a.Y + b.Y) / 2.0f;
        Vector2 middlePoint = new Vector2(middleX, middleY);

        Polygon poly;
        if (_cadastre.IsPointInsideExistingPolygon(middlePoint, out poly))
        {
            return true;
        }
        else
        {
            return false;
        }
            
    }

    private void TryClosePolygon(PolyPoint nextPoint)
    {
        // Did we draw at least 3 point ?
        if (_pointList.Count >= 3)
        {
            // Check if the new segment we're drawing will intersect with another one
            if (IsNewIntersection(GetPreviousPoint(), nextPoint) == false)
            {
                //No intersection was detected, we can create this segment and close the polygon
                PolySegment newSeg = new PolySegment(GetPreviousPoint(), nextPoint);
                _segmentList.Add(newSeg);
                UpdateLinePositions(new Vector3(nextPoint.X, nextPoint.Y, Polygon.LINE_Z_OFFSET)); // Add the last line position

                //Debug.Log("CLOSING POLY : " + _linePositionList.Count);
                ClosePolygon();
            }
            else
            {
                Debug.Log("Cannot close the polygon from this point, it would overlap with anoter one");
            }
        }
        else
        {
            Debug.Log("You are trying to close this polygon, but it doesn't have enough points");
        }
    }

    private void ClosePolygon()
    {
        _cadastre.AddNewPolygon(_pointList, _segmentList, _linePositionList);
        ResetDrawingTool();
    }

    private void UpdateLinePositions(Vector3 point)
    {
        _linePositionList.Add(point);
        _line.positionCount = _linePositionList.Count;
        _line.SetPositions(_linePositionList.ToArray());
    }

    public void ResetDrawingTool()
    {
        foreach (PolyPoint point in _pointList)
        {
            point.CheckLinkedPolygonsCount(); // If this point is used by another polygon, it will stay, otherwhise it will be sent back to the pool
        }

        _pointList.Clear();
        _segmentList.Clear();
        _linePositionList.Clear();
        _line.positionCount = 0;
    }

    
    public void HandleNewClick(Vector2 clickPoint)
    {
        /// Currently 
        PolyPoint nextPoint = null;
        bool invalidPoint = false;
        bool snapped = TrySnap(clickPoint, out nextPoint, out invalidPoint); // Try to found if the player clicked on an existing point;

        if (invalidPoint)
            return;

        if (_pointList.Count > 0)
        {
            if (snapped == true && nextPoint == _pointList[0])
            {
                TryClosePolygon(nextPoint);
            }
            else
            {
                TryAddNewSegment(nextPoint, snapped);
            }
        }
        else
        {
            // this is the first point we draw, we don't need to check any intersections
            _pointList.Add(nextPoint);
            UpdateLinePositions(new Vector3(nextPoint.X, nextPoint.Y, Polygon.LINE_Z_OFFSET));
        }
    }
    #endregion
}
