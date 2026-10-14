using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum UnitArchetype {Brawler, Marksman}

public class PrototypeNPO : BaseOperativeClass
{
    [Header("Tracking for actions")]
    public bool hasRepositioned = false;
    public bool hasDashed = false;
    public bool hasFought = false;
    public bool hasCharged = false;
    public bool hasFallenBack = false;
    public int shootCount = 0;
    public bool canShootTwice = false;
    
    [Header("Navmesh")]
    public NavMeshAgent npoAgent;
    
    //Initializing
    [Header("Reference to the NPO manager")]
    public NPOManager npoManager;

    [Header("Unit Settings")]
    public UnitArchetype archetype = UnitArchetype.Marksman;
    
    //Action conditions (Predicates)
    public bool CanShoot() => currentOrder == OrderState.Engage && HasValidShootTarget(npoManager.playerUnits) && (shootCount < (canShootTwice ? 2 : 1));
    public bool CanReposition() => !hasRepositioned && currentApl > 0;
    public bool CanDash() => !hasDashed && currentApl > 0;
    public bool CanFight() => !hasFought && inControlRange();
    public bool CanCharge() => !hasCharged && possibleChargeTarget();
    public bool Fallback() => !hasFallenBack && inDanger() && !coverNear();

    public OperativeTemplate npoData
    {
        get => operativeData;
        set => operativeData = value;
    }
    
    private void Awake()
    {
        base.Awake();
        npoManager = GetComponentInParent<NPOManager>();
        npoAgent = GetComponent<NavMeshAgent>();
        acted = false;
    }
    
    private void Update()
    {
       base.Update();
    }

    //Action implementations
    public void PerformFight()
    {
        Debug.Log($"{name} performs Fight");
        hasFought = true;
        currentApl -= 1;
        CheckTurnEnd();
    }

    public void PerformCharge()
    {
        Debug.Log($"{name} performs Charge");
        hasCharged = true;
        currentApl -= 1;
        CheckTurnEnd();
    }

    public void PerformShoot()
    {
        Debug.Log($"{name} performs Shoot");
        
        //Null check
        if (operativeData == null || operativeData.weapons == null || operativeData.weapons.Count == 0)
        {
            Debug.Log($"{name} has not weapons");
            acted = true;
            return;
        }

        WeaponTemplate chosenWeapon = null;
        int highestDamage = -1;
        
        foreach (var weapon in operativeData.weapons)
        {
            if (weapon.rules.OfType<WeaponRules.Melee>().Any()) continue;
            
            if (weapon.DMGnorm > highestDamage)
            {
                highestDamage = weapon.DMGnorm;
                chosenWeapon = weapon;
            }
        }

        if (chosenWeapon == null)
        {
            acted = true;
            return;
        }

        PrototypeUnit targetPlayer = null;
        bool prioritizeWeakest = true;
        
        int bestHealthMetric = prioritizeWeakest ? int.MaxValue : int.MinValue;

        if (npoManager != null && npoManager.playerUnits != null)
        {
            foreach (var player in npoManager.playerUnits)
            {
                if (IsValidTarget(player))
                {
                    int playerHealth = player.currentWounds;

                    if (prioritizeWeakest)
                    {
                        if (playerHealth < bestHealthMetric)
                        {
                            bestHealthMetric = playerHealth;
                            targetPlayer = player;
                        }
                    }
                    else
                    {
                        if (playerHealth > bestHealthMetric)
                        {
                            bestHealthMetric = playerHealth;
                            targetPlayer = player;
                        }
                    }
                }
            }
        }

        if (targetPlayer != null)
        {
            Debug.Log($"{name} is going to shoot {targetPlayer.name} (Wounds: {bestHealthMetric}) using {chosenWeapon.name} for {highestDamage} normal damage!");
        }
        else
        {
            Debug.Log($"{name} attempted to shoot, but no valid target was in line of sight.");
        }

        shootCount++;
        currentApl -= 1;
        CheckTurnEnd();
    }

    public void PerformFallback()
    {
        Debug.Log($"{name} performs Fallback");
        currentApl -= 2;
        hasFallenBack = true;
        CheckTurnEnd();
    }

    public void PerformReposition()
    {
        Debug.Log($"{name} performs Reposition");
        currentApl -= 1;
        hasRepositioned = true;
        CheckTurnEnd();
    }

    public void PerformDash()
    {
        Debug.Log($"{name} performs Dash");
        currentApl -= 1;
        hasDashed = true;
        CheckTurnEnd();
    }

    public void SetOrderState(OrderState newOrderState)
    {
        currentOrder = newOrderState;
    }

    private bool HasValidShootTarget(List<PrototypeUnit> playerUnits)
    {
        Debug.Log("HasValidShootTarget called");
        foreach (var player in playerUnits)
        {
            if (IsValidTarget(player))
            {
                Debug.Log("Returning true for HasValidShootTarget");
                return true;
            }
        }

        Debug.Log("Returning false for HasValidShootTarget");
        return false;
    }

    public bool IsValidTarget(BaseOperativeClass target)
    {
        bool validTargetFound = false;
        Debug.Log("IsValidTarget called");
        if (target == null || target.dead || !target.gameObject.activeInHierarchy)
        {
            Debug.Log("The checked target is dead or doesn't exist");
            return false;
        }

        if (!HasLineOfSight(target))
        {
            Debug.Log("The checked target is out of sight");
            return false;
        }

        if (target.concealed && target.closeToCover)
        {
            Debug.Log("The checked target is concealed in cover");
            Debug.Log($"The targeting line rule returned: {CheckTargetingLines(target)}");
            return CheckTargetingLines(target);
        }

        return true;
    }

    private bool HasLineOfSight(BaseOperativeClass target)
    {
        Debug.Log("HasLineOfSight called");
        Vector3 start = losStart != null ? losStart.position : transform.position;
        Vector3 targetPos = target.losStart != null ? target.losStart.position : target.transform.position;
        Vector3 direction = (targetPos - start).normalized;
        float distance = Vector3.Distance(start, targetPos);

        if (Physics.Raycast(start, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                Debug.Log($"LOS blocked by: {hit.collider.name}");
                Debug.Log("HasLineOfSight returning false");
                return false;
            }
        }
        Debug.Log("HasLineOfSight returning true");
        return true;
    }
    
    private bool inControlRange()
    {
        Debug.Log($"{name} checks if in control range of enemy");
        return false;
    }

    private bool possibleChargeTarget()
    {
        Debug.Log($"{name} checks if it has a charge target");
        return false;
    }

    private bool inDanger()
    {
        Debug.Log($"{name} inDanger called");
        if (currentWounds < operativeData.WOUNDS)
        {
            Debug.Log($"{name} has lost health, returning true");
            return true;
        }
        Debug.Log($"{name} has not lost health, returning false");
        return false;
    }

    private bool coverNear()
    {
        Debug.Log($"{name} calling a check to see if cover is near");
        return CheckDistanceToTerrain();
    }

    private void CheckTurnEnd()
    {
        if (currentApl <= 0)
        {
            acted = true;
            Debug.Log($"{name} ran out of APL and its turn is over");
        }
    }

    public void ResetActionFlags()
    {
        hasRepositioned = false;
        hasDashed = false;
        hasFought = false;
        hasCharged = false;
        hasFallenBack = false;
        shootCount = 0;
    }
}
