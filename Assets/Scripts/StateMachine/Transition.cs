public class Transition : ITransition //Transition class implementing ITransition
{
    public IState To { get; }
    public IPredicate Condition { get; }

    public Transition(IState to, IPredicate condition)
    {
        To = to;
        Condition = condition;
        //^ Public access to read To and Condition fulfilled
    }
}
