using System;
using UnityEngine;

[Serializable]
public struct PlayerSettings
{
    [Range(0.1f, 50f, order = 1)] public float Speed;

    [Range(0f, 5f)] public float InertiaDuration;
}