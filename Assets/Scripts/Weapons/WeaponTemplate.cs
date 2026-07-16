using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")] //Allows the creation of a weapon in the right click menu in Unity
public class WeaponTemplate : ScriptableObject //Scriptable object of a weapon
{
    //Stats
    public int ATK, HIT, DMGnorm, DMGcrit;
    
    //Rules
    [SerializeReference, SubclassSelector]
    public List<IWeaponRule> rules = new List<IWeaponRule>();
}
