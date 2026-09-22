using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;


public class DiceHandler : MonoBehaviour
{
    //Determines which dice are allowed to be selected for rerolls
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
    public List<CombatRoll> activeAttackDice = new List<CombatRoll>(); //List of rolled UI attack dice to track roll results for menu
    public List<CombatRoll> activeDefenseDice = new List<CombatRoll>(); //List of rolled UI defence dice for menu
    [SerializeField]
    private Image healthFill; 
    [SerializeField]
    private float currentHealthTest;
    [SerializeField]
    private MenuPanel menu;
    
    //Lists used to keep global track of attack and defense dice for rerolling purposes
    public List<DiceRoll> activePhysAttackDice = new List<DiceRoll>();
    public List<DiceRoll> activePhysDefenseDice = new List<DiceRoll>();
    
    //max amount of dice that can be selected to be rerolled, number will have to be set externally
    public int maxAllowedSelections = 2;
    public DiceSelectionMode currentSelectionMode = DiceSelectionMode.Attack; // Default to only select attack dice
    
    //Access to information package
    private InformationPackage context;
    
    [SerializeField]
    private GameObject rerollButton;

    private void Start()
    {
        currentSelectionMode = DiceSelectionMode.None;
        rerollButton.SetActive(false);    
    }

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
        activePhysAttackDice.Clear();
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
            activePhysAttackDice.Add(dieScript);
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
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);

        //Clean up dice objects
        /*foreach (DiceRoll die in thrownDice)
        {
            Destroy(die.gameObject);
        }*/
    }
    
    //==================[End throw attack coroutine]=======================
    
    //======================[Throws DEF Dice Physically]==========================================
    public IEnumerator ThrowDefenseDice(int numDice, bool isAlly, InformationPackage context) 
    {
        activePhysDefenseDice.Clear();
        this.context = context;
        
        //List<DiceRoll> thrownDefDice = new List<DiceRoll>();

        //Assign prefab based on whether the attacker is an enemy or ally 
        GameObject DefDiceObj = !isAlly ? allyDicePhysical : enemyDicePhysical;

        //Instantiate/throw the defense dice
        for (int i = 0; i < numDice; i++)
        {
            GameObject physDefDie = Instantiate(DefDiceObj, diceThrowPoint.position, Random.rotation);
            DiceRoll dieScript = physDefDie.GetComponent<DiceRoll>();
            activePhysDefenseDice.Add(dieScript);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        //Wait until all dice stop moving before processing their results
        bool allStopped = false;
        while(!allStopped)
        {
            allStopped = true;

            foreach(DiceRoll die in activePhysDefenseDice)
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
        foreach (DiceRoll die in activePhysDefenseDice)
        {
            int face = die.GetUpwardFace();
            context.defenseRolls.Add(face);
        }

        //Populate UI panel with roll results
        yield return new WaitForSeconds(2f);
        menu.OpenMenu(menu.diceRollMenu);
        SpawnDice(activePhysAttackDice, true, isAlly);
        SpawnDice(activePhysDefenseDice, false, !isAlly);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);
    }
    //==================[End throw defense coroutine]=======================

    //======================[Standard Dice reroll]==========================================
    public IEnumerator Reroll(bool isAlly)
    {
        //determines which prefabs and dice pool are used based on the current selection state
        bool isAttackMode = (currentSelectionMode == DiceSelectionMode.Attack);
        //Sets selection mode to none to stop continious rerolls
        currentSelectionMode = DiceSelectionMode.None;

        List<CombatRoll> activeUIList = isAttackMode ? activeAttackDice : activeDefenseDice;
        List<DiceRoll> activePhysList = isAttackMode ? activePhysAttackDice : activePhysDefenseDice;
        List<int> activeContextList = isAttackMode ? context.attackRolls : context.defenseRolls;

        GameObject diceObjPrefab = isAttackMode? (isAlly ? allyDicePhysical : enemyDicePhysical): (!isAlly ? allyDicePhysical : enemyDicePhysical);

        //check which panels are selected by creating new list and comparing it to current list of panels
        List<int> selectedDice = new List<int>();
        for (int i = 0; i < activeUIList.Count; i++)
        {
            if (activeUIList[i] != null && activeUIList[i].IsSelected)
            {
                selectedDice.Add(i);
            }
        }
        //case for if no dice are selected
        if (selectedDice.Count == 0)
        {
            Debug.Log("[DiceHandler] No dice selected for reroll.");
            yield break;
        }

        //Destroy corresponding physical dice
        foreach (int index in selectedDice)
        {
            if (activePhysList[index] != null)
            {
                Destroy(activePhysList[index].gameObject);
            }
        }
        //Close menu
        menu.CloseMenu(menu.diceRollMenu);
        yield return new WaitForSeconds(0.5f);

        //Instantiate and throw new dice
        List<DiceRoll> newPhysDice = new List<DiceRoll>();
        for (int i = 0; i < selectedDice.Count; i++)
        {
            GameObject newPhysDie = Instantiate(diceObjPrefab, diceThrowPoint.position, Random.rotation);
            DiceRoll dieScript = newPhysDie.GetComponent<DiceRoll>();
            newPhysDice.Add(dieScript);
            yield return new WaitForSeconds(0.1f);
        }

        //Wait for all new dice to stop moving
        yield return new WaitForSeconds(0.5f);
        bool allStopped = false;
        while (!allStopped)
        {
            allStopped = true;
            foreach (DiceRoll die in newPhysDice)
            {
                if (!die.IsStopped())
                {
                    allStopped = false;
                    break;
                }
            }
            yield return null;
        }

        //Replace updated indices in tracking lists and context
        for (int i = 0; i < selectedDice.Count; i++)
        {
            int slotIndex = selectedDice[i];
            DiceRoll newDie = newPhysDice[i];
            int newFace = newDie.GetUpwardFace();

            activePhysList[slotIndex] = newDie;
            activeContextList[slotIndex] = newFace;
        }

        yield return new WaitForSeconds(1.5f);
        //Update the menu
        menu.OpenMenu(menu.diceRollMenu);
        bool isUIAlly = isAttackMode ? isAlly : !isAlly;
        SpawnDice(activePhysList, isAttackMode, isUIAlly);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);
    }
    //==================[End standard reroll coroutine]=======================

    //determines if a dice panel can be selected
    public bool CanSelectDie(bool isAttack)
    {
        //Uses current selection state to determine which panels you can select
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
        currentSelectionMode = DiceSelectionMode.None;
        context.attackRolls.Clear();
        context.defenseRolls.Clear();
        foreach(DiceRoll die in activePhysAttackDice)
        {
            Destroy(die.gameObject);
        }

        foreach (DiceRoll die in activePhysDefenseDice)
        {
            Destroy(die.gameObject);
        }
    }

    //Makes the reroll button appear when a panel is selected
    public void UpdateRerollButtonVisibility()
    {
        if (rerollButton == null) return;

        bool isAttackMode = (currentSelectionMode == DiceSelectionMode.Attack);
        List<CombatRoll> activeList = isAttackMode ? activeAttackDice : activeDefenseDice;

        bool hasSelection = false;
        foreach (CombatRoll panel in activeList)
        {
            if (panel != null && panel.IsSelected)
            {
                hasSelection = true;
                break;
            }
        }
        // Enable button only if at least 1 panel is selected
        rerollButton.SetActive(hasSelection);
    }
    //used to call the reroll coroutine
    public void RerollButton(bool isAlly)
    {
        StartCoroutine(Reroll(isAlly));
    }

}
