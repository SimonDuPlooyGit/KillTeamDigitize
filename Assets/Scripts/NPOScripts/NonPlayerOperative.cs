using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum UnitArchetype {Brawler, Marksman}

//-----PrototypeNPO (Inherits from BaseOperativeClass. Has NPO specific functionality)-----
public class NonPlayerOperative : BaseOperativeClass
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
    private GameManager _gm;
    private DiceHandler _diceHandler;
    private InformationPackage _context;
    public OrbitCamera _mainCamera;
    
    [Header("Pacing and telegraphing")]
    [SerializeField] private float thinkDelay = 0.75f; //Beat before an action so the player sees who is acting
    [SerializeField] private float movePreviewDelay = 1.0f; //How long the path preview shows before walking
    [SerializeField] private float aimDelay = 2.0f; //How long the LOS/targeting lines show before shooting
    [SerializeField] private float shotResultDelay = 1.5f; //Time to look at the damage before moving on
    [SerializeField] private float postActionDelay = 0.75f; //Beat after an action before the next one is picked
    [SerializeField] private float walkSpeed = 1.0f;
    private const float MaxMoveTime = 15f; //Safety so a stuck agent can't hang the turn
    private bool _actionFinished = true;
    
    //Variables for charge checking in a safe way
    private const float radiusFromTarget = 0.33f;
    private const float maxDistanceFromTarget = 0.38f;
    private const float distFromAlly = 0.34f;
    private const int ringSamples = 16;
    
    protected override bool HasEngagedThisActivation => shootCount > 0 || hasFought || hasCharged;

    //Context seems to be null when we call getUnitToActivate so we need to find if earlier
    private InformationPackage Context
    {
        get
        {
            if (_context == null)
            {
                SetGameManager();
            }
            return _context;
        }
    }
    public bool IsShootingComplete { get; private set; }
    public bool isPerformingAnAction  {get; private set; }
    private PlayerOperative currentShootTarget;
    
    //Action conditions (Predicates)
    public bool CanShoot()
    {
        if (isPerformingAnAction)
        {
            //Debug.Log("Can't shoot because performing another action");
            return false;
        }
        if (shootCount >= (canShootTwice ? 2 : 1)) return false;
        if (!MovementFinished())
        {
            //Debug.Log("Can't shoot because is moving");
            return false;
        }

        if (currentShootTarget != null && IsValidTarget(currentShootTarget))
        {
            return true;
        }

        return HasValidShootTarget(npoManager.playerUnits);
    }

    public bool CanReposition() => !isPerformingAnAction && !hasRepositioned && !hasCharged && currentApl > 0;
    public bool CanDash() => !isPerformingAnAction && !hasDashed && !hasCharged && currentApl > 0;
    public bool CanFight() => !isPerformingAnAction && !hasFought && InControlRange() && currentApl > 0;
    public bool CanCharge() => !isPerformingAnAction && !hasCharged && !hasRepositioned && currentApl > 0 && PossibleChargeTarget();
    
    private bool PossibleChargeTarget() => TryFindChargeDestination(out _, out _);
    public bool Fallback() => !isPerformingAnAction && !hasFallenBack && InControlRange() && archetype == UnitArchetype.Marksman && currentApl > 1;
    
    //=====[Action runner and visualisation helpers]=====

//Entry point for every action. NPO counts as busy until the whole action (visuals included) is done
private void StartAction(IEnumerator routine)
{
    isPerformingAnAction = true;
    _actionFinished = false;
    _mainCamera = Context.MainCameraScript;
    StartCoroutine(routine);
}

private void FinishAction()
{
    isPerformingAnAction = false;
    _actionFinished = true;
}

//Polled by the behaviour tree's AsyncActionStrategy
public bool IsActionFinished() => _actionFinished;

//Beat before, action body, beat after, then check for end of turn
private IEnumerator RunAction(IEnumerator body)
{
    FocusCameraOnSelf();
    yield return new WaitForSeconds(thinkDelay);
    yield return StartCoroutine(body);
    HideLine();
    yield return new WaitForSeconds(postActionDelay);
    FinishAction();
    CheckTurnEnd();
}

