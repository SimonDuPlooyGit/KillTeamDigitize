using UnityEngine;
using BehaviourTree;

//-----NPOBehaviour Class (Building the behaviour trees for either a brawler or marksman)-----
public class NPOBehaviour : MonoBehaviour
{
    //1 Activation priority
    //  1.1 Can perform FIGHT or SHOOT (Best weapon/highest chance to kill first)
    //  1.2 Is not in COVER from player
    //  1.3 Is closer to player
    
    //2 REPOSITION or DASH
    //  2.1 Move to cover where they have a valid target
    
    //3 SHOOT action priority target
    //  3.1 Is not obscured
    //  3.2 Is not in cover
    //  3.3 Is in Control Range of objective
    //  3.4 Is closest
    //  3.5 Is wounded
    //  3.6 Has not activated
    
    //4 FIGHT action priority
    //  4.1 Is in control range of objective
    //  4.2 Is wounded
    //  4.3 Has not activated
    
    //BRAWLER ARCHETYPE PRIORITY
    //  1. FIGHT
    //  2. Charge closest
    //  3. Conceal order and reposition towards nearest player (prefer cover and can use dash)
    //  4. Dash towards player (prefer cover)
    
    //MARKSMAN ARCHETYPE PRIORITY
    //  IF CAN SHOOT: ENGAGE
    //  1. FALL BACK to cover if possible with closest target not obscured or where objective visible
    //  2. SHOOT
    //  3. Reposition to cover if possible where closest player is not obscured or where objective visible
    //  4. Dash to cover if possible with closest target not obscured or where objective visible
    // IF CAN'T SHOOT: CONCEAL
    // Start at step 3

    [SerializeField] public NonPlayerOperative itsOwnNpoScript;
    private PrioritySelector _treeRoot;

