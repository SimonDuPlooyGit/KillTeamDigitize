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
    NavMeshAgent agent;
    public float movementStat;
    private float pathDistance;
    public NavMeshPath path;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        input = new InputActions();
        AssignInputs();
        path = new NavMeshPath();
    }

    void AssignInputs()
    {
        input.Controls.Move.performed += ctx => ClickToPathfind();
    }

    void ClickToPathfind()
    {
        pathDistance = 0f;
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (NavMesh.CalculatePath(transform.position, hit.point, agent.areaMask, path))
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
                agent.destination = LimitPath(path, movementStat);
            }
            else
            {
               agent.destination = hit.point;
            }
            
            //agent.destination = hit.point;
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
