using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//-----WeaponSelectState (When the player selects weapons from the weapon menu)-----
public class WeaponSelectState : BaseState
{
    //Inherits from BaseState
    //Weapon selection state that handles player's picking a weapon to shoot or fight with in the weapon menu
    
    private readonly MenuPanel _menu; //Needs access to the menu from GameManager

    public WeaponSelectState(InformationPackage context, MenuPanel menu) : base(context)
    {
        _menu = menu;
    }
    
    public override void OnEnter()
    {
        //Context.CurrentlySelectedOperativeScript.currentOrder = OrderState.Engage;
        Debug.Log("WeaponSelectState entered");
        _menu.OpenMenu(_menu.shootMenuHolder);
        PopulateWeaponPanel(); //When this state starts we need to weapon panel to be populated with the appropriate weapons
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
        _menu.ClearTooltipsBackup();
        //Context.StateFlagsReset();
    }

    private void PopulateWeaponPanel() //Instantiate weapon panel prefabs into the shooting menu with the weapons that this unit has
    {
        _menu.shootMenuActualScript.ClearWeapons(); //Clear previous weapons for fresh slate
        
        if (Context.CurrentlySelectedOperativeSo != null && Context.CurrentlySelectedOperativeSo.weapons != null) //Safety null check
        {
            foreach (WeaponTemplate weapon in Context.CurrentlySelectedOperativeSo.weapons) //Loop through the unit's weapons and tell the UI to create a panel for each
            {
                if (Context.IsShootingRequested)
                {
                    //Debug.Log("Only populating ranged weapons");
                    if (weapon.rules.OfType<WeaponRules.Melee>().Any()) continue;
                    _menu.shootMenuActualScript.AddWeaponPanel(weapon, OnWeaponClicked);
                } else if (Context.IsFightingRequested)
                {
                    //Debug.Log("Only populating melee weapons");
                    if (weapon.rules.OfType<WeaponRules.Melee>().Any())
                    {
                        _menu.shootMenuActualScript.AddWeaponPanel(weapon, OnWeaponClicked);
                    }
                }
                else
                {
                    Debug.Log("IsShootingRequested or IsFightingRequested is false");
                }
                
                
                //^ Call AddWeaponPanel on the shootMenu script with the currently iterated weapon as a parameter
                //Send OnWeaponClicked as a delegate to AddWeaponPanel (Top of a 3 part callback down to WeaponPanel)
            }
            _menu.shootMenuActualScript.AddFooter();
        }
        else
        {
            Debug.LogWarning("No activated unit or ranged weapons found in Context!");
        }
    }
    
    private void OnWeaponClicked(WeaponTemplate chosenWeapon)
    {
        Debug.Log($"Selected Weapon: {chosenWeapon.name}");
        //Remember to ask how to detect melee rule to only play this sound for ranged
        SoundManager.PlaySound(10, 1, true);
        var heavyRule = chosenWeapon.rules.OfType<WeaponRules.Heavy>().FirstOrDefault();
        PlayerOperative playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (heavyRule != null)
        {
            if (playerOp.hasRepositionedThisActivation)
            {
                Debug.Log("Can't shoot this weapon after reposition due to heavy weapon rule");
                SoundManager.PlaySound(7);
                _menu.PopUp(_menu.heavyPopup);
            }
            else
            {
                Context.Weapon = chosenWeapon; //Set the information package weapon to the current weapon selected
                Context.IsWeaponSelected = true; //State transition flag to true
            }
        }
        else
        {
            Context.Weapon = chosenWeapon; //Set the information package weapon to the current weapon selected
            Context.IsWeaponSelected = true; //State transition flag to true
        }
    }
}
