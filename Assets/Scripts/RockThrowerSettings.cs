using System;
using UnityEngine;

[Serializable]
public struct RockThrowerSettings
{
    public float TimeBetweenThrows;
    public int RockCount;
    public float MinRockVelocity;
    public float MaxRockVelocity;
    public float MinThrowAngle;
    public float MaxThrowAngle;
    public GameObject RockPrefab;
}
