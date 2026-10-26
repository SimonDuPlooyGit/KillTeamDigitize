//Handle all the introductory stuff

using UnityEngine;

public class IntroductionState : BaseState
{
    private GameManager _gameManager;
    public bool MoveToFirstTurn;
    private MenuPanel _menu;

    public IntroductionState(InformationPackage context, GameManager gm,MenuPanel menu) : base(context)
    {
        _gameManager = gm;
        MoveToFirstTurn = false;
        _menu = menu;
    }

    public override void OnEnter()
    {
        Debug.Log("IntroductionState OnEnter");
        _gameManager.setup.InitialiseSetup();
        _menu.CloseMenu(_menu.meleeMenu);
    }

    public override void Update()
    {
        if (_gameManager.setup.setupDone)
        {
            MoveToFirstTurn = true;
        }
    }

    public override void OnExit()
    {
        Debug.Log("IntroductionState OnExit");
    }
}
