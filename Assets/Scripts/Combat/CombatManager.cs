using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic; 

public class CombatManager : MonoBehaviour
{
    //On the combat manager GameObject
    
    [SerializeField]
    GameObject dicePrefab; //Holds a dice prefab
    [SerializeField]
    GameObject diceHolder; //The horizontal layout group for the prefabs
    public List<CombatRoll> activeDice = new List<CombatRoll>(); //List of rolled dice to track roll results

    public int targetDefense; //The targeted unit's defense value
    public List<CombatRoll> GetActiveDice() //use combatManager.GetActiveDice() to HOPEFULLY access the list in other scripts. Contact me(Joss) in case of emergency (NOT USING THIS)
    {
        return activeDice;
    }

    public void SpawnDice(int numRolled) //Instantiate the dice prefabs into the holder and roll them and test each dice against the defense value
    {
        activeDice.Clear();
        foreach(Transform child in diceHolder.transform) //iterates through children to clear them
        {
            GameObject childObj = child.gameObject;
            Destroy(childObj);
        }
        for(int i=0; i<numRolled; i++) //rolls a certain number of dice
        {
            GameObject rolledDice = Instantiate(dicePrefab, diceHolder.transform);
            CombatRoll rollScript = rolledDice.GetComponent<CombatRoll>(); //updates the list with combat roll scripts (the scripts on the dice)
            if(rollScript != null )
            {
                activeDice.Add(rollScript);
            }
        }
        HitTester(targetDefense);
    }

    public void HitTester(int defense) //Testing function to see if a roll is above a specified defence value
    {
        foreach(CombatRoll die in activeDice)
        {
            int roll = die.rollResult;
            if(roll >= defense)
            {
                Debug.Log("Hit");
            }
        }
    }
}
