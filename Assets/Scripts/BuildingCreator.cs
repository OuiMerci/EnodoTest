using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCreator : MonoBehaviour {

    #region Fields
    public enum ConstructionType
    {
        White, Orange, Red, Enemy, AddSize
    }
    private ConstructionType _currentType;
    static private BuildingCreator _instance;

    // Events
    private delegate void Buildingdestroyed(Building destroyedBuilding);
    private event Buildingdestroyed OnBuildingDestroyed;

    // Building pool
    [SerializeField] private Building _buildingPrefab;
    [SerializeField] private Transform _buildingsParent;
    [SerializeField] private Transform _pooledBuildingsParent;
    [SerializeField] private int _buildingPoolStartCount;
    [SerializeField] private int _buildingPoolMaxCount;
    [SerializeField] List<BuildingTemplate> _templates; // The templates must be in the same order as the enum !

    private Pool<Building> _buildingPool;
    private List<Building> _buildingList;
    private List<Building> _enemiesList;
    private float _buildingGrowSpeed;

    #endregion

    #region properties
    static public BuildingCreator Instance
    {
        get { return _instance; }
    }
    #endregion

    #region Methods
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        _currentType = ConstructionType.White;
        _buildingList = new List<Building>();
        _buildingPool = new Pool<Building>(_buildingPrefab, _buildingPoolStartCount, _buildingPoolMaxCount, _pooledBuildingsParent, _buildingsParent);
    }

    public void TryCreateNewBuilding(Vector2 position, Polygon polygon)
    {
        if (polygon.LinkedBuilding != null)
        {
            Debug.LogWarning("Zone " + polygon.ID + " already has a building");
            return;
        }

        BuildingTemplate template;

        switch(_currentType)
        {
            case ConstructionType.AddSize:
                Debug.LogWarning("Cannot create a building in AddSize mode.");
                return;

            default:
                template = _templates[(int)_currentType];
                break;
        }

        Building newBuilding = _buildingPool.GetFromQueue();
        newBuilding.InitBuilding(position, polygon, template); // Initialise the building according to the template

        _buildingList.Add(newBuilding);
    }

    public void DestroyBuilding(Building building)
    {
        _buildingList.Remove(building);
        _buildingPool.AddToPool(building);

        if (OnBuildingDestroyed != null)
            OnBuildingDestroyed(building);
    }

    public void SetConstructionType(ConstructionType type)
    {
        _currentType = type;
        Debug.Log("Swapping to " + type);
    }

    public void TryAddSize(Polygon polygon)
    {
        if(_currentType == ConstructionType.AddSize)
        {
            polygon.BuildingGrow();
        }
    }
    #endregion
}
