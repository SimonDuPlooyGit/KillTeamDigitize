using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum OrderState {Engage, Conceal}
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
    public OrderState currentOrder = OrderState.Engage;
    public float dashDistanceScaled = (3f / 39.37f) * 10;
    public bool concealed => currentOrder == OrderState.Conceal;
    
    [Header("Sampling and LOS")]
    public List<GameObject> samplingPoints = new List<GameObject>();
    public Transform losStart;
    public LineRenderer lineRenderer;
    public Material validLine;
    public Material invalidLine;

    [Header("UI References")] 
    public GameObject unitUI;
    public GameObject healthFill;
    public GameObject aplCount;
    public GameObject concealSymbol;
    public GameObject engageSymbol;
    public GameObject injuredSymbol;
    public GameObject concealInCoverSymbol;
    public GameObject damagePopOut;

    [Header("Materials To Apply")]
    public GameObject fogEffect;
    public Material glow;

    [Header("Animation")]
    [SerializeField]
    private ParticleSystem deathParticles;
    private Animator animator;
    

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        
        fogEffect.SetActive(false);
        
        SetHealth();
        
        //Null check and initialization
        if (operativeData != null)
        {
            currentWounds = operativeData.WOUNDS;
            currentApl = operativeData.APL;
            movementStat = operativeData.MOVE;
            meterMovement = (movementStat / 39.37f) * 10;
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
        
        closeToCover = CheckDistanceToTerrain();
        FogEffectAndCoverToggle();
    }

    //Check LOS between a unit's head LOS start to another unit's LOS start (losEnd in this)
    public float DetermineLOSandDistance(Transform losEnd)
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
                Debug.Log($"LOS Clear. Total range to target: {distance}");
            }
            else
            {
                lineRenderer.material = invalidLine;
                lineRenderer.SetPosition(1, hit.point);
                Debug.Log($"LOS Blocked by {hit.collider.name}. Total range to target: {distance}");
                distance = 0;
            }
        }

        lineRenderer.enabled = true;

        return distance;
    }

    //Check a 1-inch area around the operative to determine if terrain is within 1 inch
    public bool CheckDistanceToTerrain()
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

    //Check the targeting lines rule two with argument overload based on what you can input
    public bool CheckTargetingLines(BaseOperativeClass target)
    {
        return CheckTargetingLines(this, target);
    }

    public bool CheckTargetingLines(BaseOperativeClass attacker, BaseOperativeClass target)
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
                        Debug.DrawLine(startSp, targetSp1, Color.cyan, 10f);
                        Debug.DrawLine(startSp, targetSp2, Color.cyan, 10f);
                        return true;
                    }
                }
            }
        }
        return false;
    }
    
    //Check for terrain blockers in the targeting lines rule
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
                Debug.DrawLine(start, hit.point, Color.red, 0.5f);
                return false;
            }
        }
        Debug.DrawLine(start, end, Color.green, 0.5f);
        return true;
    }

    //Do damage communication and effects in here
    public void TakeDamage(int damage)
    {
        currentWounds -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentWounds}");

        

        if(animator != null && damage>0)
        {
            animator.SetTrigger("TakeDamage");

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
    
    //Set the health and healthbar
    public void SetHealth()
    {
        if (healthFill == null || operativeData == null) return;
        var img = healthFill.GetComponent<Image>();
        if (img != null) img.fillAmount = 1;
        var textComp = healthFill.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null) textComp.text = operativeData.WOUNDS.ToString();
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

    //Set apl in the UI for the move marker
    public void SetApl(int apl)
    {
        if (aplCount == null) return;
        var textComponent = aplCount.GetComponent<TextMeshProUGUI>();
        if (textComponent != null) textComponent.text = currentApl.ToString();
    }

    //Update the APL on the movemarker
    public void UpdateApl(int currApl)
    {
        SetApl(currApl);
    }
    
    //Do animations or effects of a unit dying in here
    public void HandleDeath()
    {
        StartCoroutine(deathSequence());
    }
    
    private void FogEffectAndCoverToggle()
    {
        if (closeToCover && unitUI != null && currentOrder == OrderState.Conceal)
        {
            concealInCoverSymbol.SetActive(true);
            fogEffect.SetActive(true);
        }
    }

    private IEnumerator deathSequence()
    {
        Debug.Log($"{gameObject.name} has died.");
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(3.0f);
        deathParticles.gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        deathParticles.Play();
        yield return null;
        gameObject.SetActive(false);
    }
}
