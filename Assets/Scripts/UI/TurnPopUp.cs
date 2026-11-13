using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

public class TurnPopUp : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(AnimateObject());
    }

    private IEnumerator AnimateObject()
    {
        Vector3 originalScale = gameObject.transform.localScale;
        Vector3 targetScale = new Vector3(1.4f, 1.4f, 1.4f);
        float lerpTime = 0.15f;
        float elapsedTimeA = 0;
        float elapsedTimeB = 0;
        while (elapsedTimeA < lerpTime)
        {
            elapsedTimeA += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTimeA / lerpTime);
            gameObject.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        yield return new WaitForSeconds(0.4f);
        while (elapsedTimeB < lerpTime)
        {
            elapsedTimeB += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTimeB / lerpTime);
            gameObject.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);
        gameObject.SetActive(false);
    }
}
