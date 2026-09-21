using System;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField]
    public GameObject actionMenu;
    [SerializeField] public GameObject shootMenuHolder;
    [SerializeField]
    private GameObject tooltipHolder;
    public ShootMenu shootMenuActualScript;
    public static event Action OnCloseMenu;
    public GameObject tutSelect, tutAction, tutReposition, tutShootPrompt ,tutShoot, tutTarget;
    public GameObject moveButton2;
    public GameObject diceRollMenu;
    public GameObject tpCounter, objective;
    [SerializeField]
    private GameObject weaponTooltipHolder;
    [Header("Tutorial flags")]
    public bool tutFlag1 = true; //flag to show the select unit tut panel
    public bool tutFlag2 = true; //flag to show tut panels during action select 
    public bool tutFlag3 = true; //flag to show reposition tut panel
    public bool tutFlag4 = false; //flag to show prompt for clicking shoot in action select
    public bool tutFlag4B = true; //I know its sloppy but flag to make sure Flag4 is only set to true once
    public bool tutFlag5 = true; //flag for both shooting and targeting panels
    
    void Start()
    {
        shootMenuActualScript = shootMenuHolder.GetComponentInChildren<ShootMenu>();
        actionMenu.transform.localScale = Vector3.zero;
        shootMenuHolder.transform.localScale = Vector3.zero;
        diceRollMenu.transform.localScale = Vector3.zero;
    }
    
    //Sets the scale of the menu to one if its currently zero
    public void OpenAction()
    {
        if(actionMenu.transform.localScale == Vector3.zero)
        {
            actionMenu.transform.localScale = Vector3.one;
        }
  
    }

    //Sets the scale of the menu to zero if its currently one
    public void CloseAction()
    {
        if (actionMenu.transform.localScale == Vector3.one)
        {
            actionMenu.transform.localScale = Vector3.zero;
        }
    }

    public void OpenMenu(GameObject menu)
    {
        if (menu.transform.localScale == Vector3.zero)
        {
            menu.transform.localScale = Vector3.one;
        }

        if(menu == shootMenuHolder)
        {
            Vector3 newPosition = new Vector3(-75f, 450f, 0f);
            tooltipHolder.transform.localPosition = newPosition;
        }

    }

    public void CloseMenu(GameObject menu)
    {
        if (menu.transform.localScale == Vector3.one)
        {
            menu.transform.localScale = Vector3.zero;
        }

        if (menu == shootMenuHolder)
        {
            Vector3 newPosition = new Vector3(-542f, 200f, 0f);
            tooltipHolder.transform.localPosition = newPosition;
        }

        OnCloseMenu?.Invoke();
    }

    public void ClearTooltipsBackup()
    {
        if (weaponTooltipHolder == null) return;

        Transform holder = weaponTooltipHolder.transform;
        for (int i = holder.childCount - 1; i >= 0; i--)
        {
            Transform child = holder.GetChild(i);
            if (child != null)
            {
                // Unparent first so Unity doesn't throw the RectTransform dependency error
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }
    }

}
