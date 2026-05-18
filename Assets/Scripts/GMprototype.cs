using UnityEngine;
using UnityEngine.InputSystem;

public class GMprototype : MonoBehaviour
{
    private InputActions input;
    public GameObject currentlySelectedUnit;
    [SerializeField]
    private MenuPanel menu;
    

    private void Awake()
    {
        input = new InputActions();
        AssignInputs();
    }

    public void moveSelectedUnit()
    {
        Debug.Log("moveSelectedUnit");
        currentlySelectedUnit.GetComponent<PrototypeUnit>().moveUnitToGhost();
    }

    private void unitSelectCheck()
    {
        //Debug.Log("Checking for selected unit");
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            //Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject.tag == "AllyUnit")
            {
                currentlySelectedUnit = hit.collider.gameObject;
                currentlySelectedUnit.GetComponent<PrototypeUnit>().selected = true;
                menu.OpenAction();
            }
            else if(hit.collider.gameObject.tag != "AllyUnit" || hit.collider.gameObject == null)//probably need to alter this to prevent the menu from closing when clikcing on it
            {
                menu.CloseAction();
            }
            
        }
    }

    private void deselectUnit()
    {
        currentlySelectedUnit.GetComponent<PrototypeUnit>().selected = false;
        currentlySelectedUnit = null;
    }
    
    void AssignInputs()
    {
        input.Controls.Select.performed += ctx => unitSelectCheck();
    }
    
    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }
}
