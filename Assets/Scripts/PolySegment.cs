using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolySegment {

    // A tupple would be enough to store data for points A and B, but i'd rather use a class for the case I would like to add more features
    #region Fields
    enum Orientation
    {
        Clockwise, CounterClockwise, Collinear, Unknown
    }

    static private int _currentID = 0;
    private int _id; // used for debug
    private PolyPoint _a;
    private PolyPoint _b;
    #endregion

    #region Properties
    public int ID
    {
        get { return _id; }
    }

    public PolyPoint A
    {
        get { return _a; }
        //set { _a = value; }
    }

    public PolyPoint B
    {
        get { return _b; }
        //set { _b = value; }
    }
    #endregion
    
    #region Methods
    public PolySegment(PolyPoint a, PolyPoint b)
    {
        _a = a;
        _b = b;
        _id = _currentID;

        _currentID++;
        //Debug.Log("Created segment " + _id + " with points " + _a.ID + " and " + _b.ID);
    }

    private Orientation ComputeOrientation(Vector2 u, Vector2 v, Vector2 w, out bool isCollinear)
    {
        isCollinear = false;

        // Compute the cross product of vectors UV and VW to determine their orientation
        float crossProduct = (v.y - u.y) * (w.x - v.x) - (v.x - u.x) * (w.y - v.y);

        if (crossProduct == 0)
        {
            isCollinear = true;
            return Orientation.Collinear;
        }
        else if (crossProduct < 0)
        {
            isCollinear = false;
            return Orientation.CounterClockwise;
        }
        else //if (crossProduct > 0)
        {
            isCollinear = false;
            return Orientation.Clockwise;
        }
    }

    /// <summary>
    /// Check if this segment is intersecting with another segment CD.
    /// </summary>
    /// <param name="c">Other segment's point c</param>
    /// <param name="d">Other segment's point d</param>
    /// <returns>Returns true if the two segment are intersecting.</returns>
    public bool IsInterecting(Vector2 c, Vector2 d)
    {
        bool isCollinear; //If a point is collinear to the other segment, we don't assume the segment actually intersect, even if they have one point in common.

        Orientation orientABC = ComputeOrientation(_a.asVector2, _b.asVector2, c, out isCollinear);
        if (isCollinear) { return false; }

        Orientation orientABD = ComputeOrientation(_a.asVector2, _b.asVector2, d, out isCollinear);
        if (isCollinear) { return false; }

        Orientation orientCDA = ComputeOrientation(c, d, _a.asVector2, out isCollinear);
        if (isCollinear) { return false; }

        Orientation orientCDB = ComputeOrientation(c, d, _b.asVector2, out isCollinear);
        if (isCollinear) { return false; }

        // Compare the orientations to determine if there is an intersection
        if (orientABC != orientABD && orientCDA != orientCDB)
        {
            //Debug.Log("Intersection with segment " + _id);
            return true;
        }
        else
        {
            //Debug.Log("No intersection with segment " + _id);
            return false;
        }
    }
    #endregion
}
