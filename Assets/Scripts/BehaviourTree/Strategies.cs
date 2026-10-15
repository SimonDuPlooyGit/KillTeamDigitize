using System;

namespace BehaviourTrees
{
    public interface IStrategy
        {
            Node.Status Process();

            void Reset()
            {
                //no op
            }
        }

    public class Condition : IStrategy
    {
        readonly Func<bool> predicate;

        public Condition(Func<bool> predicate)
        {
            this.predicate = predicate;
        }
        
        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    public class ActionStrategy : IStrategy
    {
        readonly Action doSomething;

        public ActionStrategy(Action doSomething)
        {
            this.doSomething = doSomething;
        }

        public Node.Status Process()
        {
            doSomething();
            return Node.Status.Success;
        }
    }

    public class AsyncActionStrategy : IStrategy
    {
        readonly Action startAction;
        readonly Func<bool> isCompleted;
        private bool hasStarted = false;

        public AsyncActionStrategy(Action startAction, Func<bool> isCompleted)
        {
            this.startAction = startAction;
            this.isCompleted = isCompleted;
        }

        public Node.Status Process()
        {
            if (!hasStarted)
            {
                startAction?.Invoke();
                hasStarted = true;
            }

            if (isCompleted())
            {
                hasStarted = false;
                return Node.Status.Success;
            }
            
            return Node.Status.Running;
        }

        public void Reset()
        {
            hasStarted = false;
        }
    }
}

