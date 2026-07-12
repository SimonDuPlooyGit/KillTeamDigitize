using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic; 

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    GameObject dicePrefab;
    [SerializeField]
    GameObject diceHolder;
    private List<CombatRoll> activeDice = new List<CombatRoll>(); //List of rolled dice to track roll results
    public List<CombatRoll> GetActiveDice() //use combatManager.GetActiveDice() to HOPEFULLY access the list in other scripts. Contact me(Joss) in case of emergency
    {
        return activeDice;
    }
    void Start()
    {
        
    }


    public void SpawnDice(int numRolled)
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
            CombatRoll rollScript = rolledDice.GetComponent<CombatRoll>(); //updates the list
            if(rollScript != null )
            {
                activeDice.Add(rollScript);
            }
        }

        HitTester();
       
    }

    public void HitTester() //test that says hit when a die is above or equal to 3
    {
        foreach(CombatRoll die in activeDice)
        {
            int roll = die.rollResult;
            if(roll >= 3 )
            {
                Debug.Log("Hit");
            }
        }
    }
}
