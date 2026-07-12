using UnityEngine;

public interface IActive
{
    AttackTimings Step { get; } //Properties of c# - Get at what timing this rule applies

    void Execute(InformationPackage context); //Execute active ability logic
}

[System.Serializable]
public class Optics : IActive
{
    public AttackTimings Step => AttackTimings.AfterRoll; //This timing needs to change since it can get paid as an action

    public void Execute(InformationPackage context)
    {
        //Until the start of this ops next activation. whenever it is shooting enemies can't be obscured
        //Can't be done in control range of an enemy
    }
}