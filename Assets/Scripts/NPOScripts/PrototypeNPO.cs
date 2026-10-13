using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public enum UnitArchetype {Brawler, Marksman}
public enum OrderState {Engage, Conceal}

public class PrototypeNPO : MonoBehaviour
{
    //Initializing
    public OperativeTemplate npoData;
    public float movementStat;
    public float meterMovement;
    private float pathDistance;
    public bool selected = false;
    public int currentWounds;
    public int currentAPL;
    public bool dead = false;
    public float remainingMovement;
    public Material validLine;
    public Material invalidLine;
    [SerializeField] public Material glow;
    [SerializeField] public Material fog;
    [SerializeField] private GameObject unitUI;
    public NPOManager npoManager;

    [Header("Unit Settings")]
    public UnitArchetype archetype = UnitArchetype.Marksman;
    public OrderState currentOrder = OrderState.Conceal;
    
    public bool closeToCover;
    public bool acted;
    
    //Sampling data for cover and targeting lines
    public List<GameObject> samplingPoints;
    
    //Line of sight variables
    public Transform losStart; //assigned in inspector
    
    //Movement and navmesh
    private NavMeshAgent agentUnit;
    public NavMeshPath path;
    List<Vector3> limitedPoints = new List<Vector3>();
    private float currentPathDistance;
    
    //Unit UI variables
    public GameObject healthFill;
    public GameObject aplCount;
    public GameObject movementInfo;
    
    //Action conditions (Predicates)
    public bool CanShoot() => currentOrder == OrderState.Engage && HasValidShootTarget(npoManager.playerUnits);
    public bool CanFight() => inControlRange();
    public bool CanCharge() => possibleChargeTarget();
    public bool Fallback() => inDanger() && !coverNear();

    private void Awake()
    {
        currentWounds = npoData.WOUNDS;
        currentAPL = npoData.APL;
        movementStat = npoData.MOVE;
        npoManager = GetComponentInParent<NPOManager>();
        acted = false;
        
        Transform spHolder = gameObject.transform.Find("BaseSamplingPoints");
        foreach (Transform pt in spHolder)
        {
            samplingPoints.Add(pt.gameObject);
        }
    }
    
    private void Update()
    {
        closeToCover = CheckDistanceToTerrain();
        FogEffectAndCoverToggle();
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
        if (npoData == null || npoData.weapons == null || npoData.weapons.Count == 0)
        {
            Debug.Log($"{name} has not weapons");
            acted = true;
            return;
        }

        WeaponTemplate chosenWeapon = null;
        int highestDamage = -1;
        
        foreach (var weapon in npoData.weapons)
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

    public bool IsValidTarget(PrototypeUnit target)
    {
        if (target == null || target.dead || !target.gameObject.activeInHierarchy) return false;
        if (!HasLineOfSight(target)) return false;
        if (target.concealed && target.closeToCover) return false;
        return CheckTargetingLinesToPlayer(target);
    }

    private bool HasLineOfSight(PrototypeUnit target)
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
    
    public bool CheckTargetingLinesToPlayer(PrototypeUnit player)
    {
        if (samplingPoints.Count == 0 || player.samplingPoints.Count == 0) return false;

        for (int i = 0; i < samplingPoints.Count; i++)
        {
            Vector3 npoSP = samplingPoints[i].transform.position;
            for (int j = 0; j < player.samplingPoints.Count; j++)
            {
                Vector3 playerSP1 = player.samplingPoints[j].transform.position;

                if (CheckLineObstruction(npoSP, playerSP1))
                {
                    int oppositeIndex = (j + (player.samplingPoints.Count / 2)) % player.samplingPoints.Count;
                    Vector3 playerSP2 = player.samplingPoints[oppositeIndex].transform.position;

                    if (CheckLineObstruction(npoSP, playerSP2))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CheckLineObstruction(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        RaycastHit[] hits = Physics.RaycastAll(start, direction, distance);

        foreach (RaycastHit hit in hits)
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
        if (currentWounds < npoData.WOUNDS)
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

    private void FogEffectAndCoverToggle()
    {
        if (closeToCover)
        {
            unitUI.gameObject.transform.Find("StatusIcons").gameObject.transform.Find("Cover").gameObject.SetActive(true);
        }
    }
    
    public bool CheckDistanceToTerrain()
    {
        float oneInchScaled = (1f / 39.37f) * 10f; 
        bool isNearTerrain = false;

        for (int i = 0; i < samplingPoints.Count; i++)
        {
            Vector3 startPos = samplingPoints[i].transform.position;
            Vector3 direction = samplingPoints[i].transform.right;
            
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, oneInchScaled))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(startPos, hit.point, Color.magenta, 0.5f);
                    isNearTerrain = true;
                }
            }
            else
            {
                Debug.DrawLine(startPos, startPos + (direction * oneInchScaled), Color.yellow, 0.5f);
            }
        }
        return isNearTerrain;
    }
}
