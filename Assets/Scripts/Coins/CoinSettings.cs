using System;
using UnityEngine;

[Serializable]
public struct CoinSettings
{
    public int ScoreToSpawn;
    public float Lifetime;
    public float AnimationDuration;
    public GameObject CoinPrefab;
}
