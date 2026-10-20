using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShootButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float lerpTime = 0.1f;
    private Transform parentTransform;
    private Vector3 origScale;
    private Vector3 targetScale = new Vector3(1.05f, 1.05f, 1.05f);
    private Coroutine scaleCoroutine;
    private void Awake()
    {
        parentTransform = transform.parent.gameObject.transform;
        origScale = parentTransform.localScale;
       
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(origScale));
    }

    private IEnumerator ScaleTo(Vector3 endScale)
    {
        Vector3 startScale = parentTransform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / lerpTime);

            
            parentTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        parentTransform.localScale = endScale;
    }


}