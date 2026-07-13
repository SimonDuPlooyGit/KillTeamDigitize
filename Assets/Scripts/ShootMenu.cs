using UnityEngine;

public class ShootMenu : MonoBehaviour
{
    [SerializeField] public GameObject weapHolder;
    [SerializeField] public GameObject weapPanel;

    public void addWeaponPanel()
    {
        //poop
        GameObject panel = Instantiate(weapPanel, weapHolder.transform);
    }
}
