using System.Collections.Generic;
using System.Linq;

namespace BehaviourTree //Helps to avoid collisions in variable names
{
    //-----Node Class-----
    public class Node //This is the structure of a node
    {
        public enum Status {Success, Failure, Running} //A node can succeed, fail or be busy running its code, this enum holds that state
    
        public readonly string Name; //The node has a name
        public readonly int Priority; //The node can be assigned a priority
    
        protected readonly List<Node> Children = new(); //The node keeps track of the nodes following it in the tree
        protected int CurrentChild; //Index to know which child node you are on
    
        protected Node(string name = "Node", int priority = 0) //Constructor
        {
            Name = name;
            Priority = priority;
        }
        
        public void AddChild(Node child) => Children.Add(child); //Method to add a child node
        public virtual Status Process() => Children[CurrentChild].Process(); //Recursive Process() Method returning a Status enum state
    
        public virtual void Reset() //Reset all the nodes in the chain
        {
            CurrentChild = 0;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }
    
    //-----BehaviourTree Class (Node chain of Processes)-----
    public class BehaviourTree : Node
    {
        public BehaviourTree(string name) : base(name) {}
    
        public override Status Process() //Run through every child's process and if they all return Success tree gives success
        {
            while (CurrentChild < Children.Count)
            {
                var status = Children[CurrentChild].Process();
                if (status != Status.Success)
                {
                    return status; //Otherwise return the Status of a child that is running of failed
                }
                CurrentChild++;
            }
            return Status.Success;
        }
    }

    //-----PrioritySelector Class (Node that can select child nodes according to priority)-----
    public class PrioritySelector : Node
    {
        List<Node> _sortedChildren;
        List<Node> SortedChildren => _sortedChildren ??= SortChildren(); //??= is a null-coalescing assignment operator
        //i.e. only assign this to sortedChildren if sortedChildren is null

        protected virtual List<Node> SortChildren() => Children.OrderByDescending(child => child.Priority).ToList();
        //Put the child nodes into descending priority order
        
        public PrioritySelector(string name, int priority = 0) : base(name, priority) {}

        public override void Reset() //Reset from base node but also clear the sortedChildren list
        {
            base.Reset();
            _sortedChildren = null;
        }

        public override Status Process() //Check all the child Processes and return them
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        return Status.Success;
                    default:
                        continue;
                }
            }

            return Status.Failure;
        }
    }
    
    //-----Selector Class (Checks status of node at currentChild index)-----
    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority) {}

        public override Status Process()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        CurrentChild++;
                        return Status.Running;
                }
            }
            Reset();
            return Status.Failure;
        }
    }
    
    //-----Sequence Class (Sequentially evaluate the child nodes one by one)-----
    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority) {}

        public override Status Process()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                    default:
                        CurrentChild++;
                        return CurrentChild == Children.Count ? Status.Success : Status.Running;
                }
            }
            Reset();
            return Status.Success;
        }
    }
    
    //-----Leaf class (The last node in a chain, this executes and tree works its way back up from here)-----
    public class Leaf : Node
    {
        readonly IStrategy _strategy; //Holds a strategy interface. This is the type of code or condition that will occur
    
        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            _strategy = strategy;
        }
        
        public override Status Process() => _strategy.Process(); //Actually evaluate the code of the leaf
        
        public override void Reset() => _strategy.Reset(); //Reset the strategy
    }
}