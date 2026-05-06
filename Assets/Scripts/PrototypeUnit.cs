using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PrototypeUnit : MonoBehaviour
{
    private InputActions input;
    NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        input = new InputActions();
        AssignInputs();
    }

    void AssignInputs()
    {
        input.Controls.Move.performed += ctx => ClickToMove();
    }

    void ClickToMove()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            agent.destination = hit.point;
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
