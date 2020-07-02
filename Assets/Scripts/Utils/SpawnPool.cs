using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase para manejo de Queues de GameObjects.
/// </summary>
public class SpawnPool : IEnumerable<KeyValuePair<int, GameObject>>
{
    public int TotalDequeued { get; private set; } = 0;
    public int QueueCount => queueKeys.Count;
    public int Capacity { get; set; }
    public GameObject Prefab { get; set; }

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

    public GameObject[] AllObjects
    {
        get
        {
            var all = new GameObject[dictionary.Count];
            dictionary.Values.CopyTo(all, 0);
            return all;
        }
    }

    public void GetObjectsFromKeys(int[] keys, GameObject[] gameObjects)
    {
        for (int i = 0; i < keys.Length; i++)
            gameObjects[i] = dictionary[keys[i]];
    }

    public int GetQueuedKeys(int[] keys)
    {
        var queue = queueKeys.ToArray();
        int i = 0;
        while (i < queue.Length)
        {
            keys[i] = queue[i];
            i++;
        }

        return i;
    }

    public int GetActiveKeys(int[] keys)
    {
        TrackInactiveAndDestroyed();
        int i = 0;
        while (i < activeKeys.Count)
        {
            keys[i] = activeKeys[i];
            i++;
        }

        return i;
    }

    protected readonly Dictionary<int, GameObject> dictionary = null;
    protected readonly Queue<int> queueKeys = null;
    protected readonly List<int> activeKeys = null;
    protected readonly List<int> inactiveKeys = null;
    protected int current = 0;

    public SpawnPool(int capacity, GameObject prefab)
    {
        Capacity = capacity;
        Prefab = prefab;
        queueKeys = new Queue<int>(Capacity);
        activeKeys = new List<int>(Capacity);
        inactiveKeys = new List<int>(Capacity);
        dictionary = new Dictionary<int, GameObject>(Capacity);
    }

    public IEnumerator EnqueueAllCoroutine(int batchSize, YieldInstruction wait = null)
    {
        int count = Capacity - current;

        if (count <= 0) yield break;
        if (batchSize <= 0) batchSize = 1;
        if (batchSize > count) batchSize = count;

        int rounds = Mathf.CeilToInt((float) count / batchSize);
        int mod = count % batchSize;
        for (int i = 1; i <= rounds; i++)
        {
            int j = i < rounds ? batchSize : mod;
            Enqueue(j);
            yield return wait;
        }
    }

    public void EnqueueAll()
    {
        int count = Capacity - current;
        if (count <= 0) return;
        Enqueue(count);
    }

    public void Enqueue(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            if (current > Capacity) Capacity++;
            var go = GameObject.Instantiate(Prefab);
            go.SetActive(false);
            queueKeys.Enqueue(current);
            dictionary.Add(current, go);
            current++;
        }
    }

    public GameObject Peek() => queueKeys.Count == 0 ? null : dictionary[queueKeys.Peek()];

    public GameObject Dequeue()
    {
        if (queueKeys.Count == 0) return null;

        TotalDequeued++;
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
        TrackInactiveAndDestroyed();
        for (int i = 0; i < inactiveKeys.Count; i++)
            queueKeys.Enqueue(inactiveKeys[i]);

        inactiveKeys.Clear();
    }

    public IEnumerator<KeyValuePair<int, GameObject>> GetEnumerator() => dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}