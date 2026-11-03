using UnityEngine;
using UnityEngine.UI;

public class OperativeButtonReference : MonoBehaviour
{
    [Header("Assign in Inspector for this Button")]
    public OperativeTemplate operativeTemplate;
    public Sprite operativePortrait;

    [HideInInspector] public OperativeConfigureUI assignedSlot;
    private TeamSetupManager setupManager;
    private Button buttonComponent;

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(OnButtonClicked);
        }
    }

    // Called by the manager when the dropdown opens to link buttons to the active slot
    public void InitializeButton(OperativeConfigureUI slot, TeamSetupManager manager)
    {
        assignedSlot = slot;
        setupManager = manager;
    }

    public void OnButtonClicked()
    {
        if (assignedSlot != null && operativeTemplate != null)
        {
            assignedSlot.AssignOperative(operativeTemplate, operativePortrait);
            
            if (setupManager != null)
            {
                setupManager.CloseDropdown();
            }
        }

        assignedSlot.selectOperativeButton.gameObject.SetActive(false);
    }
}