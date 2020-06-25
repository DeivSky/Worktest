using UnityEngine;

public class LevelBounds
{
    public Vector2 MaxBound => maxBound.position;
    public Vector2 MinBound => minBound.position;

    public float LeftBound => minBound.position.x;
    public float RightBound => maxBound.position.x;

    private readonly Transform minBound;
    private readonly Transform maxBound;

    public LevelBounds(Transform minBound, Transform maxBound)
    {
        this.minBound = minBound;
        this.maxBound = maxBound;
    }
}
