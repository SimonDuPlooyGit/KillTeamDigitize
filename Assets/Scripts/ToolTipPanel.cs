using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject[] tooltips;
    [SerializeField]
    private GameObject panelContainer;
    private List<GameObject> activePanels = new List<GameObject>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        AddTooltips();
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

    private void ClearTooltips()
    {
        foreach(GameObject panel in activePanels)
        {
            Destroy(panel);
        }
        activePanels.Clear();
    }

   
}
