using UnityEngine;

public class WeaponSelectState : BaseState
{
    //Inherits from BaseState
    //Weapon selection state that handles player's picking a weapon to shoot or fight with in the weapon menu
    
    private readonly MenuPanel _menu; //Needs access to the menu from GameManager

    public WeaponSelectState(InformationPackage context, InputActions input, MenuPanel menu) : base(context)
    {
        _menu = menu;
    }
    public override void OnEnter()
    {
        Debug.Log("WeaponSelectState entered");
        _menu.OpenMenu(_menu.shootMenuHolder);
        PopulateWeaponPanel(); //When this state starts we need to weapon panel to be populated with the appropriate weapons
        _menu.OpenMenu(_menu.tutShoot);
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
            _menu.shootMenuActualScript.ClearWeapons(); //We need the weapon menu to be cleared of weapons when this state is exited
        }
        
        _menu.CloseMenu(_menu.shootMenuHolder);
        _menu.CloseMenu(_menu.tutShoot);
    }

    public void PopulateWeaponPanel() //Instantiate weapon panel prefabs into the shooting menu with the weapons that this unit has
    {
        _menu.shootMenuActualScript.ClearWeapons(); //Clear previous weapons for fresh slate
        
        if (Context.activatedUnitSO != null && Context.activatedUnitSO.weapons != null) //Safety null check
        {
            foreach (WeaponTemplate weapon in Context.activatedUnitSO.weapons) //Loop through the unit's weapons and tell the UI to create a panel for each
            {
                _menu.shootMenuActualScript.AddWeaponPanel(weapon, OnWeaponClicked);
                //^ Call AddWeaponPanel on the shootMenu script with the currently iterated weapon as a parameter
                //Send OnWeaponClicked as a delegate to AddWeaponPanel (Top of a 3 part callback down to WeaponPanel)
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
        Context.weapon = chosenWeapon; //Set the information package weapon to the current weapon selected
        Context.isWeaponSelected = true; //State transition flag to true
    }
}
