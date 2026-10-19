using UnityEngine;

//-----NPOActionSelect Class (Where NPO executes its turn after being selected)-----
public class NPOActionSelect : BaseState
{
    private NPOBehaviour _currentBehaviour;
    
    public NPOActionSelect(InformationPackage context) : base(context) {}

    public override void OnEnter()
    {
        Debug.Log("NPOActionSelect OnEnter");

        if (Context.CurrentlySelectedOperativeScript != null && Context.CurrentlySelectedOperativeScript is NonPlayerOperative npo)
        {
            _currentBehaviour = npo.GetComponent<NPOBehaviour>();
            if (_currentBehaviour != null)
            {
                Debug.Log("Execute the behaviour");
                _currentBehaviour.ExecuteTurn();
            }
        }
    }

    public override void Update()
    {
        if (_currentBehaviour != null && _currentBehaviour.IsRunning)
        {
            _currentBehaviour.TickTurn();
        }
    }

    public override void OnExit()
    {
        Debug.Log("NPOActionSelect OnExit");
    }
}