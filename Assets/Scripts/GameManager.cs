using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputActions input;
    public GameObject currentlySelectedUnit;

    private void Awake()
    {
        input = new InputActions();
        AssignInputs();
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
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
            }
        }
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
