using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cadastre : MonoBehaviour {

    #region Fields
    public const float SNAP_DISTANCE = 4; // Distance at which the click is snapped on an existing point

    // Poly pool
    [SerializeField] private Polygon _polygonPrefab;
    [SerializeField] private int _polygonPoolStartCount;
    [SerializeField] private int _polygonPoolMaxCount;
    [SerializeField] private Transform _polygonsParent;
    [SerializeField] private Transform _polygonsPoolParent;

    // Points pool
    [SerializeField] private PolyPoint _pointPrefab;
    [SerializeField] private Transform _pointsParent;
    [SerializeField] private Transform _pooledPointsParent;
    [SerializeField] private int _pointPoolStartCount;
    [SerializeField] private int _pointPoolMaxCount;

    // Events
    public delegate void PolygonCreated(Polygon newPoly);
    public static event PolygonCreated OnNewPolygon;
    public delegate void PolygonDestroyed(Polygon newPoly);
    public static event PolygonDestroyed OnPolygonDestroyed;

    static private Cadastre _instance;
    private List<Polygon> _polygons;
    private Pool<Polygon> _polygonsPool;
    private Pool<PolyPoint> _pointsPool;
    #endregion

    #region properties
    static public Cadastre Instance
    {
        get { return _instance; }
    }

    public List<Polygon> Polygons
    {
        get { return _polygons; }
    }

    public Pool<PolyPoint> PointsPool
    {
        get { return _pointsPool; }
    }
    #endregion

    #region Methods
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        _polygons = new List<Polygon>();
        _polygonsPool = new Pool<Polygon>(_polygonPrefab, _polygonPoolStartCount, _polygonPoolMaxCount, _polygonsPoolParent, _polygonsParent);
        _pointsPool = new Pool<PolyPoint>(_pointPrefab, _pointPoolStartCount, _pointPoolMaxCount, _pooledPointsParent, _pointsParent);
    }

    // Currently Unused
    //private void AddList<T>(List<T> permaList, List<T> tempList)
    //{
    //    foreach (T newT in tempList)
    //    {
    //        if(permaList.Contains(newT) == false)
    //        {
    //            permaList.Add(newT);
    //        }
    //    }
    //}

    /// <summary>
    /// Used to check if a new polygon isn't build around another polygon
    /// </summary>
    private bool CheckPolygonInside(Polygon newPolygon)
    {
        // Check if any existing polygon has at least one point inside newPolygon
        foreach(Polygon polygon in _polygons)
        {
            foreach(PolyPoint point in polygon.Points)
            {
                if(newPolygon.ContainsPoint(point.asVector2))
                {
                    Debug.LogWarning("The new polygon is built around an existing polygon. Polygon cancelled");
                    return true;
                }
            }
        }

        return false;
    }

    public void AddNewPolygon(List<PolyPoint> newPoints, List<PolySegment> newSegments, List<Vector3> linePositions)
    {
        Polygon newPolygon = _polygonsPool.GetFromQueue();
        newPolygon.InitPolygon(newPoints, newSegments, linePositions);

        if(CheckPolygonInside(newPolygon)) // Is this polygon is built around another polygon
        {
            // Disconnect this polygon from its points
            foreach (PolyPoint point in newPoints)
            {
                point.RemoveLinkedPolygon(newPolygon);
            }

            // Unused Destroyed Polygon Event
            if (OnNewPolygon != null)
                OnPolygonDestroyed(newPolygon);

            // Send polygon back to pool
            _polygonsPool.AddToPool(newPolygon);
        }
        else
        {
            _polygons.Add(newPolygon);

            if (OnNewPolygon != null)
                OnNewPolygon(newPolygon);
        }
    }

    /// <summary>
    /// Tests if any of the polygons contains the point
    /// </summary>
    /// <param name="point">the points that is being tested</param>
    /// <param name="result">The polygon that contains that point</param>
    /// <returns>True if an existing polygon contains that point</returns>
    public bool IsPointInsideExistingPolygon(Vector2 point, out Polygon result)
    {
        result = null;
        foreach (Polygon polygon in _polygons)
        {
            if (polygon.ContainsPoint(point))
            {
                result = polygon;
                return true;
            }
        }
        return false;
    }
    #endregion
}
