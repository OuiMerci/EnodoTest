using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : MonoBehaviour {

    #region Fields
    public static float LINE_Z_OFFSET = 1;  //Draw the line slightly above the plane to avoid visual glitches
    public const float OTHER_POINT_DISTANCE = 1000;  //When testin if a point is inside the polygon, a segment is simulated, this determines its size
    private static int _currentId;
    private int _id;
    private List<PolyPoint> _pointList;
    private List<PolySegment> _segmentList;
    private LineRenderer _line;
    private Building _building;
    private List<Polygon> _connectedPolygons;
    #endregion

    #region Properties
    public int ID
    {
        get { return _id; }
    }

    public List<PolyPoint> Points
    {
        get { return _pointList; }
    }

    public List<PolySegment> Segments
    {
        get { return _segmentList; }
    }

    public Building LinkedBuilding
    {
        get { return _building; }
        set { _building = value; }
    }
    public List<Polygon> ConnectedPolygons
    {
        get { return _connectedPolygons; }
    }
    #endregion

    #region Methods
    private void OnEnable()
    {
        Cadastre.OnNewPolygon += OnNewPolygon;
        Cadastre.OnPolygonDestroyed += OnPolygonDestroyed;
    }
    private void OnDisable()
    {
        Cadastre.OnNewPolygon -= OnNewPolygon;
        Cadastre.OnPolygonDestroyed -= OnPolygonDestroyed;
    }


    private void DrawLine(List<Vector3> linePositions = null)
    {
        if (linePositions == null)
            linePositions = GetLinePositions();

        _line.positionCount = linePositions.Count;
        _line.SetPositions(linePositions.ToArray());
    }

    // This method isn't currenly used as we pass the positions at the initialisation, but I thought still needed to be able to compute the positions itself
    private List<Vector3> GetLinePositions()
    {
        List<Vector3> linePositions = new List<Vector3>();

        foreach(PolyPoint point in _pointList)
        {
            linePositions.Add(new Vector3(point.X, point.Y, Polygon.LINE_Z_OFFSET));
        }

        return linePositions;
    }

    private void OnNewPolygon(Polygon poly)
    {
        if (poly == this)
            return;

        //Check if this polygon is connected to the new one
        foreach(PolyPoint point in poly.Points)
        {
            if(_pointList.Contains(point))
            {
                _connectedPolygons.Add(poly);
                return; //The polygon is connected
            }
        }
    }

    //This is called during initiation to check connected polygons in the existing ones
    private void FillConnectedPolygonsList()
    {
        foreach (PolyPoint point in _pointList)
        {
            foreach (Polygon poly in point.LinkedPolygons)
            {
                if (poly != this && _connectedPolygons.Contains(poly) == false)
                {
                    _connectedPolygons.Add(poly);
                }
            }
        }
    }

    private void OnPolygonDestroyed(Polygon poly)
    {
        //Try to remove the destroyed polygon from the list of connected ones
        _connectedPolygons.Remove(poly);
    }

    public void InitPolygon(List<PolyPoint> points, List<PolySegment> segments, List<Vector3> linePositions = null)
    {
        // Init fields
        _connectedPolygons = new List<Polygon>();
        _pointList = new List<PolyPoint>();
        _segmentList = new List<PolySegment>();
        _line = GetComponent<LineRenderer>();
        _id = _currentId;
        _currentId++;

        // Fill lists
        _pointList.AddRange(points);
        _pointList.ForEach(x => x.AddLinkedPolygon(this)); //Link this polygon to its points
        _segmentList.AddRange(segments);

        // Draw Line
        DrawLine(linePositions);

        //Init the list of connected polygons
        FillConnectedPolygonsList();
    }

    public bool ContainsPoint(Vector2 point)
    {
        // Check if the click is actually on a polypoint of this polygon
        foreach (PolyPoint polypoint in _pointList)
        {
            if (Vector2.Distance(point, polypoint.asVector2) < Cadastre.SNAP_DISTANCE)
            {
                // The player clicked on a point, this is note considered as INSIDE the polygon, this click will be used to create a new polygon
                return false;
            }
        }

        int intersectionCounter = 0;
        Vector2 otherPoint = new Vector2(point.x, point.y + OTHER_POINT_DISTANCE);

        foreach(PolySegment seg in Segments)
        {
            if(seg.IsInterecting(point, otherPoint))
            {
                intersectionCounter++;
            }
        }

        return (intersectionCounter % 2 == 0) ? false : true;
    }

    

    public void BuildingGrow()
    {
        if(_building!=null)
            _building.Grow();
    }
    #endregion
}
