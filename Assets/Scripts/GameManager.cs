using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputActions input;
    private InformationPackage sharedContext;
    private StateMachine stateMachine;
    [SerializeField] private MenuPanel menu;
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private GameObject shootMenuHolder;
    
    //Direct access reference for UI buttons to evoke events
    private MovementState movementState;
    
    private void Awake()
    {
        input = new InputActions();
        stateMachine = new StateMachine();
        sharedContext = new InformationPackage();
        
        //Initialize states
        var unitActivationState = new UnitActivationState(sharedContext, input, menu);
        var actionSelectionState = new ActionSelectionState(sharedContext, menu);
        movementState = new MovementState(sharedContext, input);
        var targetingState = new TargetingState(sharedContext, input, menu, shootMenuHolder);
        var combatState = new CombatState(sharedContext, combatManager);
        var weaponSelectState = new WeaponSelectState(sharedContext, input, menu);

        //Define transitions
        AddT(unitActivationState, actionSelectionState, new FuncPredicate(() => sharedContext.isMovementRequested && sharedContext.currentlySelectedUnitScript != null));
        AddT(actionSelectionState, movementState, new FuncPredicate(() => !sharedContext.isMovementRequested && sharedContext.currentlySelectedUnitScript!= null));
        AddT(movementState, unitActivationState, new FuncPredicate(() => sharedContext.isMovementConfirmed || sharedContext.currentlySelectedUnitScript == null));
        AddT(actionSelectionState, weaponSelectState, new FuncPredicate(() => sharedContext.isShootingRequested));
        AddT(weaponSelectState, targetingState, new FuncPredicate(() => sharedContext.isWeaponSelected));
        AddT(targetingState, combatState, new FuncPredicate(() => sharedContext.currentlySelectedTarget != null));
        
        stateMachine.SetState(unitActivationState);
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
