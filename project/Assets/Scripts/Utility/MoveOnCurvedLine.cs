using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnCurvedLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject objectToMove;
    public float speed;

    private Vector3[] positions = new Vector3[397];
    private Vector3[] pos;
    private int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        pos = GetLinePointsInWorldSpace();
        objectToMove.transform.position = pos[index];
    }

    Vector3[] GetLinePointsInWorldSpace()
    {
        //Get the positions which are shown in the inspector 
        lineRenderer.GetPositions(positions);
        
        //the points returned are in world space
        return positions;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position,
            pos[index],
            speed * Time.deltaTime);

        if (objectToMove.transform.position == pos[index])
        {
            index += 1;
        }

        if (index == pos.Length)
            index = 0;
    }
}