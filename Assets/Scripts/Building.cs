using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    #region Fields
    private Polygon _parentPolygon;
    private Renderer _rend;
    private BuildingCreator.ConstructionType _type;

    // Stats
    private float _growingSpeed;
    private float _resistance;
    private float _attackDamage;
    private bool _isEnemy;
    #endregion

    #region properties
    public Polygon ParentPolygon
    {
        get { return _parentPolygon; }
    }
    #endregion

    #region Methods
    private void Update()
    {
        if (_isEnemy)
            Attack();
    }

    private void Attack()
    {
        foreach(Polygon poly in _parentPolygon.ConnectedPolygons)
        {
            if(poly.LinkedBuilding != null)
                poly.LinkedBuilding.HandleEnemyAttack(_attackDamage);
        }
    }

    private void SetMaterial(Material mat)
    {
        if (_rend == null)
            _rend = GetComponent<MeshRenderer>();

        _rend.material = mat;
    }

    private void UpdateZ()
    {
        float newZ = transform.localScale.z / 2.0f;
        transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
    }

    private void DestroyBuilding()
    {
        _parentPolygon.LinkedBuilding = null;
        BuildingCreator.Instance.DestroyBuilding(this);
    }

    private void UpdateSize(float difference)
    {
        transform.localScale += new Vector3(0, 0, difference);
        UpdateZ();

        if (transform.localScale.z <= 0)
            DestroyBuilding();

    }

    public void Grow()
    {
        float difference = _growingSpeed * Time.deltaTime;
        UpdateSize(difference);
    }

    public void HandleEnemyAttack(float attackForce)
    {
        if (_isEnemy)
            return;

        float difference = Mathf.Abs(attackForce - _resistance) * Time.deltaTime;
        UpdateSize(-difference);
    }

    public void InitBuilding(Vector2 position, Polygon polygon, BuildingTemplate template)
    {
        // Place the building on the X and Y axis, then update his Z position
        transform.position = position;
        UpdateZ();

        // Link this building to its polygon
        _parentPolygon = polygon;
        _parentPolygon.LinkedBuilding = this;

        // Copy templates properties
        SetMaterial(template.Mat);
        _growingSpeed = template.GrowSpeed;
        _resistance = template.Resistance;
        _attackDamage = template.AttackDamage;
        _isEnemy = template.IsEnemy;
    }
    #endregion
}
