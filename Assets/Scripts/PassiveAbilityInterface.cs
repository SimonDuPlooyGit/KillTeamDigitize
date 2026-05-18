using UnityEngine;

public interface IPassive
{
    public void ConnectToTimings();
}

[System.Serializable]
public class HeroicLeader : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //Use Firefight Ploy for 0CP if this is the specified AoD excluding command reroll
        //Use the combat doctrine strategy ploy when you activate a friendly AoD op if this op is in the killzone and isn't in control range of an enemy
        //Use adjust doctrine ploy for 0CP if this operative is in the killzone and isn't within control range of an enemy
    }
}

[System.Serializable]
public class IronHalo : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //Once per battle, when an attack dice inflicts normal dmg on this op, you can ignore it
    }
}

[System.Serializable]
public class DoctrineWarfare : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //You can do each below once per battle
        //Whenever you would use the combat doctrine strategy ploy and then select assault, if this op is in the killzone it costs 0CP
        //Whenever you would use the combat doctrine strategy ploy and then select tactical, if this op is in the killzone it costs 0CP
    }
}

[System.Serializable]
public class ChapterVeteran : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //At the end of select operatives step, if this op is selected for deployment, select one additional chapter tactic for it to have for the battle
    }
}

[System.Serializable]
public class Grenadier : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //Can use krak and frag grenades unlimited, hit stat of that improves by 1 if you choose grenades as equipment
    }
}

[System.Serializable]
public class CamoCloak : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //Whenever an opp is shooting this op ignore the saturate weapon rule
        //This unit has the stealthy chapter tactic
        //If you picked stealthy chapter tactic, you can do both its effects
    }
}

[System.Serializable]
public class CTAggressive : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //This op's melee weapons have rending
    }
}

[System.Serializable]
public class CTDueller : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //When this op fights or retaliates each normal success can block one unresolved crit success (unless their weapon has brutal)
    }
}

[System.Serializable]
public class CTResolute : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //You can ignore any change to APL that isn't affected by enemy shock rule
    }
}

[System.Serializable]
public class CTStealthy : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //When this op is shot, and you can retain cover saves, you can retain an additional cover save OR you can retain one cover save as a crit save
        //This isn't cumulative with improved cover saves from vantage terrain
    }
}

[System.Serializable]
public class CTMobile : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //Can perform FALL BACK for 1 AP
        //This op can perform the charge action while within control range of an enemy and can leave that op's control range to do so (but then normal reqs for that move apply)
    }
}

[System.Serializable]
public class CTHardy : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //When this op is shot, defence dice of 5+ are crit successes
        //When this op is retaliating, the first time and attack dice inflicts normal damage of 3 or more, the dice inflicts 1 less damage
    }
}

[System.Serializable]
public class CTSharpshooter : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //When this op is shooting and hasn't performed Charge, Fall Back or Reposition, its bolt weapons have accurate 1 and severe
    }
}

[System.Serializable]
public class CTSiegeSpecialist : IPassive
{
    public GameLoopTiming Step => GameLoopTiming.AfterRoll; //This timing needs to change

    public void ConnectToTimings()
    {
        //This op's ranged weapons have the saturate rule
        //Whenever this op fights or retaliates enemy ops can't assist
    }
}