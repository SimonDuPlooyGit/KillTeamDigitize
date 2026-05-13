using System;
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
    private float pathDistance;
    public NavMeshPath path;
    public GameObject unitGhost;
    public LineRenderer lineRenderer;
    private Vector3[] points;
    private bool pathDrawn = false;

    private void Awake()
    {
        agentUnit = GetComponent<NavMeshAgent>();
        agentGhost = unitGhost.GetComponent<NavMeshAgent>();
        input = new InputActions();
        AssignInputs();
        path = new NavMeshPath();
    }

    private void Update()
    {
         if (agentGhost.remainingDistance == 0)
         {
             DrawPath(points);
         }
    }

    void AssignInputs()
    {
        input.Controls.Move.performed += ctx => ClickToPathfind();
    }

    void ClickToPathfind()
    {
        pathDistance = 0f;
        lineRenderer.enabled = false;
        pathDrawn = false;
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (NavMesh.CalculatePath(transform.position, hit.point, agentUnit.areaMask, path))
            {
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);
                    pathDistance += segment;
                    Debug.Log($"Segment {i}: {segment}, Total: {pathDistance}");
                }
            }

            if (pathDistance > movementStat)
            {
                agentGhost.destination = LimitPath(path, movementStat);
            }
            else
            {
               agentGhost.destination = hit.point;
            }

            points = path.corners;
            //DrawPath(points);

        }
    }

    private void DrawPath(Vector3[] points)
    {
        if (pathDrawn == false)
        {
            Vector3 upOffset = new  Vector3(0f, 1f, 0f);
            for (int i = 0; i < points.Length; i++)
            {
                //points[i] += upOffset;
            }
            lineRenderer.SetPositions(points);
            lineRenderer.enabled = true;
            pathDrawn = true;
        }
    }
    
    Vector3 LimitPath(NavMeshPath path, float maxDistance)
    {
        float distance = 0f;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);

            if (distance + segment > maxDistance)
            {
                float remaining = maxDistance - distance;
                Vector3 direction = (path.corners[i + 1] - path.corners[i]).normalized;
                return path.corners[i] + direction * remaining;
            }

            distance += segment;
        }
        
        return path.corners[path.corners.Length - 1];
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
