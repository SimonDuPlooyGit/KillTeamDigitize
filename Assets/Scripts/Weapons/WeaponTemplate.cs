using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")] //Allows the creation of a weapon in the right click menu in Unity
//-----WeaponTemplate class (holds stats for a scriptable object weapon)-----
public class WeaponTemplate : ScriptableObject //Scriptable object of a weapon
{
    //Stats
    public int atk, hit, dmgNorm, dmgCrit;
    
    //Rules
    [SerializeReference, SubclassSelector]
    public List<IWeaponRule> rules = new List<IWeaponRule>();
}