private void FocusCameraOnSelf()
{
    OrbitCamera cam = Context.MainCameraScript;
    if (cam != null && (cam.isFreecam || cam.lookAtTarget != transform))
    {
        cam.SetOrbitTarget(transform);
    }
}

//FlowLog turns warnings into gameplay flow messages, so this tells the player what the NPO is doing
private void Announce(string message)
{
    Debug.LogWarning(message);
}

private string DisplayName(BaseOperativeClass op)
{
    if (op == null) return "Unknown";
    return op.nameText != null ? op.nameText.text : op.name;
}

private void FaceTowards(Vector3 worldPos)
{
    transform.LookAt(new Vector3(worldPos.x, transform.position.y, worldPos.z));
}

//LOS line from this NPO to its target, same idea as the player's ShootCombatState line
private void ShowLineTo(BaseOperativeClass target)
{
    if (lineRenderer == null) return;
    Vector3 start = losStart != null ? losStart.position : transform.position;
    Vector3 end = target.losStart != null ? target.losStart.position : target.transform.position;

    if (validLine != null) lineRenderer.material = validLine;
    lineRenderer.positionCount = 2;
    lineRenderer.SetPosition(0, start);
    lineRenderer.SetPosition(1, end);
    lineRenderer.enabled = true;
}

private void HideLine()
{
    if (lineRenderer == null) return;
    lineRenderer.enabled = false;
    lineRenderer.positionCount = 0;
}

//Show the path the NPO is about to take, then walk it and wait until it arrives
private IEnumerator MoveWithPreview(Vector3 destination)
{
    NavMeshPath path = new NavMeshPath();
    if (lineRenderer != null && NavMesh.CalculatePath(transform.position, destination, npoAgent.areaMask, path) && path.corners.Length > 1)
    {
        if (validLine != null) lineRenderer.material = validLine;
        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(movePreviewDelay);
    }

    npoAgent.SetDestination(destination);
    yield return null; //Let the agent start its path so MovementFinished doesn't instantly return true

    float timer = 0f;
    while (!MovementFinished() && timer < MaxMoveTime)
    {
        timer += Time.deltaTime;
        yield return null;
    }
    HideLine();
}

//Works out (but doesn't start) the move to a cover node and returns the final safe spot to walk to
private Vector3 PrepareMoveToCover(NPOCoverNodes coverNode, float maxDistance)
{
    if (currentCoverSpot != null && currentCoverSpot != coverNode)
    {
        currentCoverSpot.occupied = false;
    }
    currentCoverSpot = coverNode;

    Vector3 limitedSpot = LimitDestination(coverNode.spotToStand.position, maxDistance);
    return GetFullyValidDestination(limitedSpot);
}

private bool TryGetMoveTowardNearestPlayer(float maxDistance, out Vector3 destination)
{
    destination = transform.position;
    PlayerOperative nearestPlayer = GetNearestPlayer();
    if (nearestPlayer == null) return false;

    Vector3 limitedSpot = LimitDestination(nearestPlayer.transform.position, maxDistance);
    destination = GetFullyValidDestination(limitedSpot);
    return true;
}

//=====[Actions (called by the behaviour tree)]=====

//Brawler fight. Hands over to MeleeCombatState once the beat before it has played
public void PerformFightB()
{
    Debug.Log($"{name} performs Fight");
    StartAction(FightRoutine());
}

private IEnumerator FightRoutine()
{
    FocusCameraOnSelf();
    yield return new WaitForSeconds(thinkDelay);

    if (EnemyInControlRange && enemiesInControlRange.Count > 0)
    {
        BaseOperativeClass target = enemiesInControlRange[0];
        SetOrderState(OrderState.Engage);
        FaceTowards(target.transform.position);
        Announce($"{DisplayName(this)} fights {DisplayName(target)}");
        yield return new WaitForSeconds(movePreviewDelay);

        Context.SetSelectedOperativesData(this);
        Context.SetSelectedTarget(target);
        Context.Weapon = operativeData.weapons.FirstOrDefault(w => w.rules.OfType<WeaponRules.Melee>().Any());

        hasFought = true;
        UpdateApl(1);
        FinishAction(); //Must be finished before the state machine leaves NPOActionSelect
        Context.IsFightingRequested = true;
    }
    else
    {
        Debug.Log($"{name} couldn't fight anyone");
        hasFought = true;
        UpdateApl(1);
        FinishAction();
        CheckTurnEnd();
    }
}

