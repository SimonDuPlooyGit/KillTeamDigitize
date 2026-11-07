using System;

public static class TutEvents
{
    public static event Action OnAdvanceTutorial;

    public static void AdvanceTutorial()
    {
        OnAdvanceTutorial?.Invoke();
    }
}