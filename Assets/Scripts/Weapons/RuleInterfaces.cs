public interface IWeaponRule
{
    AttackTimings Step { get; } //Get-only auto property have to provide a public way to read this enum variable
    
    void Execute(InformationPackage context); //Execute the rule with attack context
}