public void PerformCharge()
{
    Debug.Log($"{name} performs Charge");
    StartAction(RunAction(ChargeRoutine()));
}

private IEnumerator ChargeRoutine()
{
    //TryFindChargeDestination returns the target and an engagement spot that is already inside the
    //charge budget (by path distance), on the navmesh and clear of other operatives, so no extra
    //limiting or congestion filtering is needed before walking there
    if (!TryFindChargeDestination(out PlayerOperative targetPlayer, out Vector3 chargeSpot))
    {
        Debug.Log($"{name} attempted to charge, but no valid charge target was found");
        yield break;
    }

    SetOrderState(OrderState.Engage);
    hasCharged = true;
    Announce($"{DisplayName(this)} charges {DisplayName(targetPlayer)}");
    targetPlayer.ForceUIOn(true);
    FaceTowards(targetPlayer.transform.position);

    yield return StartCoroutine(MoveWithPreview(chargeSpot));

    FaceTowards(targetPlayer.transform.position);
    targetPlayer.ForceUIOn(false);
    UpdateApl(1);
}

//Called in behaviour tree
public void PerformShoot()
{
    Debug.Log($"{name} performs Shoot");
    StartAction(RunAction(ExecuteNpoShoot()));
}

//Aim (camera, LOS line, targeting lines only if the target is in cover) -> roll dice -> resolve -> show the result
private IEnumerator ExecuteNpoShoot()
{
    PlayerOperative target = currentShootTarget;
    InformationPackage ctx = Context; //Using the property makes sure the GameManager references are set

    WeaponTemplate chosenWeapon = null;
    int highestDamage = -1;
    foreach (var weapon in operativeData.weapons)
    {
        if (weapon.rules.OfType<WeaponRules.Melee>().Any()) continue;

        if (weapon.dmgNorm > highestDamage)
        {
            highestDamage = weapon.dmgNorm;
            chosenWeapon = weapon;
        }
    }

    if (target != null && chosenWeapon != null && _diceHandler != null && ctx != null)
    {
        //1. Aim. This is the only time the NPO's lines are shown
        SetOrderState(OrderState.Engage);
        target.ForceUIOn(true);
        FaceTowards(target.transform.position);
        ctx.MainCameraScript.SetOrbitTarget(target.transform);
        ShowLineTo(target);

        if (target.currentOrder == OrderState.Conceal && target.closeToCover)
        {
            CheckTargetingLines(target); //Draws the temporary targeting lines, needed because the target is in cover
        }

        Announce($"{DisplayName(this)} is shooting {DisplayName(target)}");
        yield return new WaitForSeconds(aimDelay);
        HideLine();

        //2. Roll
        ctx.MainCameraScript.SetOrbitTarget(transform);
        ctx.SetSelectedOperativesData(this);
        ctx.SetSelectedTarget(target);
        ctx.Weapon = chosenWeapon;

        ctx.AttackRolls.Clear();
        ctx.DefenseRolls.Clear();
        ctx.RetainedCrits = 0;
        ctx.RetainedNormals = 0;

        yield return StartCoroutine(_diceHandler.ThrowAttackDice(chosenWeapon.atk, false, ctx));

        ctx.MainCameraScript.SetOrbitTarget(target.transform);
        yield return StartCoroutine(_diceHandler.ThrowDefenseDice(ctx.NumDefenseDiceRoll, false, ctx));
        EvaluateNpoCombatHitsAndSaves(ctx, target);

        //3. Let the player see the result
        yield return new WaitForSeconds(shotResultDelay);
        ctx.MainCameraScript.SetOrbitTarget(transform);
        target.ForceUIOn(false);
    }
    else
    {
        Debug.Log($"{name} couldn't shoot: target, weapon, dice handler or context missing");
    }

    shootCount++;
    UpdateApl(1);
}

