using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    public class RuleToPrefab //workaround to get a disctionary in inspector for the weapon rule prefabs
    {
        public string keyword;       
        public GameObject weaponRule;   
    }
    [SerializeField]
    private GameObject[] tooltips;
    [SerializeField]
    private GameObject panelContainer;
    private Vector3 containerStartPosition = new Vector3 (-877.0505f, 0, 0);
    private GameObject weaponTooltipHolder;
    private List<GameObject> activePanels = new List<GameObject>();
    [SerializeField] 
    private TextMeshProUGUI weaponRuleText; 
    [SerializeField] 
    private List<RuleToPrefab> ruleDatabase;
    [SerializeField]
    private bool isRuleText = false;
    private GameObject scrollArrows;

    private void Awake()
    {
        if(isRuleText)
        {
            panelContainer = GameObject.FindWithTag("RuleContainer");
        }

        scrollArrows = GameObject.Find("ToolTipArrows");
        weaponTooltipHolder = GameObject.Find("WeaponRuleHolder");
    }

    private void OnEnable()
    {
        MenuPanel.OnCloseMenu += ClearTooltips;
    }

    private void OnDisable()
    {
        MenuPanel.OnCloseMenu -= ClearTooltips;
        ClearTooltips();
         if (scrollArrows.transform.localScale == Vector3.one)
        {
            scrollArrows.transform.localScale = Vector3.zero;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (panelContainer == null) return;
        if(weaponTooltipHolder != null)
        {
            weaponTooltipHolder.transform.localPosition = containerStartPosition;
        }
        ClearTooltips();
        if (!isRuleText)
        {
            AddTooltips();
        }
        else
        {
            AddRuleTooltips();
        }

        if(panelContainer.transform.childCount >=4)
        {
            scrollArrows.transform.localScale = Vector3.one;
        }
       
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ClearTooltips();
        if (scrollArrows.transform.localScale == Vector3.one)
        {
            scrollArrows.transform.localScale = Vector3.zero;
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddTooltips()
    {
        for (int i = 0; i < tooltips.Length; i++)
        {
            if (tooltips[i] == null) continue;
            GameObject toolTipPanel = Instantiate(tooltips[i],panelContainer.transform);
            activePanels.Add(toolTipPanel);
        }
    }

    private void AddRuleTooltips()
    {
        if (weaponRuleText == null) return;

        string sourceText = weaponRuleText.text;
        
        foreach (RuleToPrefab pair in ruleDatabase)
        {
            if (string.IsNullOrEmpty(pair.keyword) || pair.weaponRule == null)
                continue;
            
            if (sourceText.IndexOf(pair.keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                GameObject toolTipPanel = Instantiate(pair.weaponRule, panelContainer.transform, false);
                activePanels.Add(toolTipPanel);
            }
        }
    }

    public void ClearTooltips()
    {
        for (int i = activePanels.Count - 1; i >= 0; i--)
        {
            GameObject panel = activePanels[i];
            if (panel != null)
            {
                
                panel.transform.SetParent(null); //detatches the panel from its holder so that the backup clean method can work
                Destroy(panel);
            }
        }

        activePanels.Clear();
    }

}
