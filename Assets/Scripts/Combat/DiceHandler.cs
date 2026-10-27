using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

//-----DiceHandler Class-----
public class DiceHandler : MonoBehaviour
{
    //Determines which dice are allowed to be selected for rerolls
    public enum DiceSelectionMode
    {
        Attack,
        Defense,
        Both,
        Duplicates,
        Set,
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
    GameObject meleeAttackHolder; //The horizontal layout group for the attack dice prefabs
    [SerializeField]
    GameObject meleeDefenseHolder; //The horizontal layout group for the defense dice prefabs
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
    private InformationPackage _context;
    
    [SerializeField]
    private GameObject rerollButton;
    [SerializeField] 
    private GameObject strikeButton;
    [SerializeField] 
    private GameObject parryButton;
    [SerializeField]
    private TextMeshProUGUI xToHit;
    [SerializeField]
    private TextMeshProUGUI xToSave;
    // Guard flag to prevent infinite recursion during duplicate state syncing
    private bool _isSyncingDuplicates = false;
    //Used externally when rerolling a die to a set value
    public int setTargetValue = 6;
    //Max value if die that can be selected for rerolling
    public int maxSelectableDieValue = 6;
    //same but minimum
    public int minSelectableDieValue = 1;
    [SerializeField]
    private GameManager gameManager;

    private void Start()
    {
        currentSelectionMode = DiceSelectionMode.None;
        //currentSelectionMode = DiceSelectionMode.Duplicates; used to test duplicate functionality
        //currentSelectionMode = DiceSelectionMode.Set;
        rerollButton.SetActive(false);    
    }

    private void SpawnDice(List<DiceRoll> physDiceList, bool isAttack, bool isAlly)
    {
        GameObject holder;
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            holder = isAttack ? meleeAttackHolder : meleeDefenseHolder; 
        }
        else
        {
            holder = isAttack ? attackDiceHolder : defenseDiceHolder; //Null check for if we have attackDiceHolder or defenceDiceHolde
        }

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
        //Move dice spawn point to active unit
        if (isAlly)
        {
            diceThrowPoint.position = new Vector3(context.CurrentlySelectedOperative.transform.position.x,
                context.CurrentlySelectedOperative.transform.position.y + 0.4f,
                context.CurrentlySelectedOperative.transform.position.z);
        }
        else if (!isAlly)
        {
            diceThrowPoint.position = new Vector3(context.CurrentlySelectedOperative.transform.position.x,
                context.CurrentlySelectedOperative.transform.position.y + 0.4f,
                context.CurrentlySelectedOperative.transform.position.z);
        }
        //clear tempPhysAttackDice to keep track of attack dice. this is used to populate the dice roll menu in throwDefenseDice coroutine
        activePhysAttackDice.Clear();
        _context = context;
        
        List<DiceRoll> thrownDice= new List<DiceRoll>();

        //Assign prefab based on whether the attacker is an enemy or ally 
        GameObject atkDiceObj = isAlly? allyDicePhysical : enemyDicePhysical;

        //Instantiate/throw the attack dice
        for (int i = 0; i < numDice; i++)
        {
            GameObject physAtkDie = Instantiate(atkDiceObj, diceThrowPoint.position, Random.rotation);
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
            _context.AttackRolls.Add(face);
        }

