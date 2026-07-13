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
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Debug.Log("WeaponSelectState exited");
    }

    public void PopulateWeaponPanel()
    {
        //_menu.shootMenu.addWeaponPanel();
    }
}
