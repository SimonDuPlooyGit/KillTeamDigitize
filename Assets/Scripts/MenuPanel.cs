using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject actionMenu;
    [SerializeField]
    private GameObject shootMenu;
    void Start()
    {
        actionMenu.transform.localScale = Vector3.zero;
        shootMenu.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        
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

    }

    public void CloseMenu(GameObject menu)
    {
        if (menu.transform.localScale == Vector3.one)
        {
            menu.transform.localScale = Vector3.zero;
        }
    }
}
