using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FlowLog : MonoBehaviour
{
    [SerializeField]
    public GameObject flowLogContent;
    public GameObject logPrefab;
    
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        GameObject newLog = Instantiate(logPrefab, flowLogContent.transform);
        newLog.GetComponent<TextMeshProUGUI>().text = logString;
    }
}
