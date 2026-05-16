using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponTemplate : ScriptableObject
{
    public int ATK, HIT, DMGnorm, DMGcrit;
    [SerializeReference, SubclassSelector]
    public List<IWeaponRule> rules = new List<IWeaponRule>();
}
