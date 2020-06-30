using System;
using UnityEngine;

[Serializable]
public class DataReference<T> where T : struct
{
    public T Value;
    
    public DataReference() {}
    public DataReference(T value) => Value = value;

    public static implicit operator T(DataReference<T> reference) => reference.Value;
}
