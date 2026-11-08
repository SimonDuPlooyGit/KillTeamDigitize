using System;
using Unity.VisualScripting;
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
    public GameObject tutSelect, tutAction, tutReposition, tutRepoPrompt ,tutShoot, tutTarget, tutInvalid, tutReset;
    public GameObject moveButton2;
    public GameObject diceRollMenu;
    public GameObject meleeMenu;
    public GameObject PauseScreen;
    public GameObject tpCounter, objective;
    public GameObject winScreen, loseScreen;
    [Header("Pop ups")]
    public GameObject invTarget;
    public GameObject concTarget;
    public GameObject invCharge;
    public GameObject fightTarget;
    public GameObject heavyPopup;
    public GameObject tooClose;
    public GameObject inRange;
    [SerializeField]
    private GameObject weaponTooltipHolder;
    
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
        //Debug.Log($"Opening menu: {menu.name}");
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

    public void PopUp(GameObject panel)
    {
        if(panel != null)
        {
            panel.gameObject.SetActive(true);
        }
    }

}
