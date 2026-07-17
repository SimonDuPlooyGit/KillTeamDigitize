using System;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputActions input;
    private InformationPackage sharedContext;
    private StateMachine stateMachine;
    [SerializeField] private MenuPanel menu;
    [SerializeField] private CombatManager combatManager;
    
    //Direct access reference for UI buttons to evoke events
    public MovementState movementState;

    private void Awake()
    {
        input = new InputActions();
        stateMachine = new StateMachine();
        sharedContext = new InformationPackage();

        //Initialize states
        var unitActivationState = new UnitActivationState(sharedContext, input, menu);
        var actionSelectionState = new ActionSelectionState(sharedContext, menu);
        movementState = new MovementState(sharedContext, input);
        var targetingState = new TargetingState(sharedContext, input, menu);
        var combatState = new CombatState(sharedContext, menu, combatManager);
        var weaponSelectState = new WeaponSelectState(sharedContext, input, menu);
        var pauseState = new PauseState(sharedContext, input, stateMachine);

        //Define transitions

        //Forward transitions
        AddT(unitActivationState, actionSelectionState, new FuncPredicate(() => sharedContext.isMovementRequested && sharedContext.currentlySelectedUnitScript != null));
        AddT(actionSelectionState, movementState, new FuncPredicate(() => !sharedContext.isMovementRequested && sharedContext.currentlySelectedUnitScript != null));
        AddT(actionSelectionState, weaponSelectState, new FuncPredicate(() => sharedContext.isShootingRequested));
        AddT(weaponSelectState, targetingState, new FuncPredicate(() => sharedContext.isWeaponSelected));
        AddT(targetingState, combatState, new FuncPredicate(() => sharedContext.currentlySelectedTarget != null));
        AddT(combatState, unitActivationState, new FuncPredicate(() => sharedContext.isShootingConfirmed));
        stateMachine.SetState(unitActivationState);

        //Backwards transitions
        GoBackAState(actionSelectionState, unitActivationState, () =>
        {
            sharedContext.currentlySelectedUnitScript.selected = false;
            sharedContext.currentlySelectedUnitScript.Reset();
            sharedContext.Reset();
        });
        GoBackAState(movementState, actionSelectionState, () =>
        {
            sharedContext.isMovementRequested = true;
        });
        GoBackAState(weaponSelectState, actionSelectionState, () =>
        {
            sharedContext.isShootingRequested = false;
        });
        GoBackAState(targetingState, weaponSelectState, () =>
        {
            sharedContext.isWeaponSelected = false;
            sharedContext.weapon = null;
        });
        GoBackAState(combatState, targetingState, () =>
        {
            sharedContext.currentlySelectedTarget = null;
        });
        
        //Global any transition
        AnyT(pauseState, new FuncPredicate(() => input.Controls.Pause.WasPressedThisFrame() && stateMachine.CurrentState is not PauseState && Time.frameCount > stateMachine.LastTransitionFrame));
    }

    //Helper function to do backwards transitions with right click as the predicate
    ////The action onBackTransition is logic you want to happen on the transition such as closing menus
    public void GoBackAState(IState currentState, IState previousState, Action onBackTransition) 
    {
        AddT(currentState, previousState, new FuncPredicate(() =>
        {
            if (input.Controls.Deselect.WasPressedThisFrame())
            {
                onBackTransition?.Invoke();
                return true;
            }
            return false;
        }));
    }
    
    void AddT(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);

    void AnyT(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    public void Update()
    {
        stateMachine.Update();
    }

    public void OnMoveActionButtonPressed()
    {
        sharedContext.isMovementRequested = false;
    }
    
    public void OnConfirmMovementButtonPressed()
    {
        if (movementState != null)
        {
            movementState.ConfirmAndExecuteMovement();
        }
    }

    public void onShootingButtonPressed()
    {
        sharedContext.isShootingRequested = true;
    }
    
    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();
}
