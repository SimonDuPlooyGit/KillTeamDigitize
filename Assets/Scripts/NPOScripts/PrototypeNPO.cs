using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum UnitArchetype {Brawler, Marksman}

public class PrototypeNPO : BaseOperativeClass
{
    //Initializing
    [Header("Reference to the NPO manager")]
    public NPOManager npoManager;

    [Header("Unit Settings")]
    public UnitArchetype archetype = UnitArchetype.Marksman;
    
    //Action conditions (Predicates)
    public bool CanShoot() => currentOrder == OrderState.Engage && HasValidShootTarget(npoManager.playerUnits);
    public bool CanFight() => inControlRange();
    public bool CanCharge() => possibleChargeTarget();
    public bool Fallback() => inDanger() && !coverNear();

    public OperativeTemplate npoData
    {
        get => operativeData;
        set => operativeData = value;
    }
    
    private void Awake()
    {
        base.Awake();
        npoManager = GetComponentInParent<NPOManager>();
        acted = false;
    }
    
    private void Update()
    {
       base.Update();
    }

    //Action implementations
    public void PerformFight()
    {
        
    }

    public void PerformCharge()
    {
        
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

        acted = true;
    }

    public void PerformFallback()
    {
        Debug.Log("PerformFallback was called");
    }

    public void PerformReposition()
    {
        
    }

    public void PerformDash()
    {
        
    }

    public void SetOrderState(OrderState newOrderState)
    {
        currentOrder = newOrderState;
    }

    private bool HasValidShootTarget(List<PrototypeUnit> playerUnits)
    {
        foreach (var player in playerUnits)
        {
            if (IsValidTarget(player))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsValidTarget(BaseOperativeClass target)
    {
        if (target == null || target.dead || !target.gameObject.activeInHierarchy) return false;
        if (!HasLineOfSight(target)) return false;
        if (target.concealed && target.closeToCover) return false;
        return CheckTargetingLines(target);
    }

    private bool HasLineOfSight(BaseOperativeClass target)
    {
        Vector3 start = losStart != null ? losStart.position : transform.position;
        Vector3 targetPos = target.losStart != null ? target.losStart.position : target.transform.position;
        Vector3 direction = (targetPos - start).normalized;
        float distance = Vector3.Distance(start, targetPos);

        if (Physics.Raycast(start, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                return false;
            }
        }
        return true;
    }
    
    private bool inControlRange()
    {
        return false;
    }

    private bool possibleChargeTarget()
    {
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
}
