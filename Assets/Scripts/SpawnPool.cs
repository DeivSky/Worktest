using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPool : IEnumerable<KeyValuePair<int, GameObject>>
{
    public int TotalSpawned { get; private set; } = 0;
    public int QueueCount => queueKeys.Count;
    public int Capacity { get; set; }
    public GameObject Prefab { get; protected set; }

    public GameObject[] QueuedObjects
    {
        get
        {
            int count = QueueCount;
            var queued = new GameObject[count];
            var queueArray = queueKeys.ToArray();
            
            for (int i = 0; i < count; i++)
                queued[i] = dictionary[queueArray[i]];
    
            return queued;
        }
    }

    public int[] ActiveKeys => activeKeys.ToArray();
    public GameObject[] ActiveObjects
    {
        get
        {
            TrackInactiveAndDestroyed();
            
            var actives = new GameObject[activeKeys.Count];
            for (int i = 0; i < activeKeys.Count; i++)
                actives[i] = dictionary[activeKeys[i]];
            
            return actives;
        }
    }

    public GameObject[] AllObjects
    {
        get
        {
            var all = new GameObject[dictionary.Count];
            dictionary.Values.CopyTo(all, 0);
            return all;
        }
    }

    protected readonly Queue<int> queueKeys = null;
    protected readonly List<int> activeKeys = null;
    protected readonly List<int> inactiveKeys = null;
    protected readonly Dictionary<int, GameObject> dictionary = null;
    protected int Current = -1;

    public SpawnPool(int capacity, GameObject prefab)
    {
        Capacity = capacity;
        Prefab = prefab;
        queueKeys = new Queue<int>(Capacity);
        activeKeys = new List<int>(Capacity);
        inactiveKeys = new List<int>(Capacity);
        dictionary = new Dictionary<int, GameObject>(Capacity);
    }

    public IEnumerator InstantiateAllCoroutine(int batchSize, YieldInstruction wait = null)
    {
        int count = Capacity - Current;
        int rounds = Mathf.CeilToInt((float) count / batchSize);
        int mod = count % batchSize;
        for (int i = 1; i <= rounds; i++)
        {
            int j = i < rounds ? batchSize : mod;
            Instantiate(j);
            yield return wait;
        }
    }
    
    public void InstantiateAll()
    {
        int count = Capacity - Current;
        if (count <= 0) return;
        Instantiate(count);
    }

    public void Instantiate(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Current++;
            if (Current > Capacity) Capacity++;
            var go = GameObject.Instantiate(Prefab);
            go.SetActive(false);
            queueKeys.Enqueue(Current);
            dictionary.Add(Current, go);
        }
    }
    
    public GameObject Dequeue()
    {
        if (dictionary.Count == 0) return null;
        
        int i = queueKeys.Dequeue();
        var go = dictionary[i];
        activeKeys.Add(i);
        return go;
    }
    
    protected void TrackInactiveAndDestroyed()
    {
        for (int i = 0; i < activeKeys.Count; i++)
        {
            int key = activeKeys[i];
            var go = dictionary[key];
            if (go == null)
            {
                activeKeys.RemoveAt(i);
            }
            else if (!go.activeSelf)
            {
                activeKeys.RemoveAt(i);
                inactiveKeys.Add(key);
            }
        }
    }

    public void RequeueInactive()
    {
        for (int i = 0; i < inactiveKeys.Count; i++)
            queueKeys.Enqueue(inactiveKeys[i]);

        inactiveKeys.Clear();
    }

    public IEnumerator<KeyValuePair<int, GameObject>> GetEnumerator() => dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
