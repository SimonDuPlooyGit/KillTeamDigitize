using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum UnitArchetype {Brawler, Marksman}

//-----PrototypeNPO (Inherits from BaseOperativeClass. Has NPO specific functionality)-----
public class PrototypeNPO : BaseOperativeClass
{
    [Header("Cover and Navmesh")] 
    public NPOCoverNodes currentCoverSpot;
    public NavMeshAgent npoAgent;
    public NPOCoverNodes[] allCoverNodes;

    [Header("Tracking for actions")]
    public bool hasRepositioned;
    public bool hasDashed;
    public bool hasFought;
    public bool hasCharged;
    public bool hasFallenBack;
    public int shootCount;
    public bool canShootTwice;
    
    //Initializing
    [Header("Reference to the NPO manager")]
    public NPOManager npoManager;

    [Header("Unit Settings")]
    public UnitArchetype archetype = UnitArchetype.Marksman;
    
    [Header("Combat References")]
    public DiceHandler diceHandler;
    private InformationPackage _context;
    public bool IsShootingComplete { get; private set; }
    
    //Action conditions (Predicates)
    public bool CanShoot() => currentOrder == OrderState.Engage && HasValidShootTarget(npoManager.playerUnits) && (shootCount < (canShootTwice ? 2 : 1));
    public bool CanReposition() => !hasRepositioned && currentApl > 0;
    public bool CanDash() => !hasDashed && currentApl > 0;
    public bool CanFight() => !hasFought && InControlRange() && currentApl > 0;
    public bool CanCharge() => !hasCharged && PossibleChargeTarget() && currentApl > 0;
    public bool Fallback() => !hasFallenBack && InControlRange() && archetype == UnitArchetype.Marksman;

    public OperativeTemplate NpoData
    {
        get => operativeData;
    }
    
    protected override void Awake()
    {
        base.Awake();
        //Debug.Log($"Double checking that in PrototypeNPO that {name} is assigned to {NpoData.name}");
        GameManager gm = FindFirstObjectByType<GameManager>();
        _context = gm.SharedContext;
        allCoverNodes = FindObjectsByType<NPOCoverNodes>(FindObjectsSortMode.None);
        npoManager = GetComponentInParent<NPOManager>();
        npoAgent = GetComponent<NavMeshAgent>();
        acted = false;
    }

    protected override void Update()
    {
        if (operativeData == null)
        {
            Debug.LogError($"{name}'s operativeData just turned null!!!!");
        }
    }

    //Action implementations
    public void PerformFight()
    {
        Debug.Log($"{name} performs Fight");
        Debug.Log($"Need to still implement fighting");
        hasFought = true;
        currentApl -= 1;
        CheckTurnEnd();
    }

    public void PerformCharge()
    {
        Debug.Log($"{name} performs Charge");
        Debug.Log("Need to still implement charge");
        hasCharged = true;
        currentApl -= 1;
        CheckTurnEnd();
    }

    public void PerformShoot()
    {
        Debug.Log($"{name} performs Shoot");
        IsShootingComplete = false;
        StartCoroutine(ExecuteNpoShoot());
    }

    private IEnumerator ExecuteNpoShoot()
    {
        Debug.Log($"{name} starts shoot coroutine");

        WeaponTemplate chosenWeapon = null;
        int highestDamage = 1;

        foreach (var weapon in operativeData.weapons)
        {
            if (weapon.rules.OfType<WeaponRules.Melee>().Any()) continue;

            if (weapon.dmgNorm > highestDamage)
            {
                highestDamage = weapon.dmgNorm;
                chosenWeapon = weapon;
            }
        }

        PrototypeUnit targetPlayer = null;
        foreach (var player in npoManager.playerUnits)
        {
            if (IsValidTarget(player))
            {
                targetPlayer = player;
                break;
            }
        }

        if (targetPlayer != null && diceHandler != null)
        {
            _context.ActivatedUnitSo = operativeData;
            _context.Weapon = chosenWeapon;
            _context.CurrentlySelectedUnitScript = null;
            _context.CurrentlySelectedTargetScript = targetPlayer;
            _context.TargetUnitSo = targetPlayer.operativeData;
            
            _context.AttackRolls.Clear();
            _context.DefenseRolls.Clear();
            _context.RetainedCrits = 0;
            _context.RetainedNormals = 0;

            yield return StartCoroutine(diceHandler.ThrowAttackDice(chosenWeapon.atk, false, _context));
            yield return StartCoroutine(diceHandler.ThrowDefenseDice(_context.NumDefenseDiceRoll, false, _context));
            EvaluateNpoCombatHitsAndSaves(_context, targetPlayer);
        }
        else
        {
            Debug.Log("No LOS");
        }

        shootCount++;
        currentApl -= 1;
        CheckTurnEnd();
        IsShootingComplete = true;
    }

