using System.Collections.Generic;
using UnityEngine;

public class NPOManager : MonoBehaviour
{
    //1 Activation priority
    //  1.1 Can perform FIGHT or SHOOT (Best weapon/highest chance to kill first)
    //  1.2 Is not in COVER from player
    //  1.3 Is closer to player

    [SerializeField] public List<PrototypeNPO> npoUnits = new();
    [SerializeField] public List<PrototypeUnit> playerUnits = new();
    

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

    private void Update()
    {
        GetNPOUnitToActivate();
    }
    
    public PrototypeNPO GetNPOUnitToActivate()
    {
        Debug.Log("GetNPOUnitToActivate called");
        PrototypeNPO candidate = FindBestCandidate(npoUnits);
        if (candidate != null)
        {
            candidate.selected = true;
            return candidate;
        }

        candidate = FindNotInCover();
        if (candidate != null)
        {
            candidate.selected = true;
            return candidate;
        }

        
        return FindClosestNPO();
    }
    
    public PrototypeNPO FindBestCandidate(List<PrototypeNPO> npos)
    {
        PrototypeNPO bestCandidate = null;
        int maxDamage = -1;

        foreach (var npo in npos)
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
        
        if (bestCandidate != null) Debug.Log("FindBestCandidate found: " + bestCandidate.name);
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

    public PrototypeNPO FindNotInCover()
    {
        PrototypeNPO lowestOpenNpo = null;
        int lowestWounds = int.MaxValue;

        foreach (var npo in npoUnits)
        {
            if (npo == null || npo.dead || npo.acted || npo.closeToCover) continue;

            if (npo.currentWounds < lowestWounds)
            {
                lowestWounds = npo.currentWounds;
                lowestOpenNpo = npo;
            }
        }
        if (lowestOpenNpo != null) Debug.Log("FindNotInCover found: " + lowestOpenNpo.name);
        return lowestOpenNpo;
    }

    public PrototypeNPO FindClosestNPO()
    {
        PrototypeNPO closestNpo = null;
        float closestDistance = float.MaxValue;

        foreach (var npo in npoUnits)
        {
            if (npo == null || npo.dead || npo.acted) continue;

            foreach (var player in playerUnits)
            {
                if (player == null || player.dead)
                {
                    float distance = Vector3.Distance(npo.transform.position, player.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNpo = npo;
                    }
                }
            }
        }
        if (closestNpo != null) Debug.Log("FindClosestNPO found: " + closestNpo.name);
        closestNpo.selected = true;
        return closestNpo;
    }
}