//Marksman fallback (2 APL)
public void PerformFallback()
{
    Debug.Log($"{name} performs Fallback");
    hasFallenBack = true;
    StartAction(RunAction(RepositionMRoutine(true)));
}

//Marksman reposition (1 APL)
public void PerformRepositionM()
{
    Debug.Log($"{name} performs Marksman Reposition");
    hasRepositioned = true;
    StartAction(RunAction(RepositionMRoutine(false)));
}

//Shared by marksman reposition and fallback. Same cover priority as before
private IEnumerator RepositionMRoutine(bool isFallback)
{
    int cost = isFallback ? 2 : 1;
    string verb = isFallback ? "falls back" : "repositions";
    float budget = GetCurrentRepositionBudget();
    Vector3 destination;
    bool endTurnAfter = false;

    //1. Cover where it can shoot from
    NPOCoverNodes cover = FindBestMarksmanCoverNode(true, budget);
    if (cover != null)
    {
        destination = PrepareMoveToCover(cover, budget);
        cover.occupied = true;
        Announce($"{DisplayName(this)} {verb} to a firing position");
    }
    else
    {
        //2. Deep cover (can't shoot from there, so the turn ends after)
        cover = FindBestMarksmanCoverNode(false, budget);
        if (cover != null)
        {
            SetOrderState(OrderState.Conceal);
            destination = PrepareMoveToCover(cover, budget);
            cover.occupied = true;
            endTurnAfter = true;
            Announce($"{DisplayName(this)} {verb} into deep cover");
        }
        else
        {
            //3. Can't reach cover this action, move as close as the budget allows
            cover = FindAbsoluteClosestCoverNode();
            if (cover == null)
            {
                Debug.Log($"{name} found no unoccupied cover");
                UpdateApl(cost);
                yield break;
            }

            SetOrderState(OrderState.Conceal);
            destination = PrepareMoveToCover(cover, budget);
            Announce($"{DisplayName(this)} {verb} toward cover");
        }
    }

    yield return StartCoroutine(MoveWithPreview(destination));
    UpdateApl(cost);

    if (endTurnAfter)
    {
        Debug.Log("Ending turn early with APL = 0 because no more tactical moves");
        currentApl = 0;
        SetApl(currentApl);
    }
}

public void PerformDashM()
{
    Debug.Log($"{name} performs Marksman Dash");
    hasDashed = true;
    StartAction(RunAction(DashMRoutine()));
}

private IEnumerator DashMRoutine()
{
    NPOCoverNodes dashCover = FindAbsoluteClosestCoverNode();
    if (dashCover != null)
    {
        Vector3 destination = PrepareMoveToCover(dashCover, dashDistanceScaled);
        Announce($"{DisplayName(this)} dashes toward cover");
        yield return StartCoroutine(MoveWithPreview(destination));

        if (Vector3.Distance(transform.position, dashCover.spotToStand.position) <= 0.1f)
        {
            dashCover.occupied = true;
        }
    }
    else
    {
        Debug.Log($"{name} found no valid cover node for dash");
    }

    UpdateApl(1);
}

public void PerformRepositionB()
{
    Debug.Log($"{name} performs Brawler Reposition");
    hasRepositioned = true;
    StartAction(RunAction(BrawlerMoveRoutine(GetCurrentRepositionBudget(), "repositions")));
}

public void PerformDashB()
{
    Debug.Log($"{name} performs Brawler Dash");
    hasDashed = true;
    StartAction(RunAction(BrawlerMoveRoutine(dashDistanceScaled, "dashes")));
}

