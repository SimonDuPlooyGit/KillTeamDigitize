using System.Collections.Generic;
using UnityEngine;

public class TutManager : MonoBehaviour
{
    [Header("Sequence Data")]
    [SerializeField] private List<TutorialStepAction> tutorialSteps = new List<TutorialStepAction>();

    [Header("Runtime State")]
    [SerializeField] private int currentStepIndex = -1;

    private void OnEnable()
    {
        TutEvents.OnAdvanceTutorial += ExecuteNextStep;
    }

    private void OnDisable()
    {
        TutEvents.OnAdvanceTutorial -= ExecuteNextStep;
    }

    private void Start()
    {
        // Execute Step 0 when scene starts
        ExecuteNextStep();
    }

    public void ExecuteNextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Count)
        {
            Debug.Log("[TutManager] Tutorial sequence finished or missing.");
            return;
        }

        TutorialStepAction step = tutorialSteps[currentStepIndex];

        // Disable specified panels for this step
        foreach (GameObject panel in step.panelsToDisable)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Enable specified panels for this step
        foreach (GameObject panel in step.panelsToEnable)
        {
            if (panel != null) panel.SetActive(true);
        }

        Debug.Log($"[TutManager] Executed Step {currentStepIndex}: {step.stepName}");
    }
}