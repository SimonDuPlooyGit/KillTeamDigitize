using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject actionMenu;
    [SerializeField] public GameObject shootMenuHolder;
    [SerializeField]
    private GameObject tooltipHolder;

    public ShootMenu shootMenuActualScript;
    
    void Start()
    {
        shootMenuActualScript = shootMenuHolder.GetComponentInChildren<ShootMenu>();
        actionMenu.transform.localScale = Vector3.zero;
        shootMenuHolder.transform.localScale = Vector3.zero;
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
            Vector3 newPosition = new Vector3(-75f,470f,0f);
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
            Vector3 newPosition = new Vector3(-542f, 224f, 0f);
            tooltipHolder.transform.localPosition = newPosition;
        }
    }

}