    private void EvaluateNpoCombatHitsAndSaves(InformationPackage context, PrototypeUnit target)
    {
        int crit = 0;
        int norm = 0;
        int critDef = 0;
        int normDef = 0;

        foreach (int roll in context.AttackRolls)
        {
            if (roll >= context.Weapon.hit)
            {
                if (roll == 6) crit++;
                else norm++;
            }
        }

        foreach (int roll in context.DefenseRolls)
        {
            if (roll >= target.operativeData.save)
            {
                if (roll == 6) critDef++;
                else normDef++;
            }
        }

        if (critDef >= crit)
        {
            critDef -= crit;
            crit = 0;
        }
        else
        {
            crit -= critDef;
            critDef = 0;
        }

        norm -= critDef;
        norm -= normDef;
        if (norm < 0)
        {
            norm = 0;
        }

        int totalDamage = (crit * context.Weapon.dmgCrit) + (norm * context.Weapon.dmgNorm);
        target.TakeDamage(totalDamage);
        target.UpdateHealth(target.currentWounds, target.operativeData.wounds);
        
        diceHandler.ClearDiceRolls();
    }

    public bool IsShootingFinished()
    {
        return IsShootingComplete;
    }

    public void PerformFallback()
    {
        Debug.Log($"{name} performs Fallback");
        Debug.Log("Still need to implement fallback");
        currentApl -= 2;
        hasFallenBack = true;
        CheckTurnEnd();
    }

    public void PerformReposition()
    {
        Debug.Log($"{name} performs Reposition");

        NPOCoverNodes targetCover = FindBestCoverNode(true, meterMovement);

        if (targetCover != null)
        {
            SetOrderState(OrderState.Engage);
            ExecuteMoveToCover(targetCover, meterMovement);
            Debug.Log($"{name} repositioned into {targetCover.name} and changed to ENGAGE");
            targetCover.occupied = true;
            hasRepositioned = true;
            currentApl -= 1;
            CheckTurnEnd();
            return;
        }
        
        targetCover = FindBestCoverNode(requireShootLos: false, meterMovement);
        
        if (targetCover != null)
        {
            SetOrderState(OrderState.Conceal);

            if (targetCover != null)
            {
                ExecuteMoveToCover(targetCover, meterMovement);
                Debug.Log($"{name} repositioned into {targetCover.name} and changed to CONCEAL");
                targetCover.occupied = true;
                Debug.Log("Ending turn early with APL = 0 because no more tactical moves");
                hasRepositioned = true;
                currentApl = 0; //End turn early cause it can't shoot at all and we got to cover
                CheckTurnEnd(); ;
                return;
            }
        }
        
        NPOCoverNodes partialCover = FindBestCoverNode(false, meterMovement+dashDistanceScaled);
            
        if (partialCover != null)
        {
            ExecuteMoveToCover(partialCover, meterMovement);
            Debug.Log($"{name} could not fully reach cover, repositioning closer to ${partialCover.name}");
            PerformDash();
        }
        else
        {
            MoveTowardSafePosition();
            Debug.Log($"{name} found no cover nodes, stepping closer to safety.");
        }
        
        hasRepositioned = true;
        currentApl -= 1;
        CheckTurnEnd();
    }

    public void PerformDash()
    {
        Debug.Log($"{name} performs Dash");
        
        NPOCoverNodes dashCover = FindBestCoverNode(true, dashDistanceScaled);

        if (dashCover != null)
        {
            ExecuteMoveToCover(dashCover, dashDistanceScaled);
            Debug.Log($"{name} dashed into {dashCover.name}");
            dashCover.occupied = true;
        }
        else
        {
            MoveTowardSafePosition();
            Debug.Log($"{name} dashed into safety");
        }
        
        hasDashed = true;
        currentApl -= 1;
        CheckTurnEnd();
}

