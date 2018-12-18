using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool<T> where T : MonoBehaviour
{
    #region Fields
    private Queue<T> _objectQueue = null;
    private T _prefab = null;
    private Transform _pooledObjectsParent = null;
    private Transform _releasedObjectsParent = null;
    private int _startCount = 0;
    private int _maxCount = 0;
    private int _currentCount = 0;
    #endregion Fields

    #region Private Methods
    private void InitializeQueue()
    {
        for(int i = 0; i < _startCount; i++)
        {
            CreateNewObject();
        }
    }

    /// <summary>
    /// If the limit is not reached, instantiate a new object for the pool and return true. Else, it doesn't instantiate anything and returns false.
    /// </summary>
    /// <returns></returns>
    private bool CreateNewObject()
    {
        if (_currentCount < _maxCount)
        {
            T newObject = GameObject.Instantiate<T>(_prefab, _pooledObjectsParent.position, _pooledObjectsParent.rotation);
            newObject.name = _prefab.name;
            AddToPool(newObject);
            _currentCount++;
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion Private Methods

    #region Public Methods
    public Pool(T prefab, int startCount, int maxCount, Transform pooledObjectsParent, Transform releasedObjectsParent = null)
    {
        _objectQueue = new Queue<T>();
        _prefab = prefab;
        _startCount = startCount;
        _maxCount = maxCount;
        _pooledObjectsParent = pooledObjectsParent;
        _releasedObjectsParent = releasedObjectsParent;

        InitializeQueue();
    }

    public void AddToPool(T obj)
    {
        obj.transform.position = _pooledObjectsParent.transform.position;
        obj.transform.parent = _pooledObjectsParent;
        obj.gameObject.SetActive(false);
        _objectQueue.Enqueue(obj);
    }

    public T GetFromQueue()
    {
        T newObject = null;
        if (_objectQueue.Count == 0)
        {
            bool objectCreated = CreateNewObject();

            if (objectCreated == false)
            {
                Debug.LogError("Pool : The pool of " + _prefab.name + " has no more objects to provide and has reached its maximum of intantiation.");
                return default(T);
            }
        }

        newObject = _objectQueue.Dequeue();
        newObject.transform.parent = _releasedObjectsParent;
        newObject.gameObject.SetActive(true);

        return newObject;
    }
    #endregion Public Methods
}
