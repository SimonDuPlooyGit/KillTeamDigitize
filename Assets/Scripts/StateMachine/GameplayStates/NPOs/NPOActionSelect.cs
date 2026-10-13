using UnityEngine;

public class NPOActionSelect : BaseState
{
    private NPOManager _npoManager;
    private NPOBehaviour _currentBehaviour;
    
    public NPOActionSelect(InformationPackage context, NPOManager npoManager) : base(context)
    {
        _npoManager = npoManager;
    }

    public override void OnEnter()
    {
        Debug.Log("NPOActionSelect OnEnter");

        if (Context.currentlyActivatedNPO != null)
        {
            _currentBehaviour = Context.currentlyActivatedNPO.GetComponent<NPOBehaviour>();
            if (_currentBehaviour != null)
            {
                Debug.Log("Execute the behaviour");
                _currentBehaviour.ExecuteTurn();
            }
        }
    }

    public override void Update()
    {
        if (_currentBehaviour != null && _currentBehaviour.isRunning)
        {
            _currentBehaviour.TickTurn();
        }
    }

    public override void OnExit()
    {
        Debug.Log("NPOActionSelect OnExit");
    }
}