    private Vector3 LimitDestination(Vector3 destination, float maxDistance)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, destination, npoAgent.areaMask, path))
        {
            float accumulatedDistance = 0f;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                float segmentDistance = Vector3.Distance(path.corners[i], path.corners[i + 1]);

                if (accumulatedDistance + segmentDistance > maxDistance)
                {
                    // We exceeded the limit on this segment; return a point partway along it
                    float remainingDistance = maxDistance - accumulatedDistance;
                    Vector3 direction = (path.corners[i + 1] - path.corners[i]).normalized;
                    return path.corners[i] + direction * remainingDistance;
                }

                accumulatedDistance += segmentDistance;
            }

            // If the entire path fits within the limit, return the final target position
            return path.corners[path.corners.Length - 1];
        }

        return destination;
    }

    private void ExecuteMoveToCover(NPOCoverNodes coverNode, float maxDistance)
    {
        currentCoverSpot = coverNode;
        //coverNode.occupied = true;
        Vector3 limitedSpot = LimitDestination(coverNode.spotToStand.position, maxDistance);
        npoAgent.SetDestination(limitedSpot);
    }

    private void MoveTowardSafePosition()
    {
        Debug.Log("MoveTowardSafePosition called");
    }

    private NPOCoverNodes FindClosestCoverNode()
    {
        Debug.Log("FindClosestCoverCalled cause no cover in reposition range");
        NPOCoverNodes closest = null;
        float minDist = float.MaxValue;

        foreach (var node in allCoverNodes)
        {
            if (node.occupied) continue;
            float distance = Vector3.Distance(transform.position, node.spotToStand.position);
            if (distance < minDist)
            {
                minDist = distance;
                closest = node;
            }
        }

        return closest;
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

        if (target.Concealed && target.closeToCover)
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
    
    private bool InControlRange()
    {
        //Debug.Log($"{name} checks if in control range of enemy");
        return false;
    }

    private bool PossibleChargeTarget()
    {
        Debug.Log($"{name} checks if it has a charge target");
        return false;
    }

    private bool InDanger()
    {
        Debug.Log($"{name} inDanger called");
        if (currentWounds < operativeData.wounds && closeToCover == false)
        {
            Debug.Log($"{name} has lost health and in open, returning true");
            return true;
        }
        Debug.Log($"{name} has not lost health, returning false");
        return false;
    }

    private bool CoverNear()
    {
        Debug.Log($"{name} calling a check to see if cover is near");
        return CheckDistanceToTerrain();
    }

    public void ResetActionFlags()
    {
        hasRepositioned = false;
        hasDashed = false;
        hasFought = false;
        hasCharged = false;
        hasFallenBack = false;
        shootCount = 0;

        if (operativeData != null)
        {
            currentApl = operativeData.apl;
            SetApl(currentApl);
        }
        else
        {
            Debug.LogError("Operative data is null");
        }
    }

    public NPOCoverNodes FindBestCoverNode(bool requireShootLos, float maxMovement)
{
    Debug.Log($"FindBestCoverNode called with requireShootLOS: {requireShootLos} and maxMovement: {maxMovement}");
    NPOCoverNodes bestCoverNode = null;
    float shortestDistance = float.MaxValue;

    foreach (var node in allCoverNodes)
    {
        if (node.occupied)
        {
            Debug.Log($"{node.name} is occupied");
            continue;
        }

        if (node.deepCover != !requireShootLos) 
        {
            Debug.Log($"{node.name} cover type does not match requirement (node.deepCover={node.deepCover}, requireShootLOS={requireShootLos})");
            continue;
        }
        
        float pathDistance = GetPathDistance(transform.position, node.spotToStand.position);

        if (pathDistance > maxMovement)
        {
            continue;
        }

        // If we require Shoot LOS, validate that standing there opens a shooting angle
        if (requireShootLos)
        {
            bool hasValidShotFromNode = false;

            foreach (var player in npoManager.playerUnits)
            {
                if (player.dead || player == null) continue;
                
                Vector3 simulatedLosStartPos = node.spotToStand.position + (losStart.position - transform.position);
                Vector3 targetPos = player.losStart != null ? player.losStart.position : player.transform.position;
                
                float distance = Vector3.Distance(simulatedLosStartPos, targetPos);
                Vector3 direction = (targetPos - simulatedLosStartPos).normalized;

                bool directLosClear = true;
                RaycastHit[] hits = Physics.RaycastAll(simulatedLosStartPos, direction, distance);

                foreach (var hit in hits)
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                    if (hit.transform == player.transform || hit.transform.IsChildOf(player.transform)) continue;
                    
                    if (hit.collider.CompareTag("Terrain"))
                    {
                        directLosClear = false;
                        break;
                    }
                }
                
                if (!directLosClear) continue;
                
                bool targetingLinesValid = CheckTargetingLinesFromPosition(node.spotToStand.position, player);

                if (player.Concealed && player.closeToCover)
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

            if (!hasValidShotFromNode) continue; // Skip if this node doesn't provide a valid shot
        }

        // FIX: Moved this outside the `if (requireShootLOS)` block so it evaluates distance and picks the 
        // closest valid node whether we are looking for Shoot Cover OR Deep Cover!
        if (pathDistance < shortestDistance)
        {
            shortestDistance = pathDistance;
            bestCoverNode = node;
            Debug.Log($"Found valid cover candidate: {node.name} at distance {pathDistance}");
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
            Vector3 simulatedNpoSp = samplingPoints[i].transform.position + offset;
            for (int j = 0; j < player.samplingPoints.Count; j++)
            {
                Vector3 playerSp1 = player.samplingPoints[j].transform.position;

                if (CheckLineObstruction(simulatedNpoSp, playerSp1))
                {
                    int oppositeIndex = (j + (player.samplingPoints.Count / 2)) % player.samplingPoints.Count;
                    Vector3 playerSp2 = player.samplingPoints[oppositeIndex].transform.position;

                    if (CheckLineObstruction(simulatedNpoSp, playerSp2))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool MovementFinished()
    {
        if (npoAgent == null) return true;

        if (npoAgent.pathPending) return false;

        if (npoAgent.remainingDistance <= npoAgent.stoppingDistance)
        {
            if (!npoAgent.hasPath || npoAgent.velocity.sqrMagnitude < 0.01f)
            {
                return true;
            }
        }
        return false;
    }
}