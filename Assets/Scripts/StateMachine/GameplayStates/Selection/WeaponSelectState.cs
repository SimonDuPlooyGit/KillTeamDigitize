using UnityEngine;

public class WeaponSelectState : BaseState
{
    private readonly InputActions _input;
    private readonly MenuPanel _menu;

    public WeaponSelectState(InformationPackage context, InputActions input, MenuPanel menu) : base(context)
    {
        _input = input;
        _menu = menu;
    }
    public override void OnEnter()
    {
        Debug.Log("WeaponSelectState entered");
        PopulateWeaponPanel();
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Debug.Log("WeaponSelectState exited");

        if (_menu.shootMenuActualScript != null)
        {
            _menu.shootMenuActualScript.ClearWeapons();
        }
    }

    public void PopulateWeaponPanel()
    {
        _menu.shootMenuActualScript.ClearWeapons();
        
        if (Context.activatedUnitSO != null && Context.activatedUnitSO.weapons != null)
        {
            Debug.Log(Context.activatedUnitSO.weapons.Count.ToString());
            // Loop through the unit's weapons and tell the UI to create a panel for each
            foreach (WeaponTemplate weapon in Context.activatedUnitSO.weapons)
            {
                _menu.shootMenuActualScript.AddWeaponPanel(weapon, OnWeaponClicked);
            }
        }
        else
        {
            Debug.LogWarning("No activated unit or ranged weapons found in Context!");
        }
    }
    
    private void OnWeaponClicked(WeaponTemplate chosenWeapon)
    {
        Debug.Log($"Selected Weapon: {chosenWeapon.name}");
        
        // 1. Record the choice in our shared data packet
        Context.weapon = chosenWeapon;
        
        // 2. Set our state transition flag to true!
        Context.isWeaponSelected = true;
    }
}
