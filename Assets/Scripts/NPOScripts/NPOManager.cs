using System.Collections.Generic;
using UnityEngine;

//-----NPOManager Class (Picks an NPO to activate)-----
public class NPOManager : MonoBehaviour
{
    //1 Activation priority
    //  1.1 Can perform FIGHT or SHOOT (Best weapon/highest chance to kill first)
    //  1.2 Is not in COVER from player
    //  1.3 Is closer to player

    //Lists keeping track of the different player units and NPOs
    [SerializeField] public List<PrototypeNPO> npoUnits = new();
    [SerializeField] public List<PrototypeUnit> playerUnits = new();
    
    //Find the NPO to activate according the priority above
    public PrototypeNPO GetNpoUnitToActivate()
    {
        Debug.Log("GetNPOUnitToActivate called");
        PrototypeNPO candidate = FindBestCandidate(npoUnits);
        if (candidate != null)
        {
            candidate.AddGlow();
            candidate.selected = true;
            return candidate;
        }

        candidate = FindNotInCover();
        if (candidate != null)
        {
            candidate.AddGlow();
            candidate.selected = true;
            return candidate;
        }

        
        return FindClosestNpo();
    }
    
    //The actual logic to find the best candidate that can shoot
    private PrototypeNPO FindBestCandidate(List<PrototypeNPO> npos)
    {
        Debug.Log("FindBestCandidate called");
        PrototypeNPO bestCandidate = null;
        int maxDamage = -1;

        foreach (var npo in npos)
        {
            if (npo == null || npo.dead || npo.acted) continue;

            bool canShoot = npo.CanShoot();
            Debug.Log($"canShoot: {canShoot}");
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

    //Find the NPO that can do the most shooting damage
    private int GetBestDamage(PrototypeNPO npo)
    {
        if (npo.NpoData == null || npo.NpoData.weapons == null || npo.NpoData.weapons.Count == 0) return 0;
        
        int highestDamage = 0;
        foreach (var weapon in npo.NpoData.weapons)
        {
            if (weapon.dmgNorm > highestDamage)
            {
                highestDamage = weapon.dmgNorm;
            }
        }
        return highestDamage;
    }

    //Find the NPO that isn't in cover
    private PrototypeNPO FindNotInCover()
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

    //Find the NPO that is closest to a player unit
    private PrototypeNPO FindClosestNpo()
    {
        Debug.Log("FindClosestNPO called");
        PrototypeNPO closestNpo = null;
        float closestDistance = float.MaxValue;

        foreach (var npo in npoUnits)
        {
            if (npo == null || npo.dead || npo.acted) continue;

            foreach (var player in playerUnits)
            {
                if (player != null || !player.dead)
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
        closestNpo.AddGlow();
        closestNpo.selected = true;
        return closestNpo;
    }
}