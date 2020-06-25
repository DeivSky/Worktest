using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPool
{
    public int TotalSpawned { get; private set; } = 0;
    public int PoolCount => pool.Count;
    public int Capacity { get; set; }
    public GameObject Prefab { get; protected set; }

    public GameObject[] PooledObjects
    {
        get
        {
            var pooled = new GameObject[PoolCount];
            var dict = pool.ToArray();
            for (int i = 0; i < PoolCount; i++)
                pooled[i] = dict[i].Value;

            return pooled;
        }
    }

    public GameObject[] ActiveObjects
    {
        get
        {
            TrackInactiveAndDestroyed();
            
            var actives = new GameObject[activeObjects.Count];
            activeObjects.Values.CopyTo(actives, 0);
            return actives;
        }
    }

    protected readonly Queue<KeyValuePair<int, GameObject>> pool = null;
    protected readonly Dictionary<int, GameObject> activeObjects = null;
    protected readonly Dictionary<int, GameObject> inactiveObjects = null;
    protected int current = 0;

    public SpawnPool(int capacity, GameObject prefab)
    {
        Capacity = capacity;
        Prefab = prefab;
        pool = new Queue<KeyValuePair<int, GameObject>>(Capacity);
        activeObjects = new Dictionary<int, GameObject>(Capacity);
        inactiveObjects = new Dictionary<int, GameObject>(Capacity);
    }

    public IEnumerator InstantiateAllCoroutine(int batchSize, Action<GameObject> action = null, YieldInstruction wait = null)
    {
        int count = Capacity - PoolCount;
        int rounds = Mathf.CeilToInt((float) count / batchSize);
        int mod = count % batchSize;
        for (int i = 1; i <= rounds; i++)
        {
            int j = i < rounds ? batchSize : mod;
            Instantiate(j, action);
            yield return wait;
        }
    }
    
    public void InstantiateAll(Action<GameObject> action = null)
    {
        int count = Capacity - PoolCount;
        Instantiate(count, action);
    }

    public void Instantiate(int count = 1, Action<GameObject> action = null)
    {
        for (int i = 0; i < count; i++)
        {
            var go = GameObject.Instantiate(Prefab);
            go.SetActive(false);
            pool.Enqueue(new KeyValuePair<int, GameObject>(current++, go));
            action?.Invoke(go);
        }
    }
    
    public GameObject Spawn()
    {
        if (pool.Count == 0) return null;
        
        var keyValuePair = pool.Dequeue();
        activeObjects.Add(keyValuePair.Key, keyValuePair.Value);
        keyValuePair.Value.SetActive(true);
        return keyValuePair.Value;
    }

    protected void TrackInactiveAndDestroyed()
    {
        foreach (var active in activeObjects)
        {
            if (active.Value == null)
            {
                activeObjects.Remove(active.Key);
            }
            else if (!active.Value.activeSelf)
            {
                activeObjects.Remove(active.Key);
                inactiveObjects.Add(active.Key, active.Value);
            }
        }
    }

    public void RequeueInactive()
    {
        foreach (var inactive in inactiveObjects)
            pool.Enqueue(inactive);
        
        inactiveObjects.Clear();
    }
    
    protected virtual void OnInstantiate(GameObject gameObject) { }
}

public class SpawnPool<T> : SpawnPool where T : MonoBehaviour
{
    public T[] ActiveComponents
    {
        get
        {
            TrackInactiveAndDestroyed();
            
            var comps = new T[activeObjects.Count];
            int i = 0;
            foreach (int key in activeObjects.Keys)
                comps[i++] = components[key];

            return comps;
        }
    }
    
    protected readonly Dictionary<int, T> components = null;

    public SpawnPool(int capacity, GameObject prefab) : base(capacity, prefab) =>
        components = new Dictionary<int, T>(Capacity);

    protected override void OnInstantiate(GameObject gameObject) =>
        components.Add(current, gameObject.GetComponent<T>());
}
