using System.Collections;
using TMPro;
using UnityEngine;

public class RulePopup : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(StartLerp());
    }

    private IEnumerator StartLerp()
    {
        Vector3 originalScale = gameObject.transform.localScale;
        Vector3 targetScale = new Vector3(1.3f, 1.3f, 1.3f);
        float lerpTime = 0.1f;
        float elapsedTimeA = 0;
        float elapsedTimeB = 0;
        while (elapsedTimeA < lerpTime)
        {
            elapsedTimeA += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTimeA / lerpTime);
            gameObject.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        while (elapsedTimeB < lerpTime)
        {
            elapsedTimeB += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTimeB / lerpTime);
            gameObject.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
    }
}
