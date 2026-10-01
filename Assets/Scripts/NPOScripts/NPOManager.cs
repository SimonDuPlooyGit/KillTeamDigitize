using System.Collections.Generic;
using BehaviourTrees;
using UnityEngine;

public class NPOManager : MonoBehaviour
{
    //1 Activation priority
    //  1.1 Can perform FIGHT or SHOOT (Best weapon/highest chance to kill first)
    //  1.2 Is not in COVER from player
    //  1.3 Is closer to player

    [SerializeField] private List<PrototypeNPO> npoUnits = new();

    //1.1
    /*public PrototypeNPO FindBestShootOrFightNPO()
    {
        //iterate through npoUnits and find which ones can shoot and or fight
        //pick the one with the strongest weapon
        //if no npo's can shoot or fight transition to 1.2
        
    }*/
    
    //1.2
    /*public PrototypeNPO FindNPONotInCover()
    {
        //pick an NPO that is most important/closest to death and that isn't in cover
        //if everyone is in cover then transition to 1.3
        
    }*/

    /*public PrototypeNPO FindNPOClosestToPlayer()
    {
        //pick the NPO closest to the player
        
    }*/

    public void ActivateNPO(PrototypeNPO npo)
    {
        
    }
}