        //Populate UI panel with roll results
        yield return new WaitForSeconds(2f);
        xToHit.text = $"{context.Weapon.hit} to hit";
        xToSave.text = $"{context.CurrentlySelectedTargetScript.operativeData.save} to save";
        //Stops the sequence here if in melee state
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            yield break;
        }
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            menu.OpenMenu(menu.meleeMenu);
        }
        else
        {
            menu.OpenMenu(menu.diceRollMenu);
        }
        SpawnDice(thrownDice, true, isAlly);
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            FilterDiceList(context, true, context.Weapon.hit);
        }
        //Display whether the dice hit or not by updating the sprite
        foreach (CombatRoll dieSprite in activeAttackDice)
        {
            if (dieSprite.GetValue() >= context.Weapon.hit && gameManager._stateMachine.CurrentState != gameManager.meleeCombatState)
            {
                dieSprite.ToggleHit();
            }
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);
    }
    
    //==================[End throw attack coroutine]=======================
    
    //======================[Throws DEF Dice Physically]=========================================
    public IEnumerator ThrowDefenseDice(int numDice, bool isAlly, InformationPackage context) 
    {
        diceThrowPoint.position = new Vector3(context.CurrentlySelectedTarget.transform.position.x,
            context.CurrentlySelectedTarget.transform.position.y + 0.4f,
            context.CurrentlySelectedTarget.transform.position.z);
        activePhysDefenseDice.Clear();
        _context = context;
        
        //List<DiceRoll> thrownDefDice = new List<DiceRoll>();

        //Assign prefab based on whether the attacker is an enemy or ally 
        GameObject defDiceObj = !isAlly ? allyDicePhysical : enemyDicePhysical;

        //Instantiate/throw the defense dice
        for (int i = 0; i < numDice; i++)
        {
            GameObject physDefDie = Instantiate(defDiceObj, diceThrowPoint.position, Random.rotation);
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
            _context.DefenseRolls.Add(face);
        }

        //Populate UI panel with roll results
        yield return new WaitForSeconds(2f);
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            menu.OpenMenu(menu.meleeMenu);
        }
        else
        {
            menu.OpenMenu(menu.diceRollMenu);
        }
        SpawnDice(activePhysAttackDice, true, isAlly);
        //Display whether the dice hit or not by updating the sprite
        foreach (CombatRoll dieSprite in activeAttackDice)
        {
            if (dieSprite.GetValue() >= context.Weapon.hit && gameManager._stateMachine.CurrentState != gameManager.meleeCombatState)
            {
                dieSprite.ToggleHit();
            }
        }
        SpawnDice(activePhysDefenseDice, false, !isAlly);
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            FilterDiceList(context, true, context.Weapon.hit);
            FilterDiceList(context, false, context.targetWeapon.hit);
        }
        //Display whether the dice blocked or not by updating the sprite
        foreach (CombatRoll dieSprite in activeDefenseDice)
        {
            if (dieSprite.GetValue() >= context.CurrentlySelectedOperativeScript.operativeData.save && gameManager._stateMachine.CurrentState != gameManager.meleeCombatState)
            {
                dieSprite.ToggleBlock();
            }
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);
    }
    //==================[End throw defense coroutine]=======================

    //======================[Standard Dice reroll]==========================================
    private IEnumerator Reroll(bool isAlly)
    {
        //Move dice spawn point to active unit
        if (isAlly)
        {
            diceThrowPoint.position = new Vector3(_context.CurrentlySelectedOperative.transform.position.x,
                _context.CurrentlySelectedOperative.transform.position.y + 0.4f,
                _context.CurrentlySelectedOperative.transform.position.z);
        }
        else if (!isAlly)
        {
            diceThrowPoint.position = new Vector3(_context.CurrentlySelectedOperative.transform.position.x,
                _context.CurrentlySelectedOperative.transform.position.y + 0.4f,
                _context.CurrentlySelectedOperative.transform.position.z);
        }
        //determines which prefabs and dice pool are used based on the current selection state
        DiceSelectionMode activeMode = currentSelectionMode;
        bool isAttackMode = (activeMode == DiceSelectionMode.Attack || activeMode == DiceSelectionMode.Duplicates || activeMode == DiceSelectionMode.Set);
        //Sets selection mode to none to stop continious rerolls
        currentSelectionMode = DiceSelectionMode.None;

        List<CombatRoll> activeUIList = isAttackMode ? activeAttackDice : activeDefenseDice;
        List<DiceRoll> activePhysList = isAttackMode ? activePhysAttackDice : activePhysDefenseDice;
        List<int> activeContextList = isAttackMode ? _context.AttackRolls : _context.DefenseRolls;

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
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            menu.CloseMenu(menu.meleeMenu);
        }
        else
        {
            menu.CloseMenu(menu.diceRollMenu);
        }
        
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

        if (activeMode == DiceSelectionMode.Set)
        {
            foreach (DiceRoll die in newPhysDice)
            {
                die.SetDesiredFace(setTargetValue);
            }
        }

        //Replace updated indices in tracking lists and _context
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
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            menu.OpenMenu(menu.meleeMenu);
        }
        else
        {
            menu.OpenMenu(menu.diceRollMenu);
        }
        bool isUIAlly = isAttackMode ? isAlly : !isAlly;
        SpawnDice(activePhysList, isAttackMode, isUIAlly);
        if (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState)
        {
            FilterDiceList(_context, true, _context.Weapon.hit);
        }
        foreach (CombatRoll dieSprite in activeAttackDice)
        {
            if (dieSprite.GetValue() >= _context.Weapon.hit)
            {
                dieSprite.ToggleHit();
            }
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        menu.CloseMenu(menu.diceRollMenu);
    }
    //==================[End standard reroll coroutine]=======================

    // Helper to get the correct UI list based on whether the target die is an attack die
    private List<CombatRoll> GetActiveUIList(bool isAttack)
    {
        return isAttack ? activeAttackDice : activeDefenseDice;
    }

    //determines if a dice panel can be selected
    public bool CanSelectDie(CombatRoll clickedDie, bool isAttack)
    {
        bool isMeleeState = (gameManager._stateMachine.CurrentState == gameManager.meleeCombatState);
        //Uses current selection state to determine which panels you can select
        if (currentSelectionMode == DiceSelectionMode.None)
        {
            //If not in melee state, block selection completely
            if (!isMeleeState) return false;

            //In Melee state with no reroll mode active, limit selection to exactly 1 die at a time
            List<CombatRoll> meleeActiveList = GetActiveUIList(isAttack);
            int selectedCount = 0;
            for (int i = 0; i < meleeActiveList.Count; i++)
            {
                if (meleeActiveList[i] != null && meleeActiveList[i].IsSelected) selectedCount++;
            }
            return selectedCount < 1;
        }

        //For when restricting the value of die that can be selected
        if (clickedDie != null && !clickedDie.IsSelected)
        {
            int dieValue = clickedDie.GetValue();
            if (dieValue < minSelectableDieValue || dieValue > maxSelectableDieValue)
            {
                return false;
            }
        }

        // Mode restrictions (Attack, Defense, and Duplicates all enforce pool targeting)
        if ((currentSelectionMode == DiceSelectionMode.Attack || currentSelectionMode == DiceSelectionMode.Duplicates || currentSelectionMode == DiceSelectionMode.Set) && !isAttack) return false;
        if (currentSelectionMode == DiceSelectionMode.Defense && isAttack) return false;

        List<CombatRoll> activeList = GetActiveUIList(isAttack);

        // If in Duplicate mode, count how many total matching dice exist in the pool
        if (currentSelectionMode == DiceSelectionMode.Duplicates)
        {
            if (clickedDie == null) return false;
            if (clickedDie.IsSelected) return true;

            int targetValue = clickedDie.GetValue();
            int groupSize = 0;

            // Count every die in the pool that shares this face value
            for (int i = 0; i < activeList.Count; i++)
            {
                if (activeList[i] != null && activeList[i].GetValue() == targetValue)
                {
                    groupSize++;
                }
            }

            // Must have at least 2 matching dice to qualify as duplicates
            if (groupSize < 2)
            {
                Debug.Log("[DiceHandler] Cannot select: No duplicate dice match this value.");
                return false;
            }

            // Count currently selected dice
            int totalSelected = 0;
            for (int i = 0; i < activeList.Count; i++)
            {
                if (activeList[i] != null && activeList[i].IsSelected) totalSelected++;
            }

            // If selecting/deselecting, check against max allowed
            if (!clickedDie.IsSelected && (totalSelected + groupSize) > maxAllowedSelections)
            {
                Debug.Log($"[DiceHandler] Cannot select: Duplicate group size ({groupSize}) exceeds max allowed selections ({maxAllowedSelections}).");
                return false;
            }

            return true;
        }

        // Standard Attack/Defense single selection count
        int currentSelectedCount = 0;
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i] != null && activeList[i].IsSelected) currentSelectedCount++;
        }

        return currentSelectedCount < maxAllowedSelections;
    }

    public void ClearDiceRolls()
    {
        currentSelectionMode = DiceSelectionMode.None;
        _context.AttackRolls.Clear();
        _context.DefenseRolls.Clear();
        foreach(DiceRoll die in activePhysAttackDice)
        {
            Destroy(die.gameObject);
        }

        foreach (DiceRoll die in activePhysDefenseDice)
        {
            Destroy(die.gameObject);
        }

        ClearAllDice();
        xToHit.text = "X to hit";
        xToSave.text = "X to save";
    }

    //Makes the reroll button appear when a panel is selected
    public void UpdateRerollButtonVisibility()
    {
        bool isMeleeState = (gameManager != null && gameManager._stateMachine != null &&
                             gameManager._stateMachine.CurrentState == gameManager.meleeCombatState);

        bool isAttackMode = (currentSelectionMode == DiceSelectionMode.Attack ||
                             currentSelectionMode == DiceSelectionMode.Duplicates ||
                             currentSelectionMode == DiceSelectionMode.Set);

        // Determine selection across relevant pools
        bool hasSelection = false;

        if (currentSelectionMode != DiceSelectionMode.None)
        {
            List<CombatRoll> activeList = isAttackMode ? activeAttackDice : activeDefenseDice;
            foreach (CombatRoll panel in activeList)
            {
                if (panel != null && panel.IsSelected)
                {
                    hasSelection = true;
                    break;
                }
            }
        }
        else
        {
            // If selection mode is None (e.g., standard Melee action picking), check both lists
            foreach (CombatRoll panel in activeAttackDice)
            {
                if (panel != null && panel.IsSelected) { hasSelection = true; break; }
            }
            if (!hasSelection)
            {
                foreach (CombatRoll panel in activeDefenseDice)
                {
                    if (panel != null && panel.IsSelected) { hasSelection = true; break; }
                }
            }
        }

        // 1. Reroll Button: Visible ONLY if a die is selected AND an active reroll mode is set
        if (rerollButton != null)
        {
            rerollButton.SetActive(hasSelection && currentSelectionMode != DiceSelectionMode.None);
        }

        // 2. Strike & Parry Buttons: Visible IF a die is selected AND we are in Melee Combat State
        if (strikeButton != null) strikeButton.SetActive(hasSelection && isMeleeState);
        if (parryButton != null) parryButton.SetActive(hasSelection && isMeleeState);
    }

    //FOR DUPLICATE DICE: when a dice is selected, if in duplicate mode, selects all matching dice values
    public void OnDieSelectedStateChanged(CombatRoll clickedDie, bool isAttack, bool newSelectedState)
    {
        if (currentSelectionMode != DiceSelectionMode.Duplicates) return;
        if (_isSyncingDuplicates) return; // Prevent infinite recursive loops

        _isSyncingDuplicates = true; // Lock

        List<CombatRoll> targetList = GetActiveUIList(isAttack);
        int clickedValue = clickedDie.GetValue();

        //Loop through all panels in the active pool and sync matching values
        for (int i = 0; i < targetList.Count; i++)
        {
            CombatRoll diePanel = targetList[i];
            if (diePanel == null || diePanel == clickedDie) continue;

            //If the values match and the selection state doesn't match, sync it
            if (diePanel.GetValue() == clickedValue && diePanel.IsSelected != newSelectedState)
            {
                if (newSelectedState)
                {
                    diePanel.Select();
                }
                else
                {
                    diePanel.Deselect();
                }
            }
        }

        _isSyncingDuplicates = false; //Unlock
    }

    //used to call the reroll coroutine
    public void RerollButton(bool isAlly)
    {
        StartCoroutine(Reroll(isAlly));
    }
    //Takes in a list of ints and makes the current list of dice panles match it (used for melee combat to only display successes
    public void FilterDiceList(InformationPackage context, bool isAlly, int hit)
    {
        List<CombatRoll> panelList = isAlly ? activeAttackDice : activeDefenseDice;
        foreach(CombatRoll diePanel in panelList)
        {
            if (diePanel.GetValue() < hit)
            {
                Destroy(diePanel.gameObject);
            }
        }
    }
}
