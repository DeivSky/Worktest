using UnityEngine;

public class LevelBounds
{
    public float LeftBound => leftBound.position.x;
    public float RightBound => rightBound.position.x;

    private readonly Transform leftBound;
    private readonly Transform rightBound;

    public LevelBounds(Transform leftBound, Transform rightBound)
    {
        this.leftBound = leftBound;
        this.rightBound = rightBound;
    }
}
