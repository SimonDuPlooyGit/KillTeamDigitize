using UnityEngine;

public interface IState
{ 
   //The interface for a state
   
   //Every class implementing IState is guaranteed to have these methods below via "contract"
   //The classes can specify their own logic for these methods
   void OnEnter(); 
   void Update();
   void OnExit();
}


