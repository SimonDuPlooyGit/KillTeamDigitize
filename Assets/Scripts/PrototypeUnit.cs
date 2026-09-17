using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Unity.VisualScripting;
using TMPro;
using UnityEditor.Timeline;
using UnityEngine.UI;

public class PrototypeUnit : MonoBehaviour
{
    //Initializing
    public OperativeTemplate operativeData;
    public GameObject unitGhost;
    public float movementStat;
    private float meterMovement;
    private float pathDistance;
    public bool selected = false;
    public int currentWounds;
    public int currentAPL;
    public bool dead = false;
    public float remainingMovement;
    
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

    private void Awake()
    {
        movementStat = operativeData.MOVE;
        meterMovement = (movementStat/39.37f) * 10; //Changing the inches to meters and then applying 10x Scale.
        remainingMovement = meterMovement;
        agentUnit = GetComponent<NavMeshAgent>();
        agentGhost = unitGhost.GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        unitGhost.SetActive(false);
        currentWounds = operativeData.WOUNDS;
        currentAPL = operativeData.APL;
        healthFill = gameObject.transform.Find("UnitUI").Find("HealthBar").Find("HealthFill").gameObject;
        movementInfo = gameObject.transform.Find("UnitUI").Find("MovementInfo").Find("MoveNum").gameObject;
        aplCount = gameObject.transform.Find("UnitUI").Find("APL").Find("APLNumber").gameObject;
        SetHealth();
        
    }

    public void UpdatePathDrawing()
    {
        if (unitGhost.activeSelf &&
            !pathDrawn &&
            !agentGhost.pathPending &&
            agentGhost.velocity.sqrMagnitude < 0.01f &&
            agentGhost.remainingDistance <= agentGhost.stoppingDistance &&
            selected)
        {
            DrawPath(limitedPoints.ToArray());
        }
    }

    private void Update()
    {
         if (unitGhost.activeSelf != false &&
             !pathDrawn &&
             !agentGhost.pathPending &&
             agentGhost.velocity.sqrMagnitude < 0.01f &&
             agentGhost.remainingDistance <= agentGhost.stoppingDistance &&
             selected == true)
         {
             DrawPath(limitedPoints.ToArray());
         }
    }

    public void ClickToPathfind()
    {
        limitedPoints.Clear();
        pathDistance = 0f;
        lineRenderer.enabled = false;
        pathDrawn = false;
        unitGhost.SetActive(true);

        if (remainingMovement <= 0.05f)
        {
            Debug.Log($"No remaining movement: {remainingMovement}");
            return;
        }
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100) && selected == true)
        {
            if (NavMesh.CalculatePath(transform.position, hit.point, agentUnit.areaMask, path))
            {
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);
                    pathDistance += segment;
                    //Debug.Log($"Segment {i}: {segment}, Total: {pathDistance}");
                }
            }

            if (pathDistance > remainingMovement)
            {
                agentGhost.destination = LimitPath(path, remainingMovement);
                currentPathDistance = remainingMovement;
            }
            else
            {
                limitedPoints.Add(transform.position);
                limitedPoints.Add(hit.point);
                agentGhost.destination = hit.point;
                currentPathDistance = pathDistance;
            }
        }
        Debug.Log($"Movement left after confirming move: {(remainingMovement - currentPathDistance) * 39.37f / 10}\"");
    }

    private void DrawPath(Vector3[] points)
    {
        if (!pathDrawn)
        {
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
            lineRenderer.enabled = true;
            pathDrawn = true;
        }
    }
    
    Vector3 LimitPath(NavMeshPath path, float maxDistance)
    {
        float distance = 0f;

        limitedPoints.Add(path.corners[0]);

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            float segment = Vector3.Distance(path.corners[i], path.corners[i + 1]);

            if (distance + segment > maxDistance)
            {
                float remaining = maxDistance - distance;

                Vector3 direction = (path.corners[i + 1] - path.corners[i]).normalized;

                Vector3 finalPoint = path.corners[i] + direction * remaining;

                limitedPoints.Add(finalPoint);

                return finalPoint;
            }

            limitedPoints.Add(path.corners[i + 1]);

            distance += segment;
        }

        return path.corners[path.corners.Length - 1];
    }

    public float CalculatePathLength(NavMeshPath pathToCalculate)
    {
        float length = 0f;

        for (int i = 0; i < pathToCalculate.corners.Length - 1; i++)
        {
            length += Vector3.Distance(pathToCalculate.corners[i], pathToCalculate.corners[i + 1]);
        }
        
        return length;
    }

    public void MoveUnitToGhost()
    {
        agentUnit.destination = unitGhost.transform.position;
        remainingMovement -= currentPathDistance;
        currentPathDistance = 0f;
        Reset();
        
        if (transform.position == unitGhost.transform.position)
        {
            Reset();
        }
    }

    public void Reset()
    {
        unitGhost.transform.position = transform.position;
        lineRenderer.enabled = false;
        unitGhost.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentWounds -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Current Health: {currentWounds}");

        if (currentWounds <= 0)
        {
            currentWounds = 0;
            dead = true;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"{gameObject.name} has died!");
    }

    public void SetHealth()
    {
        healthFill.gameObject.GetComponent<Image>().fillAmount = 1;
        healthFill.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = operativeData.WOUNDS.ToString();
    }

    public void SetAPL(int apl)
    {
        aplCount.gameObject.GetComponent<TextMeshProUGUI>().text = apl.ToString();
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        healthFill.gameObject.GetComponent<Image>().fillAmount = currentHealth / maxHealth;
        Image healthFillImage = healthFill.gameObject.GetComponent<Image>();
        healthFill.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = currentHealth.ToString();
        if(healthFillImage.fillAmount <= 0.25f)
        {

            healthFillImage.color = new Color32(245, 32, 0, 255); //red for < 25%
        }
        else if (healthFillImage.fillAmount <= 0.5f)
        {
            healthFillImage.color = new Color32(245, 180, 0, 255); //Orange for < 50%
        }
        else
        {
            healthFillImage.color = new Color32(0, 245, 47, 255); //Green otherwise
        }

    }

    public void UpdateAPL(int currentAPL)
    {
        aplCount.gameObject.GetComponent<TextMeshProUGUI>().text = currentAPL.ToString();
    }

    public float DetermineLOSandDistance(Transform losEnd)
    {
        float distance = (Vector3.Distance(losStart.position, losEnd.position) / 10) * 39.37f; //Converting back to inches
        print(distance + "\"");
        Vector3 direction = (losEnd.position - losStart.position).normalized;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, losStart.position);

        if (Physics.Raycast(losStart.position, direction, out RaycastHit hit, distance))
        {
            lineRenderer.SetPosition(1, hit.point);
            Debug.Log($"LOS Blocked by {hit.collider.name}. Total range to target: {distance}");
            distance = 0;
        }
        else
        {
            lineRenderer.SetPosition(1, losEnd.position);
            Debug.Log($"LOS Clear. Total range to target: {distance}");
        }
        
        lineRenderer.enabled = true;
        
        return distance;
    }
}
