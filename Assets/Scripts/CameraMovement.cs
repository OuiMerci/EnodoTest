using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    #region Fields
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _3dYRotation;
    [SerializeField] private Vector3 _2dYRotation;

    private bool _is3D;
    #endregion

    #region properties

    #endregion

    #region Methods
    public void SwapView()
    {
        transform.eulerAngles = (_is3D) ? _3dYRotation : _2dYRotation;
        _is3D = !_is3D;
    }

    public void Move(Vector3 direction)
    {
        transform.position += direction * (_speed * Time.deltaTime);
    }
    #endregion
}
