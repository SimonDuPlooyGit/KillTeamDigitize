using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputActions input;
    private InformationPackage sharedContext;
    private StateMachine stateMachine;
    [SerializeField] private MenuPanel menu;
    
    //Direct access reference for UI buttons to evoke eveents
    private MovementState movementState;
    
    private void Awake()
    {
        input = new InputActions();
        stateMachine = new StateMachine();
        sharedContext = new InformationPackage();
        
        //Initialize states
        var unitActivationState = new UnitActivationState(sharedContext, input, menu);
        var actionSelectionState = new ActionSelectionState(sharedContext);
        var movementState = new MovementState(sharedContext, input);
        
        //Define transitions
        AddT(unitActivationState, actionSelectionState,
            new FuncPredicate(() => sharedContext.isActionSelectionRequested && sharedContext.activePrototypeUnit != null));
        AddT(actionSelectionState, movementState,
            new FuncPredicate(() => !sharedContext.isActionSelectionRequested && sharedContext.activePrototypeUnit != null));
        AddT(movementState, unitActivationState, new FuncPredicate(() => sharedContext.isMovementConfirmed || sharedContext.currentlySelectedOperative == null));
        
        stateMachine.SetState(unitActivationState);
    }
    
    void AddT(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(to, from, condition);

    void AnyT(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    public void Update()
    {
        stateMachine.Update();
    }

    public void OnMoveActionButtonPressed()
    {
        if (movementState != null)
        {
            movementState.ConfirmAndExecuteMovement();
        }
    }
    
    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();
}
