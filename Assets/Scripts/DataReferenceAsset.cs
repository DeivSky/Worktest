using UnityEngine;


public class DataReferenceAsset<T> : ScriptableObject where T : struct
{
    public T Value;
}