    public GameManager gameManagerReference;
    
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        //Debug.Log("NPOBehaviour Awake called");
        IsRunning = false;
        itsOwnNpoScript = GetComponent<NonPlayerOperative>();
        gameManagerReference = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        //Debug.Log("BuildTree called in Start() of NPOBehaviour");
        BuildTree();
    }

    private void BuildTree()
    {
        //Debug.Log("BuildTree called in NPOBehaviour");
        _treeRoot = new PrioritySelector($"{itsOwnNpoScript.name}_Brain");
        //Debug.Log($"Built _treeRoot with name {itsOwnNpoScript.name}_Brain");

        if (itsOwnNpoScript.archetype == UnitArchetype.Marksman)
        {
            BuildMarksmanTree();
            //Debug.Log($"Building marksman tree for {itsOwnNpoScript.name}_Brain");
        } else if (itsOwnNpoScript.archetype == UnitArchetype.Brawler)
        {
            BuildBrawlerTree();
            //Debug.Log($"Building brawler tree for {itsOwnNpoScript.name}_Brain");
        }
    }
    
    //BRAWLER ARCHETYPE PRIORITIES
    //1. FIGHT (Priority 40)
    //2. Charge closest (Priority 30)
    //3. Conceal order + reposition to nearest player (Priority 20)
    //4. Dash towards player (Priority 10)

    private void BuildBrawlerTree()
    {
        //1. FIGHT
        var fightSequence = new Sequence("Fight Sequence", priority: 40);
        fightSequence.AddChild(new Leaf("Can Fight?", new Condition(itsOwnNpoScript.CanFight)));
        fightSequence.AddChild(new Leaf("Do Fight", new AsyncActionStrategy(itsOwnNpoScript.PerformFightB, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(fightSequence);
    
        //2. CHARGE
        var chargeSequence = new Sequence("Charge Sequence", priority: 30);
        chargeSequence.AddChild(new Leaf("Can Charge?", new Condition(itsOwnNpoScript.CanCharge)));
        chargeSequence.AddChild(new Leaf("Do Charge", new AsyncActionStrategy(itsOwnNpoScript.PerformCharge, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(chargeSequence);
    
        //3. CONCEAL & REPOSITION
        var concealRepositionSequence = new Sequence("Conceal & Reposition", priority: 20);
        concealRepositionSequence.AddChild(new Leaf("Can Reposition?", new Condition(itsOwnNpoScript.CanReposition)));
        concealRepositionSequence.AddChild(new Leaf("Set Conceal", new ActionStrategy(() => itsOwnNpoScript.SetOrderState(OrderState.Conceal))));
        concealRepositionSequence.AddChild(new Leaf("Do Reposition", new AsyncActionStrategy(itsOwnNpoScript.PerformRepositionB, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(concealRepositionSequence);

        //4. DASH
        var dashSequence = new Sequence("Dash Sequence", priority: 10);
        dashSequence.AddChild(new Leaf("Can Dash?", new Condition(itsOwnNpoScript.CanDash)));
        dashSequence.AddChild(new Leaf("Do Dash", new AsyncActionStrategy(itsOwnNpoScript.PerformDashB, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(dashSequence);
    }
    
    //MARKSMAN ARCHETYPE PRIORITY
    //  IF CAN SHOOT: ENGAGE
    //  1. FALL BACK to cover if possible with closest target not obscured or where objective visible (Priority 40)
    //  2. SHOOT (Priority 30)
    //  3. Reposition to cover if possible where closest player is not obscured or where objective visible (Priority 20)
    //  4. Dash to cover if possible with closest target not obscured or where objective visible (Priority 10)
    // IF CAN'T SHOOT: CONCEAL
    // Start at step 3 (Priority 20)
    // Then step 4 (Priority 10)
    
    private void BuildMarksmanTree()
    {
        var fallbackSequence = new Sequence("Fall Back Sequence", priority: 40);
        fallbackSequence.AddChild(new Leaf("Should Fallback?", new Condition(itsOwnNpoScript.Fallback)));
        fallbackSequence.AddChild(new Leaf("Do Fall Back", new AsyncActionStrategy(itsOwnNpoScript.PerformFallback, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(fallbackSequence);

        var shootSequence = new Sequence("Shoot Sequence", priority: 30);
        shootSequence.AddChild(new Leaf("Can Shoot?", new Condition(itsOwnNpoScript.CanShoot)));
        shootSequence.AddChild(new Leaf("Do Shoot", new AsyncActionStrategy(itsOwnNpoScript.PerformShoot, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(shootSequence);

        var repoSequence = new Sequence("Reposition Sequence", priority: 20);
        repoSequence.AddChild(new Leaf("Can Reposition?", new Condition(itsOwnNpoScript.CanReposition)));
        repoSequence.AddChild(new Leaf("Do Reposition", new AsyncActionStrategy(itsOwnNpoScript.PerformRepositionM, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(repoSequence);

        var dashSequence = new Sequence("Dash Sequence", priority: 10);
        dashSequence.AddChild(new Leaf("Can Dash?", new Condition(itsOwnNpoScript.CanDash)));
        dashSequence.AddChild(new Leaf("Do Dash", new AsyncActionStrategy(itsOwnNpoScript.PerformDashM, itsOwnNpoScript.IsActionFinished)));
        _treeRoot.AddChild(dashSequence);
    }

    public void ExecuteTurn()
    {
        Debug.Log("ExecuteTurn called");

        if (_treeRoot == null)
        {
            BuildTree();
        }
        
        if (_treeRoot != null)
        {
            Debug.Log("Executing Turn");
            _treeRoot.Reset();
            IsRunning = true;
        }
        else
        {
            Debug.Log("No Tree");
        }
    }
    
    public void TickTurn()
    {
        if (!IsRunning || _treeRoot == null) return;
        if (itsOwnNpoScript.isPerformingAnAction) return;

        if (itsOwnNpoScript.acted || itsOwnNpoScript.currentApl <= 0)
        {
            EndNpoTurn("No APL or already acted");
            return;
        }
    
        //Debug.Log($"[BehaviorTree] Processing tree root for {itsOwnNpoScript.name}. Current APL: {itsOwnNpoScript.currentApl}");
        
        Node.Status status = _treeRoot.Process();
        
        //Debug.Log($"[BehaviorTree] {itsOwnNpoScript.name} tree root returned status: {status}");

        if (status == Node.Status.Running)
        {
            return;
        }
    
        if (status == Node.Status.Success)
        {
            Debug.Log($"{itsOwnNpoScript.name} action completed. Re-evaluate priority action");

            if (itsOwnNpoScript.currentApl > 0 && !itsOwnNpoScript.acted)
            {
                _treeRoot.Reset();
                IsRunning = true;
            }
            else
            {
                EndNpoTurn("APL depleted after action");
            }
        }
        else if (status == Node.Status.Failure)
        {
            EndNpoTurn("Tree returned failure: no valid actions found");
        }
    }

    private void EndNpoTurn(string reason)
    {
        IsRunning = false;
        if (!itsOwnNpoScript.acted) itsOwnNpoScript.ApplyEndOfActivationOrder();
        itsOwnNpoScript.acted = true;
        itsOwnNpoScript.RemoveGlow();
        itsOwnNpoScript.selected = false;

        Debug.Log($"{itsOwnNpoScript.name} turn ended. Reason: {reason}");

        if (gameManagerReference != null && gameManagerReference.SharedContext != null)
        {
            gameManagerReference.SharedContext.PlayerTurn = true;
        }
        
        itsOwnNpoScript.currentApl = 0;
        itsOwnNpoScript.UpdateApl(itsOwnNpoScript.currentApl);
        itsOwnNpoScript._mainCamera.transform.position = new Vector3(-5f, 1.7f, 0.5f);
        itsOwnNpoScript._mainCamera.transform.eulerAngles = new Vector3(27.4f, 90f, 0f);
        //itsOwnNpoScript.CheckTurnEnd();
    }
}