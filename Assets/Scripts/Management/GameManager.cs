using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//-----GameManager Class-----
public class GameManager : MonoBehaviour
{
    private InputActions _input;
    public InformationPackage SharedContext; //The script holding information to exchange between all scripts and states
    public StateMachine _stateMachine; //The state machine 
    [SerializeField] private MenuPanel menu;
    [SerializeField] public DiceHandler diceHandler;
    [SerializeField] public OrbitCamera mainCamera;
    [SerializeField] private NPOManager npoManager;
    [SerializeField] public GameSetup setup;
    private PlayerOperative _currentHoveredOperative;
    private NonPlayerOperative _currentHoveredNPO;

    //Player states
    private UnitActivationState unitActivationState;
    private ActionSelectionState actionSelectionState;
    private MovementState movementState;
    private TargetingState targetingState;
    private ShootCombatState shootCombatState;
    private WeaponSelectState weaponSelectState;
    private IntroductionState introductionState;
    public MeleeCombatState meleeCombatState;

    //NPO states
    private NPOSelection npoSelectState;
    private NPOActionSelect npoTurnPerformState;

    //public PauseState pauseState;
    //Tutorial flags
    private bool tutFlagShoot = true;
    private bool tutFlagRepo = true;

    private void Awake()
    {
        SharedContext = new InformationPackage();
        _input = new InputActions();
        _stateMachine = new StateMachine();
        SharedContext.MainCameraScript = mainCamera;
        mainCamera.isFreecam = true;
        SharedContext.AllPlayerUnits = new List<PlayerOperative>(FindObjectsByType<PlayerOperative>(FindObjectsSortMode.None));
        SharedContext.AllNpoUnits = new List<NonPlayerOperative>(FindObjectsByType<NonPlayerOperative>(FindObjectsSortMode.None));

        //Initialize states
        introductionState = new IntroductionState(SharedContext, this, menu);
        unitActivationState = new UnitActivationState(SharedContext, _input, menu, this);
        actionSelectionState = new ActionSelectionState(SharedContext, menu, this);
        movementState = new MovementState(SharedContext, _input, menu, this);
        targetingState = new TargetingState(SharedContext, _input, menu);
        shootCombatState = new ShootCombatState(SharedContext, menu, diceHandler);
        weaponSelectState = new WeaponSelectState(SharedContext, menu);
        npoSelectState = new NPOSelection(SharedContext, npoManager, this);
        npoTurnPerformState = new NPOActionSelect(SharedContext);
        meleeCombatState = new MeleeCombatState(SharedContext, menu, diceHandler);
        //pauseState = new PauseState(SharedContext, _input, _stateMachine);

        //Define transitions

        //Forward transitions
        AddT(introductionState, unitActivationState, new FuncPredicate(() => introductionState.MoveToFirstTurn));
        AddT(unitActivationState, actionSelectionState,
            new FuncPredicate(() =>
                SharedContext.IsPlayerActivationRequested && SharedContext.CurrentlySelectedOperativeScript != null));
        AddT(actionSelectionState, movementState,
            new FuncPredicate(() =>
                SharedContext.IsMovementRequested && SharedContext.CurrentlySelectedOperativeScript != null));
        AddT(movementState, actionSelectionState, new FuncPredicate(() => movementState.MovementFinished));
        AddT(actionSelectionState, weaponSelectState, new FuncPredicate(() => SharedContext.IsShootingRequested || SharedContext.IsFightingRequested));
        AddT(weaponSelectState, targetingState, new FuncPredicate(() => SharedContext.IsWeaponSelected));
        AddT(targetingState, shootCombatState, new FuncPredicate(() => SharedContext.ValidTargetShoot));
        AddT(targetingState, meleeCombatState, new FuncPredicate(() => SharedContext.ValidTargetFight));
        AddT(shootCombatState, actionSelectionState, new FuncPredicate(() =>
        {
            if (SharedContext.IsShootingConfirmed)
            {
                SharedContext.IsShootingRequested = false;
                SharedContext.IsWeaponSelected = false;
                SharedContext.ValidTargetShoot = false;
                SharedContext.IsShootingConfirmed = false;
                return true;
            }

            return false;
        }));
        AddT(actionSelectionState, npoSelectState, new FuncPredicate(() => !SharedContext.PlayerTurn));
        AddT(npoSelectState, npoTurnPerformState,
            new FuncPredicate(() => SharedContext.CurrentlySelectedOperativeScript != null && SharedContext.CurrentlySelectedOperativeScript is NonPlayerOperative)); //If NPO is found to activate
        AddT(npoTurnPerformState, unitActivationState, new FuncPredicate(() => SharedContext.PlayerTurn));
        AddT(npoTurnPerformState, meleeCombatState, new FuncPredicate(() => SharedContext.IsFightingRequested && SharedContext.CurrentlySelectedOperativeScript is NonPlayerOperative));
        AddT(meleeCombatState, npoTurnPerformState, new FuncPredicate(() => SharedContext.FightSequenceOver && SharedContext.CurrentlySelectedOperativeScript is NonPlayerOperative));
        AddT(meleeCombatState, actionSelectionState, new FuncPredicate(() => SharedContext.FightSequenceOver && SharedContext.CurrentlySelectedOperativeScript is PlayerOperative));
        
        //Player ended their turn but it's still the player's turn (NPOs have nobody left, or new turning point)
        AddT(actionSelectionState, unitActivationState,
            new FuncPredicate(() => SharedContext.PlayerTurn && SharedContext.CurrentlySelectedOperativeScript == null));
        
        //Players have nobody left to act but NPOs do (e.g. after an NPO finishes and CheckTurningPointEnd flips PlayerTurn)
        AddT(unitActivationState, npoSelectState,
            new FuncPredicate(() => !SharedContext.PlayerTurn));
        
        //AddT for pressing a pass turn button
        _stateMachine.SetState(unitActivationState);
        
        //Backwards transitions
        GoBackAState(actionSelectionState, unitActivationState,
            () =>
            {
                var op = SharedContext.CurrentlySelectedOperativeScript;
                if (op == null) return false;
                if (op.currentApl == op.operativeData.apl) return true;
                Debug.Log("Can't swap from this unit because it made an action already");
                return false;
            },
            () =>
            {
                SharedContext.CurrentlySelectedOperativeScript.Deselect();
                SharedContext.FullReset();
            }
        );
        
        /*GoBackAState(actionSelectionState, unitActivationState,
            () =>
            {
                if (SharedContext.CurrentlySelectedOperativeScript.currentApl ==
                    SharedContext.CurrentlySelectedOperativeScript.operativeData.apl)
                {
                    return true;
                }
                else
                {
                    Debug.Log("Can't swap from this unit because it made an action already");
                    return false;
                }
            },
            () =>
            {
            SharedContext.CurrentlySelectedOperativeScript.Deselect();
            SharedContext.FullReset();
            }
        );*/

        GoBackAState(movementState, actionSelectionState,()=>true,() =>
        {
            SharedContext.IsMovementRequested = false;
            SharedContext.SelectedMovement = PlayerMovementType.None;
            var playerOp = SharedContext.CurrentlySelectedOperativeScript as PlayerOperative;
            if (playerOp != null)
            {
                playerOp.ResetMoveMarker();
            }
        });
        GoBackAState(weaponSelectState, actionSelectionState, ()=>true,() =>
        {
            SharedContext.IsShootingRequested = false;
            SharedContext.IsFightingRequested = false;
        });
        GoBackAState(targetingState, weaponSelectState,()=>true,() =>
        {
            SharedContext.IsWeaponSelected = false;
            SharedContext.Weapon = null;
        });
        GoBackAState(shootCombatState, targetingState,()=>true,() =>
        {
            SharedContext.CurrentlySelectedTarget = null;
        });

        menu.tpCounter.GetComponentInChildren<TextMeshProUGUI>().text = "1";

        //Global any transition
        //AnyT(pauseState, new FuncPredicate(() => _input.Controls.Pause.WasPressedThisFrame() && _stateMachine.CurrentState is not PauseState && Time.frameCount > _stateMachine.LastTransitionFrame));
    }

