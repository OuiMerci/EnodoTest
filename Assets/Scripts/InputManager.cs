using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    #region Fields
    private const int RAYCAST_MAXDISTANCE = 200;
    private DrawingTool _drawingTool;
    private Cadastre _cadastre;
    private Polygon _selectedZone;
    private CameraMovement _cam;
    #endregion

    #region Methods
    private void Start()
    {
        _drawingTool = DrawingTool.Instance;
        _cadastre = Cadastre.Instance;
        _cam = Camera.main.GetComponent<CameraMovement>();
    }

    private void Update()
    {
        CheckMouseInput();
        CheckResetInput();
        CheckConstructionTypeInput();
        CheckBuildingSizeInput();
        CheckCameraInput();
    }

    private void CheckResetInput()
    {
        if (Input.GetButtonDown("Reset"))
        {
            _drawingTool.ResetDrawingTool();
        }
    }

    private void CheckMouseInput()
    {
        if(Input.GetMouseButtonDown(0))
        {
            MouseRaycast();
        }
    }

    private void CheckBuildingSizeInput()
    {
        if (Input.GetButton("AddSize") && _selectedZone != null)
        {
            BuildingCreator.Instance.TryAddSize(_selectedZone);
        }
    }

    private void CheckConstructionTypeInput()
    {
        if (Input.GetButtonDown("CTWhite"))
            BuildingCreator.Instance.SetConstructionType(BuildingCreator.ConstructionType.White);
        else if (Input.GetButtonDown("CTOrange"))
            BuildingCreator.Instance.SetConstructionType(BuildingCreator.ConstructionType.Orange);
        else if (Input.GetButtonDown("CTRed"))
            BuildingCreator.Instance.SetConstructionType(BuildingCreator.ConstructionType.Red);
        else if (Input.GetButtonDown("CTEnemy"))
            BuildingCreator.Instance.SetConstructionType(BuildingCreator.ConstructionType.Enemy);
        else if (Input.GetButtonDown("CTSize"))
            BuildingCreator.Instance.SetConstructionType(BuildingCreator.ConstructionType.AddSize);
    }

    private void CheckCameraInput()
    {
        if(Input.GetButtonDown("CameraSwap"))
        {
            _cam.SwapView();
        }
        else
        {
            float moveH = - Input.GetAxis("Horizontal"); // the camera is turn by 180° on the y axis, so we invert the horizontal input
            float moveV = Input.GetAxis("Vertical");

            _cam.Move(new Vector2(moveH, moveV));
        }
    }

    /// CONSTRUIRE Là OU ON CLICK !!!
    private void MouseRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
        {
            // Did we click inside a polygon
            Polygon hitPolygon = null;
            if (_cadastre.IsPointInsideExistingPolygon(hit.point, out hitPolygon))
            {
                _selectedZone = hitPolygon;
                BuildingCreator.Instance.TryCreateNewBuilding(hit.point, hitPolygon);
            }
            else
            {
                _selectedZone = null;
                _drawingTool.HandleNewClick(hit.point);
            }
        }
    }
    #endregion
}
