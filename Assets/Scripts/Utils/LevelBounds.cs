using UnityEngine;

public class LevelBounds
{
    public Vector2 Max => maxBound.position;
    public Vector2 Min => minBound.position;

    private readonly Transform minBound;
    private readonly Transform maxBound;

    public LevelBounds(Transform minBound, Transform maxBound)
    {
        this.minBound = minBound;
        this.maxBound = maxBound;
    }
}
