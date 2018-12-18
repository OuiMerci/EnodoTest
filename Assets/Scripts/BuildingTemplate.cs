using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Building")]
public class BuildingTemplate : ScriptableObject {

    #region Fiels
    [SerializeField] private Material _mat;
    [SerializeField] private float _growSpeed;
    [SerializeField] private float _resistance;
    [SerializeField] private bool _isEnemy;
    [SerializeField] private float _attackDamage;
    #endregion

    #region Properties
    public Material Mat
    {
        get { return _mat; }
    }

    public float GrowSpeed
    {
        get { return _growSpeed; }
    }

    public float Resistance
    {
        get { return _resistance; }
    }

    public float AttackDamage
    {
        get { return _attackDamage; }
    }

    public bool IsEnemy
    {
        get { return _isEnemy; }
    }
    #endregion
}
