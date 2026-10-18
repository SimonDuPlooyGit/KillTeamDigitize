//Handle all the introductory stuff

using UnityEngine;

public class IntroductionState : BaseState
{
    private GameManager _gameManager;
    public bool MoveToFirstTurn;
    
    public IntroductionState(InformationPackage context, GameManager gm) : base(context)
    {
        _gameManager = gm;
        MoveToFirstTurn = false;
    }

    public override void OnEnter()
    {
        Debug.Log("IntroductionState OnEnter");
        _gameManager.setup.InitialiseSetup();
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
