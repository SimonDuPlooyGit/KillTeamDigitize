using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEditor.Rendering.LookDev; //need this to access the Image component

public class DiceHandler : MonoBehaviour
{
    public enum DiceSelectionMode
    {
        Attack,
        Defense,
        Both,
        None
    }
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
    //List used to pass attack roll values in throwDefenseDice to populate the menu
    public List<DiceRoll> tempPhysAttackDice = new List<DiceRoll>();
    //max amount of dice that can be selected to be rerolled, number will have to be set externally
    public int maxAllowedSelections = 2;
    public DiceSelectionMode currentSelectionMode = DiceSelectionMode.Attack; // Default to only select attack dice

    //Access to information package
    private InformationPackage context;

    public void SpawnDice(List<DiceRoll> physDiceList, bool isAttack, bool isAlly)
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
        for (int i = 0; i < physDiceList.Count; i++) 
        {
            GameObject rolledDiceUI = Instantiate(dicePrefab, holder.transform);
            CombatRoll rollScript = rolledDiceUI.GetComponent<CombatRoll>();
            
            if (rollScript != null)
            {
                activeList.Add(rollScript);
                rollScript.Initialize(physDiceList[i],this,isAttack);
            }
        }
    }

    public void ClearAllDice()
    {
        activeAttackDice.Clear();
        activeDefenseDice.Clear();
        foreach (Transform child in attackDiceHolder.transform) Destroy(child.gameObject);
        foreach (Transform child in defenseDiceHolder.transform) Destroy(child.gameObject);
    }

    //======================[Throws ATK Dice Physically]==========================================
    public IEnumerator ThrowAttackDice(int numDice, bool isAlly, InformationPackage context)
    {
        //clear tempPhysAttackDice to keep track of attack dice. this is used to populate the dice roll menu in throwDefenseDice coroutine
        tempPhysAttackDice.Clear();
        this.context = context;
        
        List<DiceRoll> thrownDice= new List<DiceRoll>();

        //Assign prefab based on whether the attacker is an enemy or ally 
        GameObject AtkDiceObj = isAlly? allyDicePhysical : enemyDicePhysical;

        //Instantiate/throw the attack dice
        for (int i = 0; i < numDice; i++)
        {
            GameObject physAtkDie = Instantiate(AtkDiceObj, diceThrowPoint.position, Random.rotation);
            DiceRoll dieScript = physAtkDie.GetComponent<DiceRoll>();
            thrownDice.Add(dieScript);
            tempPhysAttackDice.Add(dieScript);
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
            context.attackRolls.Add(face);
        }

        //Populate UI panel with roll results
        yield return new WaitForSeconds(2f);
        menu.OpenMenu(menu.diceRollMenu);
        SpawnDice(thrownDice, true, isAlly);
        //yield return new WaitForSeconds(3);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);

        //Clean up dice objects
        /*foreach (DiceRoll die in thrownDice)
        {
            Destroy(die.gameObject);
        }*/
    }
    //==================[End coroutine]=======================
    
    //======================[Throws DEF Dice Physically]==========================================
    public IEnumerator ThrowDefenseDice(int numDice, bool isAlly, InformationPackage context) 
    {
        this.context = context;
        
        List<DiceRoll> thrownDefDice = new List<DiceRoll>();

        //Assign prefab based on whether the attacker is an enemy or ally 
        GameObject DefDiceObj = !isAlly ? allyDicePhysical : enemyDicePhysical;

        //Instantiate/throw the defense dice
        for (int i = 0; i < numDice; i++)
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

            foreach(DiceRoll die in thrownDefDice)
            {
                if(!die.IsStopped())
                {
                    allStopped=false;
                    break;
                }
            }
            yield return null;
        }

        //Process Defense dice values
        foreach (DiceRoll die in thrownDefDice)
        {
            int face = die.GetUpwardFace();
            context.defenseRolls.Add(face);
        }

        //Populate UI panel with roll results
        yield return new WaitForSeconds(2f);
        menu.OpenMenu(menu.diceRollMenu);
        SpawnDice(tempPhysAttackDice, true, isAlly);
        SpawnDice(thrownDefDice, false, !isAlly);
        //yield return new WaitForSeconds(3);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);
        
        /*foreach (DiceRoll die in thrownDefDice)
        {
            Destroy(die.gameObject);
        }*/
    }
    //==================[End coroutine]=======================

    /*public IEnumerator Reroll(int position, bool isAttack)
    {
        if (isAttack)
        {
            //context.attackRolls[position].reroll;
        }
        else
        {
            //context.defenseRolls[position].reroll;
        }
    }*/

    //Clears both dice roll result lists. 

    //determines if a dice panel can be selected
    public bool CanSelectDie(bool isAttack)
    {
        //Uses cuurent selection state to determine which panels you can select
        if (currentSelectionMode == DiceSelectionMode.None) return false;
        if (currentSelectionMode == DiceSelectionMode.Attack && !isAttack) return false;
        if (currentSelectionMode == DiceSelectionMode.Defense && isAttack) return false;

        // Count currently selected dice across active lists
        int totalSelected = 0;

        for (int i = activeAttackDice.Count - 1; i >= 0; i--)
        {
            if (activeAttackDice[i].IsSelected) totalSelected++;
        }

        for (int i = activeDefenseDice.Count - 1; i >= 0; i--)
        {
            if (activeDefenseDice[i].IsSelected) totalSelected++;
        }

        return totalSelected < maxAllowedSelections;
    }
    public void ClearDiceRolls()
    {
        currentSelectionMode = DiceSelectionMode.Attack;
        context.attackRolls.Clear();
        context.defenseRolls.Clear();
    }
    
}
