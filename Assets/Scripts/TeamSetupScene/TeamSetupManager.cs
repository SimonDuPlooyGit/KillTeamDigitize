using UnityEngine;
using System.Collections.Generic;

public class TeamSetupManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dropdownRoot;        // The main "OperativeButtons" panel GameObject
    public Transform contentContainer;     // The container holding your pre-existing buttons
    public List<OperativeConfigureUI> teamSlots;

    private void Start()
    {
        if (dropdownRoot != null)
            dropdownRoot.SetActive(false);

        foreach (var slot in teamSlots)
        {
            slot.Initialize(this);
        }
    }

    public void OpenOperativeSelectionDropdown(OperativeConfigureUI slot)
    {
        if (dropdownRoot != null)
            dropdownRoot.SetActive(true);

        // Find all pre-existing buttons in the container and bind them to this specific slot
        OperativeButtonReference[] buttonRefs = contentContainer.GetComponentsInChildren<OperativeButtonReference>(true);
        foreach (var btnRef in buttonRefs)
        {
            btnRef.InitializeButton(slot, this);
        }
    }

    public void CloseDropdown()
    {
        if (dropdownRoot != null)
            dropdownRoot.SetActive(false);
    }
}