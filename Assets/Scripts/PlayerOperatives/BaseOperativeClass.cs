using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.AI;

public enum OrderState {Engage, Conceal}

//-----BaseOperativeClass (The main unit class that NPOs and Player units should inherit from)-----
public abstract class BaseOperativeClass : MonoBehaviour
{
    [Header("Base Operative Class Data")]
    public OperativeTemplate operativeData;
    public float movementStat;
    public float meterMovement;
    public int currentWounds;
    public int currentApl;
    public bool dead;
    public bool selected;
    public bool closeToCover;
    public bool acted;
    public OrderState currentOrder;
    public float dashDistanceScaled = (3f / 39.37f) * 10;
    public float dashDistanceInches = 3;
    public float chargeDistanceScaled;

    [Header("Sampling and LOS")]
    public List<GameObject> samplingPoints;
    public Transform losStart;
    public LineRenderer lineRenderer;
    public Material validLine;
    public Material invalidLine;
    public bool AllyInControlRange;
    public bool EnemyInControlRange;

    [Header("UI References")] 
    public GameObject unitUI;
    public GameObject healthFill;
    public GameObject aplCount;
    public GameObject concealSymbol;
    public GameObject engageSymbol;
    public GameObject injuredSymbol;
    public GameObject concealInCoverSymbol;
    public GameObject damagePopOut;
    public TextMeshProUGUI nameText;
    public GameObject unitBase;

    [Header("Effects To Apply")]
    public GameObject fogEffect;
    public Material glow;

    [Header("Animation")]
    [SerializeField]
    private ParticleSystem deathParticles;
    private Animator _animator;
    
    [Header("Navmesh and Movement")]
    public NavMeshAgent agent;
    
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        fogEffect.SetActive(false);
        SetHealth();
        string brokenUpName = string.Concat(name.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).TrimStart(); //Space out the name from pascal case
        nameText.text = brokenUpName;
        
        //Null check and initialization
        if (operativeData != null)
        {
            //Debug.Log($"{name} (Instance ID: {gameObject.GetInstanceID()}) has been assigned {operativeData.name} operative data");
            currentWounds = operativeData.wounds;
            currentApl = operativeData.apl;
            SetApl(operativeData.apl);
            movementStat = operativeData.move;
            meterMovement = (movementStat / 39.37f) * 10;
            chargeDistanceScaled = meterMovement + (2f / 39.37f) * 10;
        }
        
