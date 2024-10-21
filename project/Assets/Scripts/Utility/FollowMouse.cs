using System;
using UnityEngine;
using UnityEngine.UI;

public class FollowMouse : MonoBehaviour
{
    public Canvas canvas; // the canvas that contains the UI element
    public RectTransform element; // the UI element to be moved
    public Image sprite; // the UI element to be moved

    public Vector2 MousePosition;

    private void Awake()
    {
        sprite.color = Color.clear;
    }

    private void Update()
    {
        // check if the left mouse button is down
        if (Input.GetMouseButton(0))
        {
            // get the mouse position in screen space
            var mousePosition = Input.mousePosition;

            // convert the mouse position to canvas space
            var canvasMousePosition = canvas.worldCamera.ScreenToWorldPoint(mousePosition);

            // set the position of the UI element to the mouse position
            element.position = canvasMousePosition;

            MousePosition = canvasMousePosition;
        }

        if (Input.GetMouseButtonDown(0))
        {
            sprite.color = Color.white;
        }

        if (Input.GetMouseButtonUp(0))
        {
            sprite.color = Color.clear;
        }
    }
}