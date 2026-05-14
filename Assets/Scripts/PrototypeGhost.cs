using System;
using System.Numerics;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class PrototypeGhost : MonoBehaviour
{
    // private InputActions input;
    // private NavMeshAgent agentUnit;
    // private float movementStat;
    // private float pathDistance;
    // public NavMeshPath path;
    // private Vector3[] points;

    // private void Awake()
    // {
    //     agentUnit = GetComponent<NavMeshAgent>();
    //     input = new InputActions();
    //     //AssignInputs();
    //     path = new NavMeshPath();
    //     movementStat = GetComponentInParent<PrototypeUnit>().movementStat;
    // }

    // void AssignInputs()
    // {
    //     input.Controls.Move.performed += ctx => ClickToPathfind();
    // }
    //
    // void ClickToPathfind()
    // {
    //     pathDistance = 0f;
    //     
    //     RaycastHit hit;
    //     if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
    //     {
    //         if (NavMesh.CalculatePath(transform.position, hit.point, agentUnit.areaMask, path))
    //         {
    //             for (int i = 0; i < path.corners.Length - 1; i++)
    //             {
    //                 float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);
    //                 pathDistance += segment;
    //                 //Debug.Log($"Segment {i}: {segment}, Total: {pathDistance}");
    //             }
    //         }
    //
    //         points = path.corners;
    //     }
    // }
    //
    // Vector3 LimitPath(NavMeshPath path, float maxDistance)
    // {
    //     float distance = 0f;
    //
    //     for (int i = 0; i < path.corners.Length - 1; i++)
    //     {
    //         float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);
    //
    //         if (distance + segment > maxDistance)
    //         {
    //             float remaining = maxDistance - distance;
    //             Vector3 direction = (path.corners[i + 1] - path.corners[i]).normalized;
    //             return path.corners[i] + direction * remaining;
    //         }
    //
    //         distance += segment;
    //     }
    //     
    //     return path.corners[path.corners.Length - 1];
    // }
    //
    // private void OnEnable()
    // {
    //     input.Enable();
    // }
    //
    // private void OnDisable()
    // {
    //     input.Disable();
    // }
}