        //Initialize the sampling points for targeting lines
        Transform spHolder = transform.Find("BaseSamplingPoints");
        if (spHolder != null)
        {
            foreach (Transform pt in spHolder)
            {
                samplingPoints.Add(pt.gameObject);
            }
        }
    }

    //Keep checking if the operative is near terrain (may later change this to only check at start and end of turn)
    protected virtual void Update()
    {
        if (currentOrder == OrderState.Engage)
        {
            engageSymbol.SetActive(true);
            concealSymbol.SetActive(false);
        } else if (currentOrder == OrderState.Conceal)
        {
            engageSymbol.SetActive(false);
            concealSymbol.SetActive(true);
        }
        
        SampleCollidersInControlRange();
        FogEffectAndCoverToggle();
    }
    
    //Calculate a path to destination and clamp the path distance according to max a distance
    //(Makes use of the out keyword at the limitedPoints and totalPathDistance arguments. This passes them by reference and makes it so we can also get limitedPoints and totalPathDistance from this function)
    //(Simply put this function can give us 3 outputs)
    public virtual Vector3 LimitPathDistance(Vector3 destination, float maxDistance, out List<Vector3> limitedPoints, out float totalPathDistance)
    {
        limitedPoints = new List<Vector3>();
        totalPathDistance = 0f;
        NavMeshPath path = new NavMeshPath();

        if (agent != null && NavMesh.CalculatePath(transform.position, destination, agent.areaMask, path))
        {
            if (path.status == NavMeshPathStatus.PathInvalid || path.corners.Length == 0)
            {
                limitedPoints.Add(transform.position);
                return transform.position;
            }
            
            for (int i = 0; i < path.corners.Length; i++)
            {
                limitedPoints.Add(path.corners[i]);
            }

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);

                if (totalPathDistance + segment > maxDistance)
                {
                    float remaining = maxDistance - totalPathDistance;
                    Vector3 direction = (path.corners[i + 1] - path.corners[i]).normalized;
                    Vector3 finalPoint = path.corners[i] + direction * remaining;
                    
                    limitedPoints.Clear();
                    for (int j = 0; j <= i; j++)
                    {
                        limitedPoints.Add(path.corners[j]);
                    }
                    limitedPoints.Add(finalPoint);
                
                    totalPathDistance = maxDistance;
                    return finalPoint;
                }
                
                totalPathDistance += segment;
            }
            return destination;
        } else if (agent == null)
        {
            Debug.Log("The agent is null");
        }
        else
        {
            Debug.Log("NavMesh.CalculatePath didn't succed");
        }

        limitedPoints.Add(transform.position);
        limitedPoints.Add(destination);
        totalPathDistance = Vector3.Distance(transform.position, destination);
        return destination;
    }

    //Check LOS between a unit's head LOS start to another unit's LOS start (losEnd in this) overloaded for a projected point as well
    public float DetermineLosAndDistance(Transform losEnd) //Call LOS and distance check on the unit
    {
        float distance = (Vector3.Distance(losStart.position, losEnd.position) / 10) * 39.37f; //Converting back to inches
        print(distance + "\"");
        Vector3 direction = (losEnd.position - losStart.position).normalized;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, losStart.position);

        if (Physics.Raycast(losStart.position, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("EnemyUnit"))
            {
                lineRenderer.material = validLine;
                lineRenderer.SetPosition(1, losEnd.position);
                Debug.Log($"LOS Clear. Total range to target: {distance} (From Unit Call)");
            }
            else
            {
                lineRenderer.material = invalidLine;
                lineRenderer.SetPosition(1, hit.point);
                Debug.Log($"LOS Blocked by {hit.collider.name}. Total range to target: {distance} (From Unit Call)");
                distance = 0;
            }
        }

        lineRenderer.enabled = true;

        return distance;
    }

    public float DetermineLosAndDistance(Transform projectedLosStart, Transform losEnd) //Call LOS and distance check at a projected point
    {
        float distance = (Vector3.Distance(projectedLosStart.position, losEnd.position) / 10) * 39.37f; //Converting back to inches
        print(distance + "\"");
        Vector3 direction = (losEnd.position - projectedLosStart.position).normalized;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, projectedLosStart.position);

        if (Physics.Raycast(projectedLosStart.position, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("EnemyUnit"))
            {
                lineRenderer.material = validLine;
                lineRenderer.SetPosition(1, losEnd.position);
                Debug.Log($"LOS Clear. Total range to target: {distance} (Projected LOS call)");
            }
            else
            {
                lineRenderer.material = invalidLine;
                lineRenderer.SetPosition(1, hit.point);
                Debug.Log($"LOS Blocked by {hit.collider.name}. Total range to target: {distance} (Projected LOS call)");
                distance = 0;
            }
        }

        lineRenderer.enabled = true;

        return distance;
    }

    //Check a 1-inch area around the operative to determine if terrain is within 1 inch
    public bool CheckTerrainInControlRange()
    {
        float oneInchScaled = (1f / 39.39f) * 10;
        bool isNearTerrain = false;

        foreach (var pt in samplingPoints)
        {
            Vector3 startPos = pt.transform.position;
            Vector3 direction = pt.transform.right;

            if (Physics.Raycast(startPos, direction, out RaycastHit hit, oneInchScaled))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(startPos, hit.point, Color.magenta, 0.5f);
                    isNearTerrain = true;
                }
            }
        }

        return isNearTerrain;
    }

    //Three overloaded CheckTargetingLines rule checks based on how and where you want to call them
    public bool CheckTargetingLines(BaseOperativeClass target) //Call it with just giving a target
    {
        return CheckTargetingLines(this, target);
    }

    public bool CheckTargetingLines(BaseOperativeClass attacker, BaseOperativeClass target) //Call it with giving an attacker and target
    {
        //Debug.Log("CheckTargetingLines called");
        if (attacker.samplingPoints.Count == 0 || target.samplingPoints.Count == 0) return false;

        for (int i = 0; i < attacker.samplingPoints.Count; i++)
        {
            Vector3 startSp = attacker.samplingPoints[i].transform.position;
            for (int j = 0; j < target.samplingPoints.Count; j++)
            {
                Vector3 targetSp1 = target.samplingPoints[j].transform.position;

                if (CheckLineObstruction(startSp, targetSp1))
                {
                    int oppositeIndex = (j + (target.samplingPoints.Count / 2)) % target.samplingPoints.Count;
                    Vector3 targetSp2 = target.samplingPoints[oppositeIndex].transform.position;

                    if (CheckLineObstruction(startSp, targetSp2))
                    {
                        Debug.DrawLine(startSp, targetSp1, Color.cyan, 1f);
                        Debug.DrawLine(startSp, targetSp2, Color.cyan, 1f);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool CheckTargetingLines(Vector3 projectedPosition, BaseOperativeClass target) //Call it from an offset position project from original point (Used in NPO projected movement checks)
    {
        if (samplingPoints.Count == 0 || target.samplingPoints.Count == 0) return false;

        Vector3 offset = projectedPosition - transform.position;

        for (int i = 0; i < samplingPoints.Count; i++)
        {
            Vector3 simulatedNpoSp = samplingPoints[i].transform.position + offset;
            for (int j = 0; j < target.samplingPoints.Count; j++)
            {
                Vector3 playerSp1 = target.samplingPoints[j].transform.position;

                if (CheckLineObstruction(simulatedNpoSp, playerSp1))
                {
                    int oppositeIndex = (j + (target.samplingPoints.Count / 2)) % target.samplingPoints.Count;
                    Vector3 playerSp2 = target.samplingPoints[oppositeIndex].transform.position;

                    if (CheckLineObstruction(simulatedNpoSp, playerSp2))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    
    //Check for terrain blockers a raycast (LOS ray or Targeting line rays)
    public bool CheckLineObstruction(Vector3 start, Vector3 end)
    {
        //Debug.Log("CheckLineObstruction called");
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        RaycastHit[] hits = Physics.RaycastAll(start, direction, distance);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                //Debug.Log($"Ray hit {hit.collider.name}");
                Debug.DrawLine(start, hit.point, Color.red, 1f);
                return false;
            }
        }
        Debug.DrawLine(start, end, Color.green, 1f);
        return true;
    }

    //Do damage communication and effects in here
    public void TakeDamage(int damage)
    {
        currentWounds -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentWounds}");
        
        if(_animator != null && damage>0)
        {
            if (currentWounds <= (int)Mathf.Ceil((float)operativeData.wounds / 2))
            {
                injuredSymbol.SetActive(true);
            }
            _animator.SetTrigger("TakeDamage");

            damagePopOut.GetComponent<TextMeshProUGUI>().text = damage.ToString();
            damagePopOut.SetActive(true);
        }
        else if(damage == 0)
        {
            damagePopOut.GetComponent<TextMeshProUGUI>().text = "Miss";
            damagePopOut.SetActive(true);
        }
        
        if (currentWounds <= 0)
        {
            currentWounds = 0;
            dead = true;
            HandleDeath();
        }
    }
    
    //Set up the healthbar
    public void SetHealth()
    {
        if (healthFill == null || operativeData == null) return;
        var img = healthFill.GetComponent<Image>();
        if (img != null) img.fillAmount = 1;
        var textComp = healthFill.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null) textComp.text = operativeData.wounds.ToString();
    }
    
    //Update the healthbar
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthFill == null) return;
        
        Image healthFillImage = healthFill.GetComponent<Image>();
        healthFillImage.fillAmount = currentHealth / maxHealth;

        var textComponent = healthFillImage.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null) textComponent.text = currentHealth.ToString();
        
        if (healthFillImage.fillAmount <= 0.25f)
            healthFillImage.color = new Color32(245, 32, 0, 255);
        else if (healthFillImage.fillAmount <= 0.5f)
            healthFillImage.color = new Color32(245, 180, 0, 255);
        else
            healthFillImage.color = new Color32(0, 245, 47, 255);
    }

    //Set apl in the UI
    public void SetApl(int apl)
    {
        //Debug.Log("SetApl called");
        if (aplCount == null) return;
        var textComponent = aplCount.GetComponent<TextMeshProUGUI>();
        if (textComponent != null) textComponent.text = apl.ToString();
    }

    //Update the APL marker on UI
    public void UpdateApl(int currApl)
    {
        Debug.Log("UpdateApl called");
        currentApl -= 1;
        SetApl(currApl);
    }
    
    //Do animations or effects of a unit dying in here (Calls DeathSequence coroutine)
    public void HandleDeath()
    {
        StartCoroutine(DeathSequence());
    }
    
    //Handle effects for when an Operative dies
    private IEnumerator DeathSequence()
    {
        Debug.Log($"{gameObject.name} has died.");
        _animator.SetTrigger("Die");
        yield return new WaitForSeconds(3.0f);
        deathParticles.gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        deathParticles.Play();
        yield return null;
        unitUI.SetActive(false);
        unitBase.SetActive(false);
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
    
    //Activates cover symbol and fog effect
    private void FogEffectAndCoverToggle()
    {
        if (closeToCover && unitUI != null && currentOrder == OrderState.Conceal)
        {
            concealInCoverSymbol.SetActive(true);
            fogEffect.SetActive(true);
        }
        else
        {
            concealInCoverSymbol.SetActive(false);
            fogEffect.SetActive(false);
        }
    }
    
    //Finish turn when apl is 0
    public void CheckTurnEnd()
    {
        if (currentApl == 0)
        {
            TurnFinished();
        }
    }

    //Add the glow material (Works as a layered material array)
    public void AddGlow()
    {
        //Debug.Log($"{gameObject.name} has added glow.");
        MeshRenderer bodyMeshRender = gameObject.GetComponentInChildren<MeshRenderer>();
        if (bodyMeshRender != null && glow != null)
        {
            Material[] mats = bodyMeshRender.materials;
            System.Array.Resize(ref mats, mats.Length + 1);
            mats[mats.Length - 1] = glow;
            bodyMeshRender.materials = mats;
        }
    }

    //Remove the glow material (Works as a layered material array)
    public void RemoveGlow()
    {
        //Debug.Log($"{gameObject.name} has removed glow.");
        MeshRenderer bodyMeshRender = gameObject.GetComponentInChildren<MeshRenderer>();
        
        Material[] mats = bodyMeshRender.materials;
        if (mats.Length > 1)
        {
            System.Array.Resize(ref mats, mats.Length - 1);
            bodyMeshRender.materials = mats;
        }
    }

    //Call this for every Operative in the game when a new turning point starts
    public virtual void ResetForNewTurningPoint()
    {
        Debug.Log($"{gameObject.name} has reset for new turning point");
        if (dead) return;
        
        acted = false;
        selected = false;

        if (operativeData != null)
        {
            currentApl = operativeData.apl;
            SetApl(currentApl);
        }
        
        RemoveGlow();
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    //Call this when CurrentlySelectedOperative has finished their turn
    public virtual void TurnFinished()
    {
        Debug.Log($"{gameObject.name}'s turn is over");
        acted = true;
        Deselect();
    }
    
    //Call this when you unselect a unit by transitioning back to UnitActivationState when you haven't performed any actions on CurrentlySelectedOperative
    public virtual void Deselect()
    {
        Debug.Log($"{gameObject.name} has been deselected");
        selected = false;
        RemoveGlow();
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    //Using Physics.OverlapSphere to check colliders in control range
    public void SampleCollidersInControlRange()
    {
        Collider[] colliders = Physics.OverlapSphere(unitBase.transform.position, 0.414f);
        closeToCover = false;
        AllyInControlRange = false;
        EnemyInControlRange = false;

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;

            if (col.CompareTag("Terrain"))
            {
                closeToCover = true;
                continue;
            }

            BaseOperativeClass otherOperative = col.GetComponentInParent<BaseOperativeClass>();
            if (otherOperative != null && !otherOperative.dead)
            {
                if (col.CompareTag(gameObject.tag))
                {
                    AllyInControlRange = true;
                }
                else
                {
                    EnemyInControlRange = true;
                }
            }
        }
    }

    public bool CheckInEachOthersControl(BaseOperativeClass otherOperative)
    {
        if (otherOperative == null) return false;
        
        float distance = Vector3.Distance(unitBase.transform.position, otherOperative.unitBase.transform.position);
        float maxEngagementDistance = 0.574f;
        return distance <= maxEngagementDistance;
    }
}