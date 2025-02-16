using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(LineRenderer))]
public class ParabolicProjection : MonoBehaviour
{
    [Header("Check 'Enable Test Shoot' and press [Space] in Play Mode")]
    public bool enableTestShoot = false;
    public Rigidbody projectilePrefab;
 
    [Space]
    public float force = 50;
 
    private Vector3[] segments;
    private int numSegments = 0;
    private int maxIterations = 10000;
    private int maxSegmentCount = 300;
    private float segmentStepModulo = 10f;
 
    private Rigidbody currentProjectilePrefab;
    private LineRenderer lineRenderer;
 
    public void ChangeProjectile(Rigidbody newProjectile)
    {
        currentProjectilePrefab = newProjectile;
    }
 
    public void Shoot()
    {
        Rigidbody projectile = Instantiate(currentProjectilePrefab);
        projectilePrefab.transform.position = transform.position;
        projectile.transform.rotation = transform.rotation;
        projectile.linearVelocity = transform.forward * force;
    }
 
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        currentProjectilePrefab = projectilePrefab;
    }
 
 
    private void Update()
    {
        SimulatePath(transform.gameObject, transform.forward * force, currentProjectilePrefab.linearDamping);
 
        if (Input.GetKeyDown(KeyCode.Space) && enableTestShoot)
        {
            Shoot();
        }
    }
 
    private void SimulatePath(GameObject obj, Vector3 forceDirection, float drag)
    {
        float timestep = Time.fixedDeltaTime;
 
        float stepDrag = 1 - drag * timestep;
        Vector3 velocity = forceDirection * timestep;
        Vector3 gravity = Physics.gravity * timestep * timestep;
        Vector3 position = obj.transform.position;
 
        if (segments == null || segments.Length != maxSegmentCount)
        {
            segments = new Vector3[maxSegmentCount];
        }
 
        segments[0] = position;
        numSegments = 1;
 
        for (int i = 0; i < maxIterations && numSegments < maxSegmentCount; i++)
        {
            velocity += gravity;
            velocity *= stepDrag;
 
            position += velocity;
 
            if (i % segmentStepModulo == 0)
            {
                segments[numSegments] = position;
                numSegments++;
            }
        }
 
        Draw();
    }
 
    private void Draw()
    {
        Color startColor = Color.magenta;
        Color endColor = Color.magenta;
        startColor.a = 1f;
        endColor.a = 1f;
 
        lineRenderer.transform.position = segments[0];
 
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
 
        lineRenderer.positionCount = numSegments;
        for (int i = 0; i < numSegments; i++)
        {
            lineRenderer.SetPosition(i, segments[i]);
        }
    }
}