using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct RockSettings
{
    public int RockCount;
    [FormerlySerializedAs("TimeBetweenThrows")] public float MinTimeBetweenThrows;
    public float MaxTimeBetweenThrows;
    public float MinRockVelocity;
    public float MaxRockVelocity;
    public float MinThrowAngle;
    public float MaxThrowAngle;
    public float Gravity;
    public Vector2 BounceMultiplier;
    public GameObject RockPrefab;
}
