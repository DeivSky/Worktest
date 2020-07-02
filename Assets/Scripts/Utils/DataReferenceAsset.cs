using UnityEngine;

/// <summary>
/// Clase genérica para generar Assets a partir de estructuras.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataReferenceAsset<T> : ScriptableObject where T : struct
{
    public T Value;
}