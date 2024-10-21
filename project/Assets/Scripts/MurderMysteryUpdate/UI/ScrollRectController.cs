using UnityEngine;
using UnityEngine.UI;

public class ScrollRectController : MonoBehaviour
{
    public ScrollRect firstScrollRect;
    public ScrollRect secondScrollRect;
    
    public bool syncHorizontal = true;
    public bool syncVertical = true;

    void Start()
    {
        // Add a listener to the first ScrollRect's onValueChanged event
        firstScrollRect.onValueChanged.AddListener(OnFirstScrollRectValueChanged);
    }

    void OnFirstScrollRectValueChanged(Vector2 value)
    {
        // Update the verticalNormalizedPosition of the second ScrollRect
        // based on the verticalNormalizedPosition of the first ScrollRect
        var verticalPosition = value.y;
        var horizontalPosition = value.x;
        
        secondScrollRect.verticalNormalizedPosition = syncVertical ? verticalPosition : secondScrollRect.verticalNormalizedPosition;
        secondScrollRect.horizontalNormalizedPosition = syncHorizontal ? horizontalPosition : secondScrollRect.horizontalNormalizedPosition;
    }
}