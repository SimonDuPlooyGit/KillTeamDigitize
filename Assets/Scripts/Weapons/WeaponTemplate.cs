using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponTemplate : ScriptableObject
{
    //Stats
    public int ATK, HIT, DMGnorm, DMGcrit;
    
    //Rules
    [SerializeReference, SubclassSelector]
    public List<IWeaponRule> rules = new List<IWeaponRule>();
}
