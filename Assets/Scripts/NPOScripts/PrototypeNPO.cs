using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum UnitArchetype {Brawler, Marksman}

public class PrototypeNPO : BaseOperativeClass
{
    [Header("Cover and Navmesh")] 
    public NPOCoverNodes currentCoverSpot;
    public NavMeshAgent npoAgent;
    
    [Header("Tracking for actions")]
    public bool hasRepositioned = false;
    public bool hasDashed = false;
    public bool hasFought = false;
    public bool hasCharged = false;
    public bool hasFallenBack = false;
    public int shootCount = 0;
    public bool canShootTwice = false;
    
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
        
        bool needShootLOS = CanShoot() || currentOrder == OrderState.Engage;
        NPOCoverNodes targetCover = FindBestCoverNode(requireShootLOS: needShootLOS);

        if (targetCover != null)
        {
            currentCoverSpot = targetCover;
            targetCover.occupied = true;
            npoAgent.SetDestination(targetCover.spotToStand.position);
            Debug.Log($"{name} repositioning to cover node at {targetCover.transform.position}");
        }
        else
        {
            Debug.Log($"{name} found no optimal cover node, holding position or advancing");
        }
        
        currentApl -= 1;
        hasRepositioned = true;
        CheckTurnEnd();
    }

    public void PerformDash()
    {
        Debug.Log($"{name} performs Dash");

        NPOCoverNodes targetCover = FindBestCoverNode(requireShootLOS: false);
        if (targetCover != null)
        {
            currentCoverSpot = targetCover;
            targetCover.occupied = true;
            npoAgent.SetDestination(targetCover.spotToStand.position);
        }
        
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
        if (currentWounds < operativeData.WOUNDS && closeToCover == false)
        {
            Debug.Log($"{name} has lost health and in open, returning true");
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

    public NPOCoverNodes FindBestCoverNode(bool requireShootLOS)
    {
        NPOCoverNodes[] allCoverNodes = FindObjectsOfType<NPOCoverNodes>();
        NPOCoverNodes bestCoverNode = null;
        float shortestDistance = float.MaxValue;

        foreach (var node in allCoverNodes)
        {
            if (node.occupied) continue;
            
            if (node.deepCover == requireShootLOS) continue; //If we want shoot cover we want deepCover == false
            
            float pathDistance = GetPathDistance(transform.position, node.spotToStand.position);
            
            if (pathDistance > meterMovement && hasRepositioned) continue;

            if (requireShootLOS)
            {
                bool hasValidShotFromNode = false;

                foreach (var player in npoManager.playerUnits)
                {
                    if (player.dead || player == null) continue;
                    
                    Vector3 simulatedLOSStartPos = node.spotToStand.position + (losStart.position - transform.position);
                    Vector3 targetPos = player.losStart != null ? player.losStart.position : player.transform.position;
                    
                    float distance = Vector3.Distance(simulatedLOSStartPos, targetPos);
                    Vector3 direction = (targetPos - simulatedLOSStartPos).normalized;

                    bool directLOSClear = true;
                    RaycastHit[] hits = Physics.RaycastAll(simulatedLOSStartPos, direction, distance);

                    foreach (var hit in hits)
                    {
                        if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                        if (hit.transform == player.transform || hit.transform.IsChildOf(player.transform)) continue;
                        
                        if (hit.collider.CompareTag("Terrain"))
                        {
                            directLOSClear = false;
                            break;
                        }
                    }
                    
                    if (!directLOSClear) continue;
                    
                    bool targetingLinesValid = CheckTargetingLinesFromPosition(node.spotToStand.position, player);

                    if (player.concealed && player.closeToCover)
                    {
                        if (targetingLinesValid)
                        {
                            hasValidShotFromNode = true;
                            Debug.Log($"Can see {player.name} even in concealed cover from {node.name} since LOS and targeting lines valid");
                            break;
                        }
                    }
                    else
                    {
                        hasValidShotFromNode = true;
                        Debug.Log($"Can see {player.name} with valid LOS from {node.name} since they are in the open");
                        break;
                    }
                }
                if (!hasValidShotFromNode) continue;

                if (pathDistance < shortestDistance)
                {
                    shortestDistance = pathDistance;
                    bestCoverNode = node;
                }
            }
        }
        return bestCoverNode;
    }

    private float GetPathDistance(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(start, end, npoAgent.areaMask, path))
        {
            float distance = 0f;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return distance;
        }
        return float.MaxValue;
    }

    private bool CheckTargetingLinesFromPosition(Vector3 testPosition, PrototypeUnit player)
    {
        if (samplingPoints.Count == 0 || player.samplingPoints.Count == 0) return false;

        Vector3 offset = testPosition - transform.position;

        for (int i = 0; i < samplingPoints.Count; i++)
        {
            Vector3 simulatedNpoSP = samplingPoints[i].transform.position + offset;
            for (int j = 0; j < player.samplingPoints.Count; j++)
            {
                Vector3 playerSP1 = player.samplingPoints[j].transform.position;

                if (CheckLineObstruction(simulatedNpoSP, playerSP1))
                {
                    int oppositeIndex = (j + (player.samplingPoints.Count / 2)) % player.samplingPoints.Count;
                    Vector3 playerSP2 = player.samplingPoints[oppositeIndex].transform.position;

                    if (CheckLineObstruction(simulatedNpoSP, playerSP2))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
