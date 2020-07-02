using System;
using UnityEngine;

[Serializable]
public struct RockSettings
{
    [Range(1, 100)] public int RockCount;
    [MinMaxRange("Min", 0.5f, "Max", 10f)] public MinMax TimeBetweenThrows;
    [MinMaxRange("Min", 0.1f, "Max", 50f)] public MinMax RockVelocity;
    [MinMaxRange("Min", 0f, "Max", 90f)] public MinMax ThrowAngle;
    [Range(-0.1f, -100f)] public float Gravity;
    public Vector2 BounceMultiplier;
    public GameObject RockPrefab;
}