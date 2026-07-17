using UnityEngine;
using UnityEngine.InputSystem;

public class PauseState : BaseState
{
    private readonly InputActions _input;
    private readonly StateMachine _stateMachine;
    
    public PauseState(InformationPackage context, InputActions input, StateMachine stateMachine) : base(context)
    {
        _input = input;
        _stateMachine = stateMachine;
    }

    public override void OnEnter()
    {
        Debug.Log("Pause State entered");
        Time.timeScale = 0f;
        _input.Controls.Pause.performed += OnPausePerformed;
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Time.timeScale = 1f;
        _input.Controls.Pause.performed -= OnPausePerformed;
        Debug.Log("Pause state exited");
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        _stateMachine.GoToPreviousState();
    }
}
