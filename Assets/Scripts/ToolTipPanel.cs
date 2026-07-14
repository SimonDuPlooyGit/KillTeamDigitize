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
    private List<GameObject> activePanels = new List<GameObject>();
    [SerializeField] 
    private TextMeshProUGUI weaponRuleText; 
    [SerializeField] 
    private List<RuleToPrefab> ruleDatabase;
    [SerializeField]
    private bool isRuleText = false;

    private void Awake()
    {
        if(isRuleText)
        {
            panelContainer = GameObject.FindWithTag("RuleContainer");
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isRuleText)
        {
            AddTooltips();
        }
        else
        {
            AddRuleTooltips();
        }
       
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ClearTooltips();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddTooltips()
    {
        for (int i = 0; i < tooltips.Length; i++)
        {
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

    private void ClearTooltips()
    {
        foreach(GameObject panel in activePanels)
        {
            Destroy(panel);
        }
        activePanels.Clear();
    }

   
}
