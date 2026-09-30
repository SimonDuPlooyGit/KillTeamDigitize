using UnityEngine;
using BehaviourTrees;

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

    private PrototypeNPO unit;
    private PrioritySelector treeRoot;

    private void Awake()
    {
        unit = GetComponent<PrototypeNPO>();
    }

    private void Start()
    {
        BuildTree();
    }

    public void BuildTree()
    {
        treeRoot = new PrioritySelector($"{unit.name}_Brain");

        if (unit.archetype == UnitArchetype.Marksman)
        {
            BuildMarksmanTree();
        } else if (unit.archetype == UnitArchetype.Brawler)
        {
            BuildBrawlerTree();
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
        fightSequence.AddChild(new Leaf("Can Fight?", new Condition(unit.CanFight)));
        fightSequence.AddChild(new Leaf("Do Fight", new ActionStrategy(unit.PerformFight)));
        
        //2. CHARGE
        var chargeSequence = new Sequence("Charge Sequence", priority: 30);
        chargeSequence.AddChild(new Leaf("Can Charge?", new Condition(unit.CanCharge)));
        chargeSequence.AddChild(new Leaf("Do Fight", new ActionStrategy(unit.PerformCharge)));
        
        // 3. CONCEAL & REPOSITION
        var concealRepositionSequence = new Sequence("Conceal & Reposition", priority: 20);
        concealRepositionSequence.AddChild(new Leaf("Set Conceal", new ActionStrategy(() => unit.SetOrderState(OrderState.Conceal))));
        concealRepositionSequence.AddChild(new Leaf("Do Reposition", new ActionStrategy(unit.PerformReposition)));
        treeRoot.AddChild(concealRepositionSequence);

        // 4. DASH
        treeRoot.AddChild(new Leaf("Dash", new ActionStrategy(unit.PerformDash), priority: 10));
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
        fallbackSequence.AddChild(new Leaf("Can Shoot?", new Condition(unit.CanShoot)));
        fallbackSequence.AddChild(new Leaf("Should Fallback?", new Condition(unit.Fallback)));
        fallbackSequence.AddChild(new Leaf("Do Fall Back", new ActionStrategy(unit.PerformFallback)));
        treeRoot.AddChild(fallbackSequence);

        // 2. SHOOT
        var shootSequence = new Sequence("Shoot Sequence", priority: 30);
        shootSequence.AddChild(new Leaf("Can Shoot?", new Condition(unit.CanShoot)));
        shootSequence.AddChild(new Leaf("Do Shoot", new ActionStrategy(unit.PerformShoot)));
        treeRoot.AddChild(shootSequence);

        // 3. REPOSITION
        treeRoot.AddChild(new Leaf("Reposition", new ActionStrategy(unit.PerformReposition), priority: 20));

        // 4. DASH
        treeRoot.AddChild(new Leaf("Dash", new ActionStrategy(unit.PerformDash), priority: 10));
    }

    public void ExecuteTurn()
    {
        if (treeRoot != null)
        {
            treeRoot.Reset();
            treeRoot.Process();
        }
    }
}
