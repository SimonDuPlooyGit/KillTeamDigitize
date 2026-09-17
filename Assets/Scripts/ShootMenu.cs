using System;
using UnityEngine;

public class ShootMenu : MonoBehaviour
{
    [SerializeField] public GameObject weapHolder;
    [SerializeField] public GameObject weapPanel;

    public void ClearWeapons()
    {
        Transform shootHeader = weapHolder.transform.Find("ShootMenu");
        Transform shootFooter = weapHolder.transform.Find("ShootFooter");
        foreach (Transform child in weapHolder.transform)
        {
            if (child != shootHeader && child !=shootFooter)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void AddWeaponPanel(WeaponTemplate weapon, Action<WeaponTemplate> onWeaponSelected)
    {
        int lastIndex = weapHolder.transform.childCount - 1;
        GameObject panelObj = Instantiate(weapPanel, weapHolder.transform);
        panelObj.transform.SetSiblingIndex(lastIndex-1);
        WeaponPanel panel = panelObj.GetComponent<WeaponPanel>();
        
        panel.Setup(weapon, onWeaponSelected);
    }
}
