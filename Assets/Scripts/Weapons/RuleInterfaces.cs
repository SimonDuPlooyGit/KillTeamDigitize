using UnityEngine;

public interface IWeaponRule
{
    AttackTimings Step { get; } //Properties of c#
    
    void Execute(InformationPackage context); //Execute the rule with attack context
}
