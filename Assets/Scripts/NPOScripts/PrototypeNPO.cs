using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum UnitArchetype {Brawler, Marksman}
public enum OrderState {Engage, Conceal}

public class PrototypeNPO : MonoBehaviour
{
    //Initializing
    public OperativeTemplate npoData;
    public GameObject unitGhost;
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

    [Header("Unit Settings")]
    public UnitArchetype archetype = UnitArchetype.Marksman;
    public OrderState currentOrder = OrderState.Engage;
    public bool closeToCover;
    
    //Sampling data for cover and targeting lines
    public List<GameObject> samplingPoints;
    
    //Line of sight variables
    public Transform losStart; //assigned in inspector
    
    //Movement and navmesh
    private NavMeshAgent agentGhost;
    private NavMeshAgent agentUnit;
    public NavMeshPath path;
    public LineRenderer lineRenderer;
    private bool pathDrawn = false;
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
}
