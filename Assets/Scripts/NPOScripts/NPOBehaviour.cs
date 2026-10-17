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

    private PrototypeNPO _unit;
    private PrioritySelector _treeRoot;

    public GameManager gameManagerReference;
    
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        _unit = GetComponent<PrototypeNPO>();
        IsRunning = false;
        gameManagerReference = GameObject.FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        BuildTree();
    }

    private void BuildTree()
    {
        _treeRoot = new PrioritySelector($"{_unit.name}_Brain");

        if (_unit.archetype == UnitArchetype.Marksman)
        {
            BuildMarksmanTree();
            Debug.Log("Building marksman tree");
        } else if (_unit.archetype == UnitArchetype.Brawler)
        {
            BuildBrawlerTree();
            Debug.Log("Building brawler tree");
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
        fightSequence.AddChild(new Leaf("Can Fight?", new Condition(_unit.CanFight)));
        fightSequence.AddChild(new Leaf("Do Fight", new ActionStrategy(_unit.PerformFight)));
        
        //2. CHARGE
        var chargeSequence = new Sequence("Charge Sequence", priority: 30);
        chargeSequence.AddChild(new Leaf("Can Charge?", new Condition(_unit.CanCharge)));
        chargeSequence.AddChild(new Leaf("Do Fight", new ActionStrategy(_unit.PerformCharge)));
        
        // 3. CONCEAL & REPOSITION
        var concealRepositionSequence = new Sequence("Conceal & Reposition", priority: 20);
        concealRepositionSequence.AddChild(new Leaf("Set Conceal", new ActionStrategy(() => _unit.SetOrderState(OrderState.Conceal))));
        concealRepositionSequence.AddChild(new Leaf("Do Reposition", new ActionStrategy(_unit.PerformReposition)));
        _treeRoot.AddChild(concealRepositionSequence);

        // 4. DASH
        _treeRoot.AddChild(new Leaf("Dash", new ActionStrategy(_unit.PerformDash), priority: 10));
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
        // 1. FALL BACK (Only if can shoot and needs fallback)
        var fallbackSequence = new Sequence("Fall Back Sequence", priority: 40);
        fallbackSequence.AddChild(new Leaf("Can Shoot?", new Condition(_unit.CanShoot)));
        fallbackSequence.AddChild(new Leaf("Should Fallback?", new Condition(_unit.Fallback)));
        fallbackSequence.AddChild(new Leaf("Do Fall Back", new ActionStrategy(_unit.PerformFallback))); //Also async for moving
        _treeRoot.AddChild(fallbackSequence);

        // 2. SHOOT
        var shootSequence = new Sequence("Shoot Sequence", priority: 30);
        shootSequence.AddChild(new Leaf("Can Shoot?", new Condition(_unit.CanShoot)));
        shootSequence.AddChild(new Leaf("Do Shoot", new AsyncActionStrategy(_unit.PerformShoot, _unit.IsShootingFinished)));
        _treeRoot.AddChild(shootSequence);

        // 3. REPOSITION
        var repoSequence = new Sequence("Reposition Sequence", priority: 20);
        repoSequence.AddChild(new Leaf("Can Reposition?", new Condition(_unit.CanReposition)));
        repoSequence.AddChild(new Leaf("Do Reposition", new AsyncActionStrategy(_unit.PerformReposition, _unit.MovementFinished)));
        _treeRoot.AddChild(repoSequence);

        // 4. DASH
        var dashSequence = new Sequence("Dash Sequence", priority: 10);
        dashSequence.AddChild(new Leaf("Can Dash?", new Condition(_unit.CanDash)));
        dashSequence.AddChild(new Leaf("Do Dash", new ActionStrategy(_unit.PerformDash))); //Change this to async with a dash finish checker
        _treeRoot.AddChild(dashSequence);
    }

    public void ExecuteTurn()
    {
        if (_treeRoot != null)
        {
            Debug.Log("Executing Turn");
            _treeRoot.Reset();
            IsRunning = true;
        }
    }

    public void TickTurn()
    {
        if (!IsRunning || _treeRoot == null) return;

        if (_unit.acted || _unit.currentApl <= 0)
        {
            IsRunning = false;
            _unit.RemoveGlow();
            _unit.selected = false;
            Debug.Log($"{_unit.name} turn ended (No APL or already acted).");
            return;
        }
        
        Node.Status status = _treeRoot.Process();

        if (status == Node.Status.Running)
        {
            return;
        }
        
        if (status == Node.Status.Success)
        {
            Debug.Log($"{_unit.name} action completed. Re-evaluate priority action");

            if (_unit.currentApl > 0 && !_unit.acted)
            {
                _treeRoot.Reset();
            }
            else
            {
                IsRunning = false;
                Debug.Log($"{_unit.name} Turn ended since APL depleted");
                gameManagerReference.CheckTurningPointEnd();
            }
        }
        else if (status == Node.Status.Failure)
        {
            IsRunning = false;
            Debug.Log($"{_unit.name} Turn ended since Tree returned failure: no valid actions found");
        }
    }
}