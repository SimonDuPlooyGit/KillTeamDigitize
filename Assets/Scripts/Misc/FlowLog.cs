using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlowLog : MonoBehaviour
{
    [SerializeField]
    public GameObject flowLogContent;
    public GameObject logPrefab;
    [SerializeField] 
    private ScrollRect scrollRect;
    [SerializeField]
    private float lerpTime = 0.5f;
    [SerializeField]
    private float autoCloseDelay = 3.0f;

    private bool isOut = false;
    private Vector2 inPos = new Vector2(1367f, -426f);
    private Vector2 outPos = new Vector2(500f, -426f);
    private RectTransform rectTransform;

    private Coroutine slideCoroutine;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type) //You can call a Debug.LogException and then it will pop up in the gameplay flow UI
    {
        if (type == LogType.Warning && !stackTrace.Contains("IsPointerOverGameObject"))
        {
            GameObject newLog = Instantiate(logPrefab, flowLogContent.transform);
            newLog.GetComponent<TextMeshProUGUI>().text = logString;
            StartCoroutine(ScrollToBottom());

            SetLogState(true);

            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
            }
            autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
        }
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();

        //sets scroll rect to bottom
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void SetLogState(bool open)
    {
        //dont lerp if its already in the correct spot
        if (isOut == open) return;

        isOut = open;

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        Vector2 target = isOut ? outPos : inPos;
        slideCoroutine = StartCoroutine(Slide(rectTransform.anchoredPosition, target));
    }

    private IEnumerator AutoCloseTimer()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        SetLogState(false);
        autoCloseCoroutine = null;
    }

    public void SlideAnim()
    {
        EventSystem.current?.SetSelectedGameObject(null);

        //cancel the buffer timer if manually toggling the log
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        SoundManager.PlaySound(1);
        SetLogState(!isOut);
    }

    private IEnumerator Slide(Vector2 startPos, Vector2 targetPos)
    {
        float elapsedTime = 0f;
        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / lerpTime);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        rectTransform.anchoredPosition = targetPos;
        slideCoroutine = null;
    }


}
