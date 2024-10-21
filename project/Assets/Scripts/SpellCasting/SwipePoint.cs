using UnityEngine;

public class SwipePoint
{
    public Vector2 point;
    public float timestamp;

    public SwipePoint(Vector2 point, float timestamp)
    {
        this.point = point;
        this.timestamp = timestamp;
    }
}