//Shared by brawler reposition and dash. Deep cover nearest the player, otherwise straight at the nearest player
private IEnumerator BrawlerMoveRoutine(float maxDistance, string verb)
{
    SetOrderState(OrderState.Conceal);
    Vector3 destination;
    NPOCoverNodes cover = FindBestBrawlerCover(maxDistance);

    if (cover != null)
    {
        destination = PrepareMoveToCover(cover, maxDistance);
        cover.occupied = true;
        Announce($"{DisplayName(this)} {verb} into cover");
    }
    else
    {
        if (currentCoverSpot != null)
        {
            currentCoverSpot.occupied = false;
            currentCoverSpot = null;
        }

        if (!TryGetMoveTowardNearestPlayer(maxDistance, out destination))
        {
            UpdateApl(1);
            yield break;
        }
        Announce($"{DisplayName(this)} {verb} toward the nearest operative");
    }

    yield return StartCoroutine(MoveWithPreview(destination));
    UpdateApl(1);
}

    public OperativeTemplate NpoData => operativeData;
    
    protected override void Awake()
    {
        canShootTwice = false;
        base.Awake();
        //Debug.Log($"Double checking that in PrototypeNPO that {name} is assigned to {NpoData.name}");
        allCoverNodes = FindObjectsByType<NPOCoverNodes>(FindObjectsSortMode.None);
        npoManager = GetComponentInParent<NPOManager>();
        npoAgent = GetComponent<NavMeshAgent>();
        if (npoAgent != null && walkSpeed > 0f) npoAgent.speed = walkSpeed;
        acted = false;
        //_mainCamera = _context.MainCameraScript;
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0;
        b.y = 0;
        return Vector3.Distance(a, b);
    }

    private bool TryFindChargeDestination(out PlayerOperative bestTarget, out Vector3 bestSpot)
    {
        bestTarget = null;
        bestSpot = Vector3.zero;

        if (Context == null || Context.AllPlayerUnits == null || npoAgent == null) return false;
        float budget = chargeDistanceScaled;
        
        var targets = Context.AllPlayerUnits
            .Where(p => p != null && !p.dead && p.gameObject.activeInHierarchy)
            .Where(p => FlatDistance(transform.position, p.transform.position) <= budget + radiusFromTarget)
            .OrderBy(p => FlatDistance(transform.position, p.transform.position));

        foreach (var player in targets)
        {
            if (TryFindEngagementSpot(player, budget, out Vector3 spot))
            {
                bestTarget = player;
                bestSpot = spot;
                return true;
            }
        }
        
        return false;
    }

    private bool TryFindEngagementSpot(PlayerOperative target, float budget, out Vector3 bestSpot)
    {
        bestSpot = Vector3.zero;
        Vector3 targetPos = target.transform.position;
        Vector3 towardNpo = transform.position - targetPos;
        towardNpo.y = 0;
        
        if (towardNpo.sqrMagnitude < 0.001f) towardNpo = Vector3.forward;
        towardNpo = towardNpo.normalized;
        
        float bestPathDistance = float.MaxValue;
        bool found = false;
        float step = 360f / ringSamples;

        for (int i = 0; i < ringSamples; i++)
        {
            Vector3 direction = Quaternion.Euler(0f, step * i, 0f) * towardNpo;
            Vector3 candidate = targetPos + direction * radiusFromTarget;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.1f, npoAgent.areaMask)) continue;
            Vector3 spot = hit.position;
            
            if (FlatDistance(spot, targetPos) > maxDistanceFromTarget) continue;
            if (IsSpotOccupied(spot, target)) continue;
            if (!TryGetPathDistance(transform.position, spot, out float pathDistance)) continue;
            if (pathDistance > budget) continue;

            if (pathDistance < bestPathDistance)
            {
                bestPathDistance = pathDistance;
                bestSpot = spot;
                found = true;
            }
        }

        return found;
    }

    private bool IsSpotOccupied(Vector3 spot, PlayerOperative target)
    {
        foreach (var npo in Context.AllNpoUnits)
        {
            if (npo == null || npo == this || npo.dead || !npo.gameObject.activeInHierarchy) continue;
            if (FlatDistance(spot, npo.transform.position) < distFromAlly) return true;
        }

        foreach (var player in Context.AllPlayerUnits)
        {
            if (player == null || player == target || player.dead || !player.gameObject.activeInHierarchy) continue;
            if (FlatDistance(spot, player.transform.position) < distFromAlly) return true;
        }
        
        return false;
    }

    private bool TryGetPathDistance(Vector3 start, Vector3 end, out float distance)
    {
        distance = 0f;
        NavMeshPath path = new NavMeshPath();
        
        if (!NavMesh.CalculatePath(start, end, npoAgent.areaMask, path)) return false;
        if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 1) return false;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return true;
    }

    //Handle hitting player in combat for shooting
    private void EvaluateNpoCombatHitsAndSaves(InformationPackage context, PlayerOperative target)
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

        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("Shoot");
        SoundManager.PlaySound(11, 0.7f, true);
        gameObject.GetComponent<SmokeTrails>().SpawnSmoke(norm,gameObject.transform,target.transform);
        gameObject.GetComponent<SmokeTrails>().SpawnSmoke(crit, gameObject.transform, target.transform);
        Debug.Log($"{name} is hitting {target.name} for {norm} hits of {context.Weapon.dmgNorm} and {crit} hit of {context.Weapon.dmgCrit}");
        int totalDamage = (crit * context.Weapon.dmgCrit) + (norm * context.Weapon.dmgNorm);
        Debug.Log($"This totals to {totalDamage} damage");
        target.TakeDamage(totalDamage);
        target.UpdateHealth(target.currentWounds, target.operativeData.wounds);
        _diceHandler.ClearDiceRolls();
    }

    //Clamp/limit any navmesh movement according to the NPO's movement budget
    private Vector3 LimitDestination(Vector3 destination, float maxDistance)
    {
        List<Vector3> dummyPoints;
        float dummyDistance;
        return LimitPathDistance(destination, maxDistance, out dummyPoints, out dummyDistance);
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
        PlayerOperative nearestPlayer = GetNearestPlayer();
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
    private PlayerOperative GetNearestPlayer()
    {
        PlayerOperative nearest = null;
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
    private bool HasValidShootTarget(List<PlayerOperative> playerUnits)
    {
        Debug.Log("HasValidShootTarget called");
        foreach (var player in playerUnits)
        {
            if (IsValidTarget(player))
            {
                currentShootTarget = player;
                //Debug.Log($"{player.name} is valid target for {name}");
                return true;
            }
            else
            {
                //Debug.Log($"{player.name} is not valid target for {name}");
            }
        }
        
        currentShootTarget = null;
        return false;
    }

    //Checks LOS and targeting lines to know if a player is valid target
    public bool IsValidTarget(BaseOperativeClass target)
    {
        //Debug.Log("IsValidTarget called");
        if (target == null || target.dead || !target.gameObject.activeInHierarchy || target.EnemyInControlRange)
        {
            Debug.Log("The checked target is dead or is in engage in control range");
            return false;
        }

        if (!HasLineOfSight(target))
        {
            //Debug.Log("The checked target is out of sight");
            return false;
        }

        if (target.currentOrder == OrderState.Conceal && target.closeToCover)
        {
            //Debug.Log("The checked target is concealed in cover");
            //Debug.Log($"The targeting line rule returned: {CheckTargetingLines(target)}");
            return CheckTargetingLines(target, false);
        }

        if (target.EnemyInControlRange)
        {
            return false;
        }

        return true;
    }

    //Check logic of the LOS check
    private bool HasLineOfSight(BaseOperativeClass target)
    {
        if (target == null) return false;
        Vector3 start = losStart != null ? losStart.position : transform.position;
        return HasLineOfSight(start, target);
    }
    
    //Overloaded LOS check from a project position
    public bool HasLineOfSight(Vector3 customLosStart, BaseOperativeClass target)
    {
        if (target == null || target.dead || !target.gameObject.activeInHierarchy) return false;

        Vector3 targetPos = target.losStart != null ? target.losStart.position : target.transform.position;
        float distance = Vector3.Distance(customLosStart, targetPos);
        Vector3 direction = (targetPos - customLosStart).normalized;

        RaycastHit[] hits = Physics.RaycastAll(customLosStart, direction, distance);
        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            if (hit.transform == target.transform || hit.transform.IsChildOf(target.transform)) continue;
            
            if (hit.collider.CompareTag("Terrain"))
            {
                return false;
            }
        }
        return true;
    }
    
    //Check if this NPO is in control range of a player unit
    private bool InControlRange()
    {
        return EnemyInControlRange;
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
        return CheckTerrainInControlRange();
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
    public override void TurnFinished()
    {
        base.TurnFinished();
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
        isPerformingAnAction = false;
        _actionFinished = true;
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
                
                Vector3 simulatedLosStartPos = node.spotToStand.position + ((losStart != null ? losStart.position : transform.position) - transform.position);
                bool directLosClear = HasLineOfSight(simulatedLosStartPos, player);
                if (!directLosClear) continue;
                
                bool targetingLinesValid = CheckTargetingLines(node.spotToStand.position, player);
                
                if (player.currentOrder == OrderState.Conceal && player.closeToCover)
                {
                    if (targetingLinesValid)
                    {
                        hasValidShotFromNode = true;
                        break;
                    }
                }
                else
                {
                    hasValidShotFromNode = true;
                    currentShootTarget = player;
                    break;
                }
            }
            if (!hasValidShotFromNode) continue;
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
    private bool CheckTargetingLinesFromPosition(Vector3 testPosition, PlayerOperative player)
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
    
    //Grab the game manager and context and dicehandler outside Awake() because it causes problems
    public void SetGameManager()
    {
        _gm = FindFirstObjectByType<GameManager>();
        if (_gm == null)
        {
            Debug.LogWarning($"{name} didn't get gm");
        }
        _context = _gm.SharedContext;
        if (_context == null)
        {
            Debug.LogWarning($"{name} didn't get context from gm");
        }
        _diceHandler = _gm.diceHandler;
        if (_diceHandler == null)
        {
            Debug.LogWarning($"{name} didn't get diceHandler from gm");
        }
    }
    
    //Check if a spot has a teamate there
    private bool IsBlockedByTeammate(Vector3 position)
    {
        float personalRadius = 0.16f;
        Collider[] colliders = Physics.OverlapSphere(position, personalRadius);
        
        foreach (var col in colliders)
        {
            BaseOperativeClass otherOp = col.GetComponentInParent<BaseOperativeClass>();
            if (otherOp != null && otherOp != this && !otherOp.dead && otherOp.CompareTag(gameObject.tag))
            {
                return true;
            }
        }
        return false;
    }
    
    //Filter method to make sure a pathfind destination doesn't end on an ally
    private Vector3 GetUncongestedDestination(Vector3 destination)
    {
        float personalRadius = 0.16f;
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, destination, npoAgent.areaMask, path) && path.corners.Length > 0)
        {
            Vector3 finalPoint = path.corners[path.corners.Length - 1];
            
            Collider[] colliders = Physics.OverlapSphere(finalPoint, personalRadius);
            foreach (var col in colliders)
            {
                BaseOperativeClass otherOp = col.gameObject.GetComponent<BaseOperativeClass>();
                if (otherOp != null && otherOp != this && !otherOp.dead && otherOp.CompareTag(gameObject.tag))
                {
                    Debug.Log($"{name}'s destination {finalPoint} is blocked by teammate {otherOp.name}. Finding alternative nearby spot");

                    //Try a few random/spiral offsets around the blocked point
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = i * (360f / 6f) * Mathf.Deg2Rad;
                        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.6f;
                        Vector3 candidatePoint = finalPoint + offset;

                        //Verify if this candidate point is on the NavMesh and free of teammates
                        if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, 0.5f, npoAgent.areaMask))
                        {
                            if (!IsBlockedByTeammate(hit.position) && !IsPointInEnemyControlRange(hit.position, 0.03f))
                            {
                                return hit.position; //Found a clear spot
                            }
                        }
                    }
                }
            }
            return finalPoint;
        }
        return destination;
    }

    //Combine filter methods to find a fully safe spot
    private Vector3 GetFullyValidDestination(Vector3 destination)
    {
        Vector3 safeFromControlRange = GetControlRangeSafeDestination(destination);
        Vector3 nonOverlapSpot = GetUncongestedDestination(safeFromControlRange);
        return nonOverlapSpot;
    }
}