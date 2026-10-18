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
    public DiceHandler _diceHandler;
    private InformationPackage _context;
    public bool IsShootingComplete { get; private set; }
    public bool isPerformingAnAction  {get; private set; }
    private PrototypeUnit currentShootTarget;
    
    //Action conditions (Predicates)
    public bool CanShoot()
    {
        if (isPerformingAnAction) return false;
        if (currentOrder != OrderState.Engage) return false;
        if (shootCount >= (canShootTwice ? 2 : 1)) return false;
        if (!MovementFinished()) return false;

        if (currentShootTarget != null && IsValidTarget(currentShootTarget))
        {
            return true;
        }

        return HasValidShootTarget(npoManager.playerUnits);
    }

    public bool CanReposition() => !isPerformingAnAction && !hasRepositioned && currentApl > 0;
    public bool CanDash() => !isPerformingAnAction && !hasDashed && currentApl > 0;
    public bool CanFight() => !isPerformingAnAction && !hasFought && InControlRange() && currentApl > 0;
    public bool CanCharge() => !isPerformingAnAction && !hasCharged && PossibleChargeTarget() && currentApl > 0;
    public bool Fallback() => !isPerformingAnAction && !hasFallenBack && InControlRange() && archetype == UnitArchetype.Marksman;

    public OperativeTemplate NpoData
    {
        get => operativeData;
    }
    
    protected override void Awake()
    {
        canShootTwice = false;
        base.Awake();
        //Debug.Log($"Double checking that in PrototypeNPO that {name} is assigned to {NpoData.name}");
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogWarning($"{name} didn't get gm");
        }
        _context = gm.SharedContext;
        if (_context == null)
        {
            Debug.LogWarning($"{name} didn't get context from gm");
        }
        _diceHandler = gm.diceHandler;
        if (_diceHandler == null)
        {
            Debug.LogWarning($"{name} didn't get diceHandler from gm");
        }
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
    
    //Fight action for brawler
    public void PerformFightB()
    {
        Debug.Log($"{name} performs Fight");
        Debug.Log($"Need to still implement fighting");
        hasFought = true;
        currentApl -= 1;
        SetApl(currentApl);
        CheckTurnEnd();
    }

    //Brawler charge action
    public void PerformCharge()
    {
        Debug.Log($"{name} performs Charge");
        Debug.Log("Need to still implement charge");
        hasCharged = true;
        currentApl -= 1;
        SetApl(currentApl);
        CheckTurnEnd();
    }

    //Called in behaviour tree to start ExecuteNpoShoot coroutine
    public void PerformShoot()
    {
        Debug.Log($"{name} performs Shoot");
        isPerformingAnAction = true;
        IsShootingComplete = false;
        StartCoroutine(ExecuteNpoShoot());
    }

    //Coroutine for Executing an NPOs shoot action calls DiceHandler dice throwing methods
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

        if (currentShootTarget != null && _diceHandler != null)
        {
            _context.ActivatedUnitSo = operativeData;
            _context.Weapon = chosenWeapon;
            _context.CurrentlySelectedUnitScript = null;
            _context.CurrentlySelectedTargetScript = currentShootTarget;
            _context.CurrentlySelectedTarget = currentShootTarget.gameObject;
            _context.TargetUnitSo = currentShootTarget.operativeData;
            
            _context.AttackRolls.Clear();
            _context.DefenseRolls.Clear();
            _context.RetainedCrits = 0;
            _context.RetainedNormals = 0;

            yield return StartCoroutine(_diceHandler.ThrowAttackDice(chosenWeapon.atk, false, _context));
            yield return StartCoroutine(_diceHandler.ThrowDefenseDice(_context.NumDefenseDiceRoll, false, _context));
            EvaluateNpoCombatHitsAndSaves(_context, currentShootTarget);
        }
        else
        {
            Debug.Log("target player is null or dice handler is null");
        }

        shootCount++;
        currentApl -= 1;
        SetApl(currentApl);
        CheckTurnEnd();
        IsShootingComplete = true;
        isPerformingAnAction = false;
    }

    //Handle hitting player in combat for shooting
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

        Debug.Log($"{name} is hitting {target.name} for {norm} hits of {context.Weapon.dmgNorm} and {crit} hit of {context.Weapon.dmgCrit}");
        int totalDamage = (crit * context.Weapon.dmgCrit) + (norm * context.Weapon.dmgNorm);
        Debug.Log($"This totals to {totalDamage} damage");
        target.TakeDamage(totalDamage);
        target.UpdateHealth(target.currentWounds, target.operativeData.wounds);
        currentShootTarget = null;
        _diceHandler.ClearDiceRolls();
    }

    //Query if the shooting logic is all done
    public bool IsShootingFinished()
    {
        return IsShootingComplete;
    }

    //Marksman fallback behaviour
    public void PerformFallback()
    {
        Debug.Log($"{name} performs Fallback");
        Debug.Log("Still need to implement fallback");
        currentApl -= 2;
        SetApl(currentApl);
        hasFallenBack = true;
        CheckTurnEnd();
    }

    //Marksman reposition behaviour
    public void PerformRepositionM()
    {
        Debug.Log($"{name} performs Marksman Reposition");
        hasRepositioned = true;
        currentApl -= 1;
        SetApl(currentApl);
        
        NPOCoverNodes targetCover = FindBestMarksmanCoverNode(true, meterMovement);

        if (targetCover != null)
        {
            SetOrderState(OrderState.Engage);
            ExecuteMoveToCover(targetCover, meterMovement);
            Debug.Log($"{name} repositioned into {targetCover.name} and changed to ENGAGE");
            targetCover.occupied = true;
            CheckTurnEnd();
            return;
        }
        
        targetCover = FindBestMarksmanCoverNode(false, meterMovement);
        
        if (targetCover != null)
        {
            SetOrderState(OrderState.Conceal);
            ExecuteMoveToCover(targetCover, meterMovement);
            Debug.Log($"{name} repositioned into {targetCover.name} and changed to CONCEAL");
            targetCover.occupied = true;
            Debug.Log("Ending turn early with APL = 0 because no more tactical moves");
            currentApl = 0; //End turn early cause it can't shoot at all and we got to cover
            SetApl(currentApl);
            CheckTurnEnd();
            return;
        }
        
        NPOCoverNodes coverToGetTo = FindAbsoluteClosestCoverNode();

        if (coverToGetTo != null)
        {
            SetOrderState(OrderState.Conceal);
            ExecuteMoveToCover(coverToGetTo, meterMovement);
            Debug.Log($"{name} cannot reach cover in 1 turn. Moving {meterMovement}m closer to {coverToGetTo.name}");
        }
        else
        {
            Debug.LogWarning($"{name} found no unoccupied cover");
        }
        
        CheckTurnEnd();
    }

    //Brawler reposition logic
    public void PerformRepositionB()
    {
        Debug.Log($"{name} performs Brawler Reposition");
        hasRepositioned = true;
        currentApl -= 1;
        SetApl(currentApl);
        SetOrderState(OrderState.Conceal);
        NPOCoverNodes bestBrawlerCover = FindBestBrawlerCover(meterMovement);

        if (bestBrawlerCover != null)
        {
            if (currentCoverSpot != null)
            {
                currentCoverSpot.occupied = false;
                Debug.Log($"{name} left {currentCoverSpot.name}");
            }
            ExecuteMoveToCover(bestBrawlerCover, meterMovement);
            Debug.Log($"{name} brawler-repositioned into {bestBrawlerCover.name} under CONCEAL");
            bestBrawlerCover.occupied = true;
        }
        else
        {
            if (currentCoverSpot != null)
            {
                currentCoverSpot.occupied = false;
            }
            MoveToNearestPlayer(meterMovement);
        }
        
        CheckTurnEnd();
    }

    //Marksman dash logic
    public void PerformDashM()
    {
        Debug.Log($"{name} performs Marksman Dash");
        hasDashed = true;
        currentApl -= 1;
        SetApl(currentApl);

        NPOCoverNodes dashCover = FindAbsoluteClosestCoverNode();

        if (dashCover != null)
        {
            ExecuteMoveToCover(dashCover, dashDistanceScaled);
            Debug.Log($"{name} dashed {dashDistanceScaled}m closer to {dashCover.name}");
            if (Vector3.Distance(transform.position, dashCover.spotToStand.position) <= 0.1f)
            {
                dashCover.occupied = true;
            }
        }
        else
        {
            Debug.Log($"{name} found no valid cover node for dash");
        }
        
        CheckTurnEnd();
    }

    //Brawler dash logic
    public void PerformDashB()
    {
        Debug.Log($"{name} performs Brawler Dash");
        hasDashed = true;
        currentApl -= 1;
        SetApl(currentApl);
        
        SetOrderState(OrderState.Conceal);
        
        NPOCoverNodes bestBrawlerCover = FindBestBrawlerCover(dashDistanceScaled);

        if (bestBrawlerCover != null)
        {
            if (currentCoverSpot != null)
            {
                currentCoverSpot.occupied = false;
            }
            ExecuteMoveToCover(bestBrawlerCover, dashDistanceScaled);
            Debug.Log($"{name} brawler-dashed into {bestBrawlerCover.name}");
            bestBrawlerCover.occupied = true;
        }
        else
        {
            if (currentCoverSpot != null)
            {
                currentCoverSpot.occupied = false;
                Debug.Log($"{name} left {currentCoverSpot.name}");
            }
            MoveToNearestPlayer(dashDistanceScaled);
        }
        
        CheckTurnEnd();
    }

    //Clamp/limit any navmesh movement according to the NPO's movement budget
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

    //Execute the navmesh movement to a destination
    private void ExecuteMoveToCover(NPOCoverNodes coverNode, float maxDistance)
    {
        if (currentCoverSpot != null && currentCoverSpot != coverNode)
        {
            currentCoverSpot.occupied = false;
            Debug.Log($"{name} left {currentCoverSpot.name}");
        }
        
        currentCoverSpot = coverNode;
        
        Vector3 limitedSpot = LimitDestination(coverNode.spotToStand.position, maxDistance);
        npoAgent.SetDestination(limitedSpot);
    }

    //For marksmans to find the absolute closest cover
    private NPOCoverNodes FindAbsoluteClosestCoverNode()
    {
        NPOCoverNodes closest = null;
        float shortestDistance = float.MaxValue;
        
        foreach (var node in allCoverNodes)
        {
            if (node.occupied) continue;

            float pathDistance = GetPathDistance(transform.position, node.spotToStand.position);
            if (pathDistance < shortestDistance)
            {
                shortestDistance = pathDistance;
                closest = node;
            }
        }
        return closest;
    }
    
    //Find the best cover node for a brawler
    private NPOCoverNodes FindBestBrawlerCover(float maxDistance)
    {
        PrototypeUnit nearestPlayer = GetNearestPlayer();
        if (nearestPlayer == null)
        {
            Debug.LogWarning("No nearest player");
            return null;
        }
        
        NPOCoverNodes bestNode = null;
        float shortestDistance = float.MaxValue;

        foreach (var node in allCoverNodes)
        {
            if (node.occupied || !node.deepCover) continue;
            
            float pathDistance = GetPathDistance(transform.position, node.spotToStand.position);
            if (pathDistance > maxDistance) continue;
            
            float distanceToPlayer = Vector3.Distance(node.spotToStand.position, nearestPlayer.transform.position);

            if (distanceToPlayer < shortestDistance)
            {
                shortestDistance = distanceToPlayer;
                bestNode = node;
            }
        }
        
        return bestNode;
    }
    
    //Get the nearest player for brawler npo
    private PrototypeUnit GetNearestPlayer()
    {
        PrototypeUnit nearest = null;
        float minDistance = float.MaxValue;

        foreach (var player in npoManager.playerUnits)
        {
            if (player == null || player.dead || !player.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = player;
            }
        }

        return nearest;
    }

    //Move the brawler to the nearest player
    private void MoveToNearestPlayer(float maxDistance)
    {
        PrototypeUnit nearestPlayer = GetNearestPlayer();
        if (nearestPlayer != null)
        {
            Vector3 limitedSpot = LimitDestination(nearestPlayer.transform.position, maxDistance);
            npoAgent.SetDestination(limitedSpot);
            Debug.Log($"{name} moving {maxDistance}m directly toward {nearestPlayer.name}");
        }
    }
    
    //For a marksman if you want them to finest nearest deepCover
    private NPOCoverNodes FindClosestDeepCoverNode()
    {
        Debug.Log("FindClosestDeepCoverCalled for dash");
        NPOCoverNodes closest = null;
        float minDist = float.MaxValue;

        foreach (var node in allCoverNodes)
        {
            if (node.occupied || !node.deepCover) continue;
            float distance = Vector3.Distance(transform.position, node.spotToStand.position);
            if (distance < minDist)
            {
                minDist = distance;
                closest = node;
            }
        }
        
        Debug.Log($"FindClosestDeepCoverCalled found {closest.name} and it is {minDist} meters away");
        Debug.Log($"At the time of this cover being found dash distance for NPO is {dashDistanceScaled}");
        return closest;
    }

    //Sets an npos order state
    public void SetOrderState(OrderState newOrderState)
    {
        currentOrder = newOrderState;
    }

    //Checks for a marksman if they have any valid shooting targets
    private bool HasValidShootTarget(List<PrototypeUnit> playerUnits)
    {
        Debug.Log("HasValidShootTarget called");
        foreach (var player in playerUnits)
        {
            if (IsValidTarget(player))
            {
                currentShootTarget = player;
                Debug.Log($"{player.name} is valid target for {name}");
                return true;
            }
            else
            {
                Debug.Log($"{player.name} is not valid target for {name}");
            }
        }
        
        currentShootTarget = null;
        return false;
    }

    //Checks LOS and targeting lines to know if a player is valid target
    public bool IsValidTarget(BaseOperativeClass target)
    {
        //Debug.Log("IsValidTarget called");
        if (target == null || target.dead || !target.gameObject.activeInHierarchy)
        {
            Debug.Log("The checked target is dead or doesn't exist");
            return false;
        }

        if (!HasLineOfSight(target))
        {
            //Debug.Log("The checked target is out of sight");
            return false;
        }

        if (target.Concealed && target.closeToCover)
        {
            //Debug.Log("The checked target is concealed in cover");
            //Debug.Log($"The targeting line rule returned: {CheckTargetingLines(target)}");
            return CheckTargetingLines(target);
        }

        return true;
    }

    //Check logic of the LOS check
    private bool HasLineOfSight(BaseOperativeClass target)
    {
        //Debug.Log("HasLineOfSight called");
        Vector3 start = losStart != null ? losStart.position : transform.position;
        Vector3 targetPos = target.losStart != null ? target.losStart.position : target.transform.position;
        Vector3 direction = (targetPos - start).normalized;
        float distance = Vector3.Distance(start, targetPos);

        if (Physics.Raycast(start, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                //Debug.Log($"LOS blocked by: {hit.collider.name}");
                //Debug.Log("HasLineOfSight returning false");
                return false;
            }
        }
        //Debug.Log("HasLineOfSight returning true");
        return true;
    }
    
    //Check if this NPO is in control range of a player unit
    private bool InControlRange()
    {
        //Debug.Log($"{name} checks if in control range of enemy");
        return false;
    }

    //For a brawler to check if a player unit is in range for charging a player unit
    private bool PossibleChargeTarget()
    {
        Debug.Log($"{name} checks if it has a charge target");
        return false;
    }

    //Check for an NPO on whether they have taken damage and are in the open
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

    //Calls CheckDistanceToTerrain
    private bool CoverNear()
    {
        Debug.Log($"{name} calling a check to see if cover is near");
        return CheckDistanceToTerrain();
    }
    
    //Reset when turning points end
    public override void ResetForNewTurningPoint()
    {
        base.ResetForNewTurningPoint();

        if (dead && currentCoverSpot != null)
        {
            currentCoverSpot.occupied = false;
            currentCoverSpot = null;
        }
        
        ResetActionFlags();
    }

    //General Reset
    public override void ResetForNewActivation()
    {
        base.ResetForNewActivation();
        ResetActionFlags();
    }
    
    //Reset action taken trackers
    public void ResetActionFlags()
    {
        hasRepositioned = false;
        hasDashed = false;
        hasFought = false;
        hasCharged = false;
        hasFallenBack = false;
        shootCount = 0;
        currentShootTarget = null;
    }

    //Find the best cover for a marksman NPO
    public NPOCoverNodes FindBestMarksmanCoverNode(bool requireShootLos, float maxMovement)
{
    Debug.Log($"FindBestMarksmanCoverNode called with requireShootLOS: {requireShootLos} and maxMovement: {maxMovement}");
    NPOCoverNodes bestCoverNode = null;
    float shortestDistance = float.MaxValue;

    foreach (var node in allCoverNodes)
    {
        if (node.occupied)
        {
            //Debug.Log($"{node.name} is occupied");
            continue;
        }

        if (node.deepCover != !requireShootLos) 
        {
            //Debug.Log($"{node.name} cover type does not match requirement (node.deepCover={node.deepCover}, requireShootLOS={requireShootLos})");
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
                    currentShootTarget = player;
                    break;
                }
            }

            if (!hasValidShotFromNode) continue; // Skip if this node doesn't provide a valid shot
        }
        
        if (pathDistance < shortestDistance)
        {
            shortestDistance = pathDistance;
            bestCoverNode = node;
            Debug.Log($"Found valid cover candidate: {node.name} at distance {pathDistance}");
        }
    }
    return bestCoverNode;
}

    //Helper function to get distance between two points according to navmesh movement
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

    //Check the prospective targeting lines rule ahead of moving from a spot when trying to shoot at a player that is concealed and in cover
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

    //Checker for the behaviour tree to wait for navmesh movement to finish
    public bool MovementFinished()
    {
        //if (npoAgent == null) return true;

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