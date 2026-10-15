using System.Collections.Generic;using UnityEngine;

[CreateAssetMenu(fileName = "OperativeTemplate", menuName = "Scriptable Objects/OperativeTemplate")]
//-----OperativeTemplate Class (Scriptable object of an operative)------
public class OperativeTemplate : ScriptableObject
{
    //Stats
    public int apl, move, save, wounds;
    
    //Keywords such as Imperium, Astartes, Warrior etc
    public Keywords keywords;
    
    //Active abilities
    [SerializeReference, SubclassSelector]
    public List<IActive> aabilities = new List<IActive>();
    
    //Passive abilities
    [SerializeReference, SubclassSelector]
    public List<IPassive> pabilities = new List<IPassive>();
    
    //Loadout of weapons
    public List<WeaponTemplate> weapons = new List<WeaponTemplate>();
}



//Keywords such as Imperium, Astartes, Warrior etc
[System.Flags]
public enum Keywords
{
    None = 0,
    Imperium = 1 << 0,
    AdeptusAstartes = 1 << 1,
    Leader = 1 << 2,
    SpaceMarineCaptain = 1 << 3,
    AngelOfDeath = 1 << 4,
    AssaultIntercessor = 1 << 5,
    Sergeant = 1 << 6,
    Intercessor = 1 << 7,
    Grenadier = 1 << 8,
    Warrior = 1 << 9,
    HeavyIntercessor = 1 << 10,
    Gunner = 1 << 11,
    Eliminator = 1 << 12,
    Sniper = 1 << 13,
}