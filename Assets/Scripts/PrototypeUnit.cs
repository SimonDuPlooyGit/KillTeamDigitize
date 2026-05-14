using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class PrototypeUnit : MonoBehaviour
{
    private InputActions input;
    private NavMeshAgent agentUnit;
    private NavMeshAgent agentGhost;
    public float movementStat;
    private float meterMovement;
    private float pathDistance;
    public NavMeshPath path;
    private NavMeshPath limitedUnitPath;
    public GameObject unitGhost;
    public LineRenderer lineRenderer;
    private Vector3[] points;
    private bool pathDrawn = false;
    List<Vector3> limitedPoints = new List<Vector3>();
    public bool selected = false;

    private void Awake()
    {
        meterMovement = (movementStat/39.37f) * 10; //Changing the inches to meters and then applying 10x Scale.
        agentUnit = GetComponent<NavMeshAgent>();
        agentGhost = unitGhost.GetComponent<NavMeshAgent>();
        input = new InputActions();
        AssignInputs();
        path = new NavMeshPath();
    }

    private void Update()
    {
         if (!pathDrawn &&
             !agentGhost.pathPending &&
             agentGhost.velocity.sqrMagnitude < 0.01f &&
             agentGhost.remainingDistance <= agentGhost.stoppingDistance &&
             selected == true)
         {
             DrawPath(limitedPoints.ToArray());
         }
    }

    void AssignInputs()
    {
        input.Controls.Move.performed += ctx => ClickToPathfind();
    }

    private void ClickToPathfind()
    {
        limitedPoints.Clear();
        pathDistance = 0f;
        lineRenderer.enabled = false;
        pathDrawn = false;
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100) && selected == true)
        {
            if (NavMesh.CalculatePath(transform.position, hit.point, agentUnit.areaMask, path))
            {
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);
                    pathDistance += segment;
                    //Debug.Log($"Segment {i}: {segment}, Total: {pathDistance}");
                }
            }

            if (pathDistance > meterMovement)
            {
                agentGhost.destination = LimitPath(path, meterMovement);
            }
            else
            {
                limitedPoints.Add(transform.position);
                limitedPoints.Add(hit.point);
                agentGhost.destination = hit.point;
            }

            points = path.corners;
        }
    }

    private void DrawPath(Vector3[] points)
    {
        if (pathDrawn == false)
        {
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
            lineRenderer.enabled = true;
            pathDrawn = true;
        }
    }
    
    Vector3 LimitPath(NavMeshPath path, float maxDistance)
    {
        float distance = 0f;

        limitedPoints.Add(path.corners[0]);

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);

            if (distance + segment > maxDistance)
            {
                float remaining = maxDistance - distance;

                Vector3 direction = (path.corners[i + 1] - path.corners[i]).normalized;

                Vector3 finalPoint = path.corners[i] + direction * remaining;

                limitedPoints.Add(finalPoint);

                return finalPoint;
            }

            limitedPoints.Add(path.corners[i + 1]);

            distance += segment;
        }

        return path.corners[path.corners.Length - 1];
    }

    public void moveUnitToGhost()
    {
        if (selected == true)
        {
            Vector3[] unitPoints = limitedPoints.ToArray();
            limitedUnitPath = new NavMeshPath();
            for (int i = 0; i < unitPoints.Length; i++)
            {
                limitedUnitPath.corners[i] = unitPoints[i];
            }
            agentUnit.SetPath(limitedUnitPath);
        }
    }
    

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }
}
