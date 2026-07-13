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
        
        if (Context.activatedUnit != null && Context.activatedUnit.weapons != null)
        {
            Debug.Log(Context.activatedUnit.weapons.Count.ToString());
            // Loop through the unit's weapons and tell the UI to create a panel for each
            foreach (WeaponTemplate weapon in Context.activatedUnit.weapons)
            {
                _menu.shootMenuActualScript.AddWeaponPanel(weapon);
            }
        }
        else
        {
            Debug.LogWarning("No activated unit or ranged weapons found in Context!");
        }
    }
}
