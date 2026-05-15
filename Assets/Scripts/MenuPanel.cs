using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject actionMenu;
    void Start()
    {
        actionMenu.transform.localScale = Vector3.zero;
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
}
