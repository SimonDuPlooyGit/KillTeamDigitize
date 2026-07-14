using Unity.VisualScripting;
using UnityEditor.Timeline;
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

    public void AddWeaponPanel(WeaponTemplate weapon)
    {
        GameObject newObj = Instantiate(weapPanel, weapHolder.transform);
        WeaponPanel panel = newObj.GetComponent<WeaponPanel>();

        panel.name.text = weapon.name;
        panel.atk.text = weapon.ATK.ToString();
        panel.hit.text = weapon.HIT.ToString() + "+";
        panel.dmg.text = $"{weapon.DMGnorm}/{weapon.DMGcrit}";
        
        string rulesText = "";
        foreach(var rule in weapon.rules)
        {
            rulesText += rule.GetType().Name + " ";
        }
        panel.wr.text = rulesText;
    }
}
