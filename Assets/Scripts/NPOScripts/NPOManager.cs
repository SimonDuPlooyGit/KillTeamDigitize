using System.Collections.Generic;
using BehaviourTrees;
using Unity.VisualScripting;
using UnityEngine;

public class NPOManager : MonoBehaviour
{
    //1 Activation priority
    //  1.1 Can perform FIGHT or SHOOT (Best weapon/highest chance to kill first)
    //  1.2 Is not in COVER from player
    //  1.3 Is closer to player

    [SerializeField] private List<PrototypeNPO> npoUnits = new();
    [SerializeField] private List<PrototypeUnit> playerUnits = new();
    
    private List<PrototypeNPO> sortedRangedWeapons = new();
    private List<PrototypeNPO> sortedMeleeWeapons = new(); 
    

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

    private void Awake()
    {
        ActivateNPO(findBestCandidate());
    }
    
    public PrototypeNPO findBestCandidate()
    {
        PrototypeNPO bestCandidate = null;
        int maxDamage = -1;

        foreach (var npo in npoUnits)
        {
            if (npo == null || npo.dead) continue;

            bool canShoot = npo.CanShoot();
            bool canFight = npo.CanFight();

            if (!canShoot && !canFight) continue;

            int bestDamage = GetBestDamage(npo);

            if (bestDamage > maxDamage)
            {
                maxDamage = bestDamage;
                bestCandidate = npo;
            }
        }
        
        return bestCandidate;
    }

    private int GetBestDamage(PrototypeNPO npo)
    {
        if (npo.npoData == null || npo.npoData.weapons == null || npo.npoData.weapons.Count == 0) return 0;
        
        int highestDamage = 0;
        foreach (var weapon in npo.npoData.weapons)
        {
            if (weapon.DMGnorm > highestDamage)
            {
                highestDamage = weapon.DMGnorm;
            }
        }
        return highestDamage;
    }
    
    public void ActivateNPO(PrototypeNPO npo)
    {
        npo.selected = true;
    }
}
