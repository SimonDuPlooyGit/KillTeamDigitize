using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Animator animator;
    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");
    [SerializeField]
    private Canvas canvasFront;
    [SerializeField]
    private Canvas canvasInside;
    [SerializeField]
    private Canvas canvasInside2;
    [SerializeField]
    private Image fadeImage;
    private float fadeDuration = 1.0f;
    void Start()
    {
       animator = GetComponent<Animator>();
        CloseBook(); 
    }

    public void CloseBook()
    {
        StartCoroutine(CloseBookSequence());
    }

    public void OpenBook()
    {
        StartCoroutine(OpenBookSequence());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator OpenBookSequence()
    {
        animator.SetBool(IsOpenHash, true);
        canvasFront.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.4f);
        canvasInside.gameObject.SetActive(true);
        canvasInside2.gameObject.SetActive(true);
    }

    private IEnumerator CloseBookSequence()
    {
        animator.SetBool(IsOpenHash, false);
        canvasInside.gameObject.SetActive(false);
        canvasInside2.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.4f);
        canvasFront.gameObject.SetActive(true);
    }

    public void PlayGame()
    {
        StartCoroutine(FadeRoutine(0f,1f));
        
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha)
    {
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

        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene("SampleScene");
    }
}
