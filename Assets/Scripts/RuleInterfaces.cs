using UnityEngine;

public interface IWeaponRule
{
    GameLoopTiming Step { get; } //Properties of c#
    
    void Execute(AttackPackage context); //Execute the rule with attack context
}
