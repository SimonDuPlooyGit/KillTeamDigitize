using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FlowLog : MonoBehaviour
{
    [SerializeField]
    public GameObject flowLogContent;
    public GameObject logPrefab;
    [SerializeField] 
    private ScrollRect scrollRect;

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
}
