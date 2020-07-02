using System;
using UnityEngine;

[Serializable]
public struct CoinSettings
{
    [Range(1, 20)] public int ScoreToSpawn;
    [Range(0.5f, 20f)] public float Lifetime;
    [Range(0f, 5f)] public float AnimationDuration;
    public GameObject CoinPrefab;
}