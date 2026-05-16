using UnityEngine;

public interface IWeaponRule
{
    AttackStep Step { get; } //Properties of c#
    
    void Execute(AttackPackage context);
}
