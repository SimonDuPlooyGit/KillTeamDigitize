using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.UI;

public class PrototypeUnit : MonoBehaviour
{
    public OperativeTemplate operativeData;
    private NavMeshAgent agentUnit;
    private NavMeshAgent agentGhost;
    public float movementStat;
    private float meterMovement;
    private float pathDistance;
    public NavMeshPath path;
    public GameObject unitGhost;
    public LineRenderer lineRenderer;
    private Vector3[] points;
    private bool pathDrawn = false;
    List<Vector3> limitedPoints = new List<Vector3>();
    public bool selected = false;
    public int currentWounds;
    public bool dead = false;
    //Unit UI healthbar variables
    public GameObject healthFill;
    public GameObject aplCount;

    private void Awake()
    {
        movementStat = operativeData.MOVE;
        meterMovement = (movementStat/39.37f) * 10; //Changing the inches to meters and then applying 10x Scale.
        agentUnit = GetComponent<NavMeshAgent>();
        agentGhost = unitGhost.GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        unitGhost.SetActive(false);
        currentWounds = operativeData.WOUNDS;
        healthFill = gameObject.transform.Find("UnitUI").Find("HealthBar").Find("HealthFill").gameObject;
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

            if (pathDistance > meterMovement)
            {
                agentGhost.destination = LimitPath(path, meterMovement);
            }
            else
            {
                limitedPoints.Add(transform.position);
                limitedPoints.Add(hit.point);
                agentGhost.destination = hit.point;
            }

            points = path.corners;
        }
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

    public void moveUnitToGhost()
    {
        agentUnit.destination = unitGhost.transform.position;

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
}
