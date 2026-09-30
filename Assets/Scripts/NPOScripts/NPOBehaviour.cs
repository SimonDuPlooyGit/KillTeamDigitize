using UnityEngine;

public class NPOBehaviour : MonoBehaviour
{
    //1 Activation priority
    //  1.1 Can perform FIGHT or SHOOT (Best weapon/highest chance to kill first)
    //  1.2 Is not in COVER from player
    //  1.3 Is closer to player
    
    //2 REPOSITION or DASH
    //  Move to cover where they have a valid target
    
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
}
