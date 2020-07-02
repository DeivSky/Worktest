using System;

/// <summary>
/// Clase genérica para pasar referencias de estructuras.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class DataReference<T> where T : struct
{
    public T Value;

    public DataReference()
    {
    }

    public DataReference(T value) => Value = value;

    public static implicit operator T(DataReference<T> reference) => reference.Value;
}