using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEditor.Rendering.LookDev; //need this to access the Image component

public class DiceHandler : MonoBehaviour
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
    [SerializeField]
    Transform diceThrowPoint; //Where dice spawn from
    public List<CombatRoll> activeAttackDice = new List<CombatRoll>(); //List of rolled attack dice to track roll results
    public List<CombatRoll> activeDefenseDice = new List<CombatRoll>(); //List of rolled defence dice
    [SerializeField]
    private Image healthFill; 
    [SerializeField]
    private float currentHealthTest;
    [SerializeField]
    private MenuPanel menu;
    //Lists of Attack and Defense dice results. They are populated in the ThrowDice coroutine
    public List<int> AtkRollResults = new List<int>();
    public List<int> DefRollResults = new List<int>();

    //Access to information package
    private InformationPackage context;

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

    //======================[Throws Dice Physically]==========================================
    public IEnumerator ThrowDice(int numDice, int numDef, bool isAlly, InformationPackage context) 
    {
        this.context = context;
        ClearDiceRolls();
        
        List<DiceRoll> thrownDice= new List<DiceRoll>();
        List<DiceRoll> thrownDefDice = new List<DiceRoll>();

        //Assign prefab based on whether the attacker is an enemy or ally 
        GameObject AtkDiceObj = isAlly? allyDicePhysical : enemyDicePhysical;
        GameObject DefDiceObj = !isAlly ? allyDicePhysical : enemyDicePhysical;

        //Instantiate/throw the attack dice
        for (int i = 0; i < numDice; i++)
        {
            GameObject physAtkDie = Instantiate(AtkDiceObj, diceThrowPoint.position, Random.rotation);
            DiceRoll dieScript = physAtkDie.GetComponent<DiceRoll>();
            thrownDice.Add(dieScript);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(2f);

        //Instantiate/throw the defense dice
        for (int i = 0; i < numDef; i++)
        {
            GameObject physDefDie = Instantiate(DefDiceObj, diceThrowPoint.position, Random.rotation);
            DiceRoll dieScript = physDefDie.GetComponent<DiceRoll>();
            thrownDefDice.Add(dieScript);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        //Wait until all dice stop moving before processing their results
        bool allStopped = false;
        while(!allStopped)
        {
            allStopped = true;

            foreach(DiceRoll die in thrownDice)
            {
                if(!die.IsStopped())
                {
                    allStopped=false;
                    break;
                }
            }
            yield return null;
        }

        //Process Attack dice values
        foreach (DiceRoll die in thrownDice)
        {
            int face = die.GetUpwardFace();
            AtkRollResults.Add(face);
            context.attackRolls.Add(face);
        }

        //Process Defense dice values
        foreach (DiceRoll die in thrownDefDice)
        {
            int face = die.GetUpwardFace();
            DefRollResults.Add(face);
            context.defenseRolls.Add(face);
        }

        //Populate UI panel with roll results
        yield return new WaitForSeconds(2f);
        menu.OpenMenu(menu.diceRollMenu);
        SpawnDice(AtkRollResults, true, isAlly);
        SpawnDice(DefRollResults, false, !isAlly);
        yield return new WaitForSeconds(3);
        menu.CloseMenu(menu.diceRollMenu);

        //Clean up dice objects
        foreach (DiceRoll die in thrownDice)
        {
            Destroy(die.gameObject);
        }
        foreach (DiceRoll die in thrownDefDice)
        {
            Destroy(die.gameObject);
        }
    }
    //==================[End coroutine]=======================

    //Clears both dice roll result lists. 
    public void ClearDiceRolls()
    {
        AtkRollResults.Clear();
        DefRollResults.Clear();
        context.attackRolls.Clear();
        context.defenseRolls.Clear();
    }
    
}
