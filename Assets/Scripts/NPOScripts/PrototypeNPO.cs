using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("Unit Settings")]
    public UnitArchetype archetype = UnitArchetype.Marksman;
    public OrderState currentOrder = OrderState.Conceal;
    public bool closeToCover;
    
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
    public bool CanShoot() => currentOrder == OrderState.Engage && hasValidShootTarget();
    public bool CanFight() => inControlRange();
    public bool CanCharge() => possibleChargeTarget();
    public bool Fallback() => inDanger() && coverNear();

    private void Awake()
    {
        Transform spHolder = gameObject.transform.Find("BaseSamplingPoints");
        foreach (Transform pt in spHolder)
        {
            samplingPoints.Add(pt.gameObject);
        }
    }
    
    private void Update()
    {
        closeToCover = CheckDistanceToTerrain();
        fogEffectAndCoverToggle();
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
        
    }

    public void PerformFallback()
    {
        
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

    private bool hasValidShootTarget()
    {
        return true;
    }

    private bool inControlRange()
    {
        return true;
    }

    private bool possibleChargeTarget()
    {
        return true;
    }

    private bool inDanger()
    {
        return true;
    }

    private bool coverNear()
    {
        return true;
    }

    private void fogEffectAndCoverToggle()
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
