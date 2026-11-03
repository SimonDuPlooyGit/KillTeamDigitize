using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OperativeConfigureUI : MonoBehaviour
{
    [Header("UI References")]
    public Button selectOperativeButton; 
    public Button selectWeaponsButton;   
    public Image portraitImage;          
    public TextMeshProUGUI nameText;     
    public TextMeshProUGUI weaponsText;  
    
    [Header("Stats Text References")]
    public TextMeshProUGUI aplText;
    public TextMeshProUGUI moveText;
    public TextMeshProUGUI saveText;
    public TextMeshProUGUI woundsText;

    [HideInInspector] public OperativeTemplate selectedOperativeTemplate;
    [HideInInspector] public List<WeaponTemplate> availableWeapons = new List<WeaponTemplate>();
    
    private TeamSetupManager setupManager;
    private bool weaponsListVisible = false;
    
    public void Initialize(TeamSetupManager manager)
    {
        setupManager = manager;
        
        if (selectOperativeButton != null)
            selectOperativeButton.onClick.AddListener(OnSelectOperativeClicked);
            
        if (selectWeaponsButton != null)
            selectWeaponsButton.onClick.AddListener(OnSelectWeaponsClicked);
    }

    private void OnSelectOperativeClicked()
    {
        Debug.Log("Select operative was clicked");
        setupManager.OpenOperativeSelectionDropdown(this);
    }

    private void OnSelectWeaponsClicked()
    {
        if (selectedOperativeTemplate == null)
        {
            weaponsText.text = "Select an operative first!";
            return;
        }

        weaponsListVisible = !weaponsListVisible;
        if (weaponsListVisible)
        {
            string weaponListString = "";
            foreach (var weapon in availableWeapons)
            {
                weaponListString += $"- {weapon.name}\n";
            }
            weaponsText.text = weaponListString.TrimEnd();
        }
        else
        {
            weaponsText.text = "-";
        }
    }

    public void AssignOperative(OperativeTemplate template, Sprite portraitSprite)
    {
        selectedOperativeTemplate = template;

        if (template != null)
        {
            nameText.text = template.name;
            aplText.text = $"{template.apl}";
            moveText.text = $"{template.move}";
            saveText.text = $"{template.save}";
            woundsText.text = $"{template.wounds}";
            
            availableWeapons = new List<WeaponTemplate>(template.weapons);
            
            if (portraitImage != null && portraitSprite != null)
            {
                portraitImage.gameObject.SetActive(true);
                portraitImage.sprite = portraitSprite;
            }

            weaponsText.text = "Click 'Select Weapons' to view";
        }
        
        // Close dropdown after selection
        if (setupManager != null)
        {
            setupManager.CloseDropdown();
        }
    }
}