using System;
using UnityEngine;

public class ShootMenu : MonoBehaviour
{
    [SerializeField] public GameObject weapHolder;
    [SerializeField] public GameObject weapPanel;

    public void ClearWeapons()
    {
        Transform shootHeader = weapHolder.transform.Find("ShootMenu");
        foreach (Transform child in weapHolder.transform)
        {
            if (child != shootHeader)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void AddWeaponPanel(WeaponTemplate weapon, Action<WeaponTemplate> onWeaponSelected)
    {
        GameObject panelObj = Instantiate(weapPanel, weapHolder.transform);
        WeaponPanel panel = panelObj.GetComponent<WeaponPanel>();
        
        panel.Setup(weapon, onWeaponSelected);
    }
}
