using System;

namespace BehaviourTree
{
    //-----Base IStrategy Interface-----
    public interface IStrategy
        {
            Node.Status Process();

            void Reset()
            {
                //no op
            }
        }

    //-----Condition Class (IStrategy that evaluates a predicate)-----
    public class Condition : IStrategy
    {
        readonly Func<bool> _predicate;

        public Condition(Func<bool> predicate)
        {
            _predicate = predicate;
        }
        
        public Node.Status Process() => _predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    //-----Action Strategy Class (Evaluates an immediate action)-----
    public class ActionStrategy : IStrategy
    {
        readonly Action _doSomething; //Action is a built-in delegate from System.Action. You can pass a function into it

        public ActionStrategy(Action doSomething)
        {
            _doSomething = doSomething;
        }

        public Node.Status Process()
        {
            _doSomething();
            return Node.Status.Success;
        }
    }

    //-----AsyncActionStrategy (Evaluates an action that takes time to complete like coroutines or navmesh movements)-----
    public class AsyncActionStrategy : IStrategy
    {
        readonly Action _startAction;
        readonly Func<bool> _isCompleted;
        private bool _hasStarted;

        public AsyncActionStrategy(Action startAction, Func<bool> isCompleted)
        {
            _startAction = startAction;
            _isCompleted = isCompleted;
            _hasStarted = false;
        }

        public Node.Status Process()
        {
            if (!_hasStarted)
            {
                _startAction?.Invoke();
                _hasStarted = true;
            }

            if (_isCompleted())
            {
                _hasStarted = false;
                return Node.Status.Success;
            }
            
            return Node.Status.Running;
        }

        public void Reset()
        {
            _hasStarted = false;
        }
    }
}