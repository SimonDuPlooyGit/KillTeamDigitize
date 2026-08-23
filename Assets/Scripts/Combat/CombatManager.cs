using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; //need this to access the Image component

public class CombatManager : MonoBehaviour
{
    //On the combat manager GameObject
    [Header("Dice Prefabs")]
    [SerializeField]
    GameObject allyDicePrefab; //Holds an ally dice prefab
    [SerializeField]
    GameObject enemyDicePrefab; //Holds an enemy dice prefab
    [SerializeField]
    GameObject allyDicePhysical;
    [SerializeField]
    GameObject enemyDicePhysical;
    [Header("Holders/Transforms")]
    [SerializeField]
    GameObject attackDiceHolder; //The horizontal layout group for the attack dice prefabs
    [SerializeField]
    GameObject defenseDiceHolder; //The horizontal layout group for the defense dice prefabs
    public List<CombatRoll> activeAttackDice = new List<CombatRoll>(); //List of rolled dice to track roll results
    public List<CombatRoll> activeDefenseDice = new List<CombatRoll>();
    [SerializeField]
    private Image healthFill;
    [SerializeField]
    private float currentHealthTest;

    public void SpawnDice(List<int> preRolledValues, bool isAttack, bool isAlly)
    {
        GameObject holder = isAttack ? attackDiceHolder : defenseDiceHolder; //Null check for if we have attackDiceHolder or defenceDiceHolder
        List<CombatRoll> activeList = isAttack ? activeAttackDice : activeDefenseDice; //Null check for the lists
        GameObject dicePrefab = isAlly ? allyDicePrefab : enemyDicePrefab; //Sets prefab to either ally or opp depending on the isAlly bool

        // Clear previous visual dice inside this holder
        foreach (Transform child in holder.transform)
        {
            Destroy(child.gameObject);
        }
        activeList.Clear();

        // Instantiate and initiate rolls
        for (int i = 0; i < preRolledValues.Count; i++) 
        {
            GameObject rolledDice = Instantiate(dicePrefab, holder.transform);
            CombatRoll rollScript = rolledDice.GetComponent<CombatRoll>();
            
            if (rollScript != null)
            {
                activeList.Add(rollScript);
                rollScript.RollTo(preRolledValues[i]); //Force visual outcome to match math
            }
        }
    }

    public void RerollDieVisually(int index, int newResult, bool isAttack)
    {
        List<CombatRoll> activeList = isAttack ? activeAttackDice : activeDefenseDice;
        if (index >= 0 && index < activeList.Count)
        {
            activeList[index].RollTo(newResult);
        }
    }

    public void ClearAllDice()
    {
        activeAttackDice.Clear();
        activeDefenseDice.Clear();
        foreach (Transform child in attackDiceHolder.transform) Destroy(child.gameObject);
        foreach (Transform child in defenseDiceHolder.transform) Destroy(child.gameObject);
    }

    private void Start() 
    {
      
    }
}
