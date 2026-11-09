using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutManager : MonoBehaviour
{
    [Header("Sequence Data")]
    [SerializeField] private List<TutorialStepAction> tutorialSteps = new List<TutorialStepAction>();

    [Header("Runtime State")]
    [SerializeField] private int currentStepIndex = -1;

    [Header("Transition Settings")]
    [SerializeField]
    private float yieldTime = 1f;
    [SerializeField]
    private float fadeDuration = 1f;
    [SerializeField]
    private Image fadeImage;

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
        fadeImage.color = new Color32(0, 0, 0, 255);
        StartCoroutine(IntroFade(1f,0f));
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

        if (!string.IsNullOrEmpty(step.sceneToLoad))
        {
            Debug.Log($"[TutManager] Transitioning to scene: {step.sceneToLoad}");
            StartCoroutine(TransitionToScene(step.sceneToLoad));
        }
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        // Optional delay before fade begins
        if (yieldTime > 0f) yield return new WaitForSeconds(yieldTime);

        // Wait for fade-out to finish BEFORE loading scene
        yield return StartCoroutine(FadeRoutine(0f,1f));

        SceneManager.LoadScene(sceneName);
    }
    private IEnumerator IntroFade(float startAlpha, float targetAlpha)
    {
        yield return new WaitForSeconds(0.5f);
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        fadeImage.raycastTarget = true;

        Color startColor = new Color(color.r, color.g, color.b, startAlpha);
        Color targetColor = new Color(color.r, color.g, color.b, targetAlpha);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        fadeImage.color = targetColor;
        fadeImage.raycastTarget = false;
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha)
    {
        yield return new WaitForSeconds(yieldTime);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        Color startColor = new Color(color.r, color.g, color.b, startAlpha);
        Color targetColor = new Color(color.r, color.g, color.b, targetAlpha);

        fadeImage.raycastTarget = true;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        fadeImage.color = targetColor;

        yield return new WaitForSeconds(0.2f);
    }

    public void StartTutButton()
    {
        SoundManager.PlaySound(1);
        TutEvents.AdvanceTutorial();
    }
}