    //Helper function to do backwards transitions with right click as the predicate
    ////The action onBackTransition is logic you want to happen on the transition such as closing menus
    public void GoBackAState(IState currentState, IState previousState, Func<bool> canGoBack, Action onBackTransition) 
    {
        AddT(currentState, previousState, new FuncPredicate(() =>
        {
            if (_input.Controls.Deselect.WasPressedThisFrame())
            {
                if (!canGoBack())
                {
                    return false;
                }

                if(SceneManager.GetActiveScene().name == "Tut1")
                {
                    return false;
                }

                if (SceneManager.GetActiveScene().name == "Tut2" && !menu.tutFlagBack)
                {
                    return false;
                }

                if (SceneManager.GetActiveScene().name == "Tut3")
                {
                    return false;
                }

                onBackTransition?.Invoke();
                return true;
            }
            return false;
        }));
    }
    
    //Helper function to add a transition
    void AddT(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
    //Helper function to add a transition from any state
    void AnyT(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        _stateMachine.Update(); //Keep the state machine updating
        if (_stateMachine.CurrentState == weaponSelectState)
        {
            mainCamera.switchOffForMenu = true;
        }
        else
        {
            mainCamera.switchOffForMenu = false;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (hit.collider.gameObject.CompareTag("AllyUnit"))
            {
                PlayerOperative hitOperative = hit.collider.GetComponent<PlayerOperative>();

                // Clearing NPO if switching directly to an ally
                if (_currentHoveredNPO != null && !IsSelectedUnit(_currentHoveredNPO))
                {
                    _currentHoveredNPO.ToggleUIOff();
                    _currentHoveredNPO.ToggleStatusOff();
                    _currentHoveredNPO = null;
                }

                // Switching from Ally A to Ally B
                if (_currentHoveredOperative != null && _currentHoveredOperative != hitOperative)
                {
                    if (!IsSelectedUnit(_currentHoveredOperative))
                    {
                        _currentHoveredOperative.ToggleUIOff();
                        _currentHoveredOperative.ToggleStatusOff();
                    }
                }

                if (_currentHoveredOperative != hitOperative)
                {
                    _currentHoveredOperative = hitOperative;
                    _currentHoveredOperative.ToggleUIOn();
                    _currentHoveredOperative.ToggleStatusIcons();
                }

                return;
            }
            else if (hit.collider.gameObject.CompareTag("EnemyUnit"))
            {
                NonPlayerOperative hitOperative = hit.collider.GetComponent<NonPlayerOperative>();

                if (_currentHoveredOperative != null && !IsSelectedUnit(_currentHoveredOperative))
                {
                    _currentHoveredOperative.ToggleUIOff();
                    _currentHoveredOperative = null;
                }

                if (_currentHoveredNPO != null && _currentHoveredNPO != hitOperative)
                {
                    if (!IsSelectedUnit(_currentHoveredNPO))
                    {
                        _currentHoveredNPO.ToggleUIOff();
                    }
                }

                if (_currentHoveredNPO != hitOperative)
                {
                    _currentHoveredNPO = hitOperative;
                    _currentHoveredNPO.ToggleUIOn();
                    _currentHoveredNPO.ToggleStatusIcons();
                }

                return;
            }
        }
        ClearHoverState();
    }

    public void OnRepositionActionButtonPressed()
    {
        if (!SharedContext.CurrentlySelectedOperativeScript.EnemyInControlRange)
        {
            SoundManager.PlaySound(2, 1, true);
            SharedContext.SelectedMovement = PlayerMovementType.Reposition;
            SharedContext.IsMovementRequested = true;

            if(SceneManager.GetActiveScene().name == "Tut2" && tutFlagRepo)
            {
                TutEvents.AdvanceTutorial();
                tutFlagRepo = false;
            }
        }
        else
        {
            Debug.Log("Cannot reposition out of enemy control range");
            SoundManager.PlaySound(7);
            menu.PopUp(menu.inRange);
        }

        if(SceneManager.GetActiveScene().name == "Tut1" && !SharedContext.CurrentlySelectedOperativeScript.GetComponent<PlayerOperative>().hasRepositionedThisActivation)
        {
            TutEvents.AdvanceTutorial();
        }
    }

    public void OnChargeActionButtonPressed()
    {
        if (!SharedContext.CurrentlySelectedOperativeScript.EnemyInControlRange)
        {
            SoundManager.PlaySound(2, 1, true);
            SharedContext.SelectedMovement = PlayerMovementType.Charge;
            SharedContext.IsMovementRequested = true;
        }
        else
        {
            Debug.Log("Cannot charge out of enemy control range");
            SoundManager.PlaySound(7);
            menu.PopUp(menu.inRange);
        }
    }

    public void OnDashActionButtonPressed()
    {
        if (!SharedContext.CurrentlySelectedOperativeScript.EnemyInControlRange)
        {
            SoundManager.PlaySound(2, 1, true);
            SharedContext.SelectedMovement = PlayerMovementType.Dash;
            SharedContext.IsMovementRequested = true;
        }
        else
        {
            Debug.Log("Cannot dash out of enemy control range");
            SoundManager.PlaySound(7);
            menu.PopUp(menu.inRange);
        }

        if (SceneManager.GetActiveScene().name == "Tut1")
        {
            TutEvents.AdvanceTutorial();
        }
    }

    public void OnFallbackButtonPressed()
    {
        if (SharedContext.CurrentlySelectedOperativeScript.EnemyInControlRange)
        {
            SoundManager.PlaySound(2, 1, true);
            SharedContext.SelectedMovement = PlayerMovementType.Fallback;
            SharedContext.IsMovementRequested = true;
        }
        else
        {
            Debug.Log("Can only fallback out of enemy control range");
            SoundManager.PlaySound(7);
            menu.PopUp(menu.invCharge);
        }
    }
    
    public void OnConfirmMovementButtonPressed()
    {
        if (movementState != null)
        {
            movementState.ConfirmAndExecuteMovement();
        }
    }

    public void OnShootingButtonPressed()
    {
        if (!SharedContext.CurrentlySelectedOperativeScript.EnemyInControlRange)
        {
            SoundManager.PlaySound(3, 1, true);
            SharedContext.IsShootingRequested = true;

            if(SceneManager.GetActiveScene().name == "Tut2" && tutFlagShoot)
            {
                TutEvents.AdvanceTutorial();
                tutFlagShoot = false;
            }
        }
        else
        {
            Debug.Log("Cannot shoot when engaged by enemy in control range");
            SoundManager.PlaySound(7);
            menu.PopUp(menu.inRange);//dont know if in control range is the best way to phrase this not being available
        }
    }

    public void OnFightButtonPressed()
    {
        SoundManager.PlaySound(3, 1, true);
        SharedContext.IsFightingRequested = true;
    }
    
    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    public void Quit()
    {
        Application.Quit();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnEndTurnPressed()
    {
        if (SharedContext.CurrentlySelectedOperativeScript != null)
        {
            SoundManager.PlaySound(2, 1, true);
            SharedContext.CurrentlySelectedOperativeScript.acted = true;

            if (SharedContext.CurrentlySelectedOperativeScript.currentOrder == OrderState.Engage)
            {
                SharedContext.CurrentlySelectedOperativeScript.engageSymbol.SetActive(false);
                SharedContext.CurrentlySelectedOperativeScript.engageSymbolSpent.SetActive(true);
            } else if (SharedContext.CurrentlySelectedOperativeScript.currentOrder == OrderState.Conceal)
            {
                SharedContext.CurrentlySelectedOperativeScript.concealSymbol.SetActive(false);
                SharedContext.CurrentlySelectedOperativeScript.concealSymbolSpent.SetActive(true);
            }
            
            SharedContext.CurrentlySelectedOperativeScript.selected = false;
            SharedContext.CurrentlySelectedOperativeScript.TurnFinished();
        }
        
        SharedContext.FullReset();
        SharedContext.PlayerTurn = false;
        CheckTurningPointEnd();
    }

    public void CheckTurningPointEnd()
    {
        bool playerUnitsLeft = SharedContext.AllPlayerUnits.Exists(u => !u.dead && !u.acted);
        bool nposLeft = SharedContext.AllNpoUnits.Exists(n => !n.dead && !n.acted);

        if (!playerUnitsLeft && !nposLeft)
        {
            SharedContext.CurrentTurningPoint++;
            menu.tpCounter.GetComponentInChildren<TextMeshProUGUI>().text = SharedContext.CurrentTurningPoint.ToString();
            if (SharedContext.CurrentTurningPoint > InformationPackage.LastTurningPoint)
            {
                Debug.Log("Last turning point ended");
                return;
            }
            
            Debug.Log($"Starting turning point {SharedContext.CurrentTurningPoint}");

            foreach (var unit in SharedContext.AllPlayerUnits)
            {
                unit.ResetForNewTurningPoint();
            }

            foreach (var npo in SharedContext.AllNpoUnits)
            {
                npo.ResetForNewTurningPoint();
            }
            
            SharedContext.PlayerTurn = true;
            CheckGameEnd();
            return;
        }

        if (SharedContext.PlayerTurn && !playerUnitsLeft)
        {
            SharedContext.PlayerTurn = false;
        } else if (!SharedContext.PlayerTurn && !nposLeft)
        {
            SharedContext.PlayerTurn = true;
        }
    }

    public void CheckGameEnd()
    {
        Debug.Log("Check Game End called");
        int alivePlayerUnits = 0;
        int aliveNpoUnits = 0;

        foreach (var unit in SharedContext.AllPlayerUnits)
        {
            if (!unit.dead)
            {
                alivePlayerUnits++;
            }
        }

        foreach (var unit in SharedContext.AllNpoUnits)
        {
            if (!unit.dead)
            {
                aliveNpoUnits++;
            }
        }

        Debug.Log($"There are {alivePlayerUnits} player operatives alive and {aliveNpoUnits} npo alive");
        
        if (alivePlayerUnits == 0 || aliveNpoUnits == 0)
        {
            Debug.Log("Game is over");
        }
    }

    private void ClearHoverState()
    {
        if (_currentHoveredOperative != null)
        {
            //toggle off if not selected
            if (!IsSelectedUnit(_currentHoveredOperative))
            {
                _currentHoveredOperative.ToggleUIOff();
                _currentHoveredOperative.ToggleStatusOff();
            }
            _currentHoveredOperative = null;
        }

        if (_currentHoveredNPO != null)
        {
            //same for NPO
            if (!IsSelectedUnit(_currentHoveredNPO))
            {
                _currentHoveredNPO.ToggleUIOff();
                _currentHoveredNPO.ToggleStatusOff();
            }
            _currentHoveredNPO = null;
        }
    }

    private bool IsSelectedUnit(MonoBehaviour unitScript)
    {
        if (SharedContext == null || SharedContext.CurrentlySelectedOperativeScript == null)
            return false;

        return SharedContext.CurrentlySelectedOperativeScript == unitScript;
    }
}