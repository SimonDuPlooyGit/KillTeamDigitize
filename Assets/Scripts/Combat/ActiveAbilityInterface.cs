public interface IActive
{
    AttackTimings Step { get; } //Properties of c# - Get at what timing this rule applies

    void Execute(InformationPackage context); //Execute active ability logic
}

//This is the only active ability in the game WE ARE GOING TO CUT THIS
//We are cutting this because we aren't implementing OBSCURED
[System.Serializable]
public class Optics : IActive
{
    public AttackTimings Step => AttackTimings.PreRoll; //This timing needs to change since it can get paid as an action

    public void Execute(InformationPackage context)
    {
        //Until the start of this ops next activation. whenever it is shooting enemies can't be obscured
        //Can't be done in control range of an enemy
    }
}