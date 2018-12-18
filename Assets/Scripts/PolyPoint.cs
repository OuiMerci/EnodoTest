using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyPoint : MonoBehaviour {

    // A vector2 would be enough to store data for points X and Y, but i'd rather use a class for the case I would like to add more features
    #region Fields
    static private int _currentId = 0;
    private int _id; // used for debug
    private float _x;
    private float _y;
    private GameObject _pointDisplay; //The object used to display a point on the map
    private List<Polygon> _linkedPolygons; // Lists polygons using this point, if no polygon use this point anymore, send it back to the pool
    #endregion

    #region properties
    public int ID
    {
        get { return _id; }
    }

    public float X
    {
        get { return _x; }
        set { _x = value; }
    }

    public float Y
    {
        get { return _y; }
        set { _y = value; }
    }

    public Vector2 asVector2
    {
        get { return new Vector2(_x, _y); }
    }

    public GameObject PointDisplay
    {
        get { return _pointDisplay; }
        set { _pointDisplay = value; }
    }

    public List<Polygon> LinkedPolygons
    {
        get { return _linkedPolygons; }
    }

    #endregion

    #region Methods
    private void UpdateDisplayPosition()
    {
        transform.position = new Vector2(_x, _y);
    }

    public void InitPoint(Vector2 point)
    {
        _x = point.x;
        _y = point.y;
        _id = _currentId;
        _currentId++;

        _linkedPolygons = new List<Polygon>();
        UpdateDisplayPosition();
        //Debug.Log("Created point " + _id + " with value : (" + _x + ", " + _y + ")");
    }

    // Currently unused as the player cannot delete a polygon
    public void RemoveLinkedPolygon(Polygon poly)
    {
        _linkedPolygons.Remove(poly);

        CheckLinkedPolygonsCount();
    }

    public void AddLinkedPolygon(Polygon poly)
    {
        if(_linkedPolygons.Contains(poly) == false)
            _linkedPolygons.Add(poly);
    }

    public void CheckLinkedPolygonsCount()
    {
        if (_linkedPolygons.Count < 1)
            Cadastre.Instance.PointsPool.AddToPool(this);
    }
    #endregion
}
