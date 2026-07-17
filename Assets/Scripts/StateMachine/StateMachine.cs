using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    //The StateMachine class manages different states and transitions
    
    private StateNode current; //The current state, kept as a StateNode class
    private StateNode previous; //The node that was previous to current
    private Dictionary<Type, StateNode> nodes = new(); //A dictionary of states, key is the State name, value is the StateNode class
    HashSet<ITransition> anyTransitions = new(); //A hashset (to only have unique values and faster lookup) to track global state transitions that can happen whenever

    public IState CurrentState => current?.State; //Make a separate public reference for the current state
    public int LastTransitionFrame { get; private set; } = -1; //This was necessary because otherwise Same-frame input reflection transitioned us back to pause immediately
    
    public void Update() //Checks if transitions need to occur every frame
    {
        var transition = GetTransition(); //If there is a transition change the state
        if (transition != null)
        {
            ChangeState(transition.To);
        }
        
        current.State?.Update();
    }

    public void SetState(IState state) //Sets current to the state that was changed to and call the state's OnEnter function
    {
        current = nodes[state.GetType()];
        current.State?.OnEnter();
    }

    public void ChangeState(IState state) //Change the states, call exit of previous and enter of the new one
    {
        if (state == current.State) return;
        
        var previousState = current.State;
        var nextState = nodes[state.GetType()].State;
        
        previousState?.OnExit();
        nextState?.OnEnter();

        previous = current;
        current = nodes[state.GetType()];

        LastTransitionFrame = Time.frameCount;
    }

    ITransition GetTransition() //Returns a transition
    {
        foreach (var transition in anyTransitions) //Check any transitions first
        {
            if (transition.Condition.Evaluate()) //Evaluate if the transition's predicate is true
            {
                return transition;
            }
        }
        
        foreach (var transition in current.Transitions)
        {
            if (transition.Condition.Evaluate()) //Evaluate if the transition's predicate is true
            {
                return transition;
            }
        }

        return null;
    }

    public void AddTransition(IState from, IState to, IPredicate condition) //Add a transition to the state machine
    {
        GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition); //Not recursion since the AddTransition here is the one from StateNode
    }

    public void AddAnyTransition(IState to, IPredicate condition) //Add an any transition
    {
        anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
    }

    StateNode GetOrAddNode(IState state) //Add a state node into the state node dictionary
    {
        var node = nodes.GetValueOrDefault(state.GetType());

        if (node == null)
        {
            node = new StateNode(state);
            nodes.Add(state.GetType(), node);
        }
        
        return node;
    }

    //Use this instead of the GameManager's go back a state if you need to go back from an anytransition like pausing
    public void GoToPreviousState() 
    {
        if (previous != null)
        {
            ChangeState(previous.State);
        }
        
    }
    
    class StateNode //Class for a state node
    {
        public IState State { get; } //Hold the State type
        public HashSet<ITransition> Transitions { get; } //Hold all the transitions from this state

        public StateNode(IState state)
        {
            State = state;
            Transitions = new HashSet<ITransition>();
        }

        public void AddTransition(IState to, IPredicate condition) //Add a transition to the hashset of this State
        {
            Transitions.Add(new Transition(to, condition));
        }
    }
}
