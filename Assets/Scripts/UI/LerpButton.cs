using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class LerpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float lerpTime = 0.1f;
    private Transform parentTransform;
    private Vector3 origScale;
    [SerializeField]
    private Vector3 targetScale = new Vector3(1.05f, 1.05f, 1.05f);
    private Coroutine scaleCoroutine;
    //If true, hovering over an object will lerp its parent object's scale. set to false to scale the gameobject with the script
    [SerializeField]
    private bool isParentLerp = true;
    private void Awake()
    {
        //Set original scale to lerp back to
        parentTransform = transform.parent.gameObject.transform;
        if(isParentLerp)
        {
            origScale = parentTransform.localScale;
        }
        else
        {
            origScale = gameObject.transform.localScale;
        }
        
       
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //When hovered over scale to target to scale
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //When no longer hovering scale back to original scale
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(origScale));
    }

    //Takes in a scale then lerps the object's scale to that scale
    private IEnumerator ScaleTo(Vector3 endScale)
    {
        Vector3 startScale;
        if(isParentLerp) 
        {
            startScale = parentTransform.localScale;
        }
        else
        {
            startScale = gameObject.transform.localScale;
        }
        
        float elapsedTime = 0f;

        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / lerpTime);

            if(isParentLerp) 
            {
                parentTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            }
            else
            {
                gameObject.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            }
            

            yield return null;
        }

        if(isParentLerp)
        {
            parentTransform.localScale = endScale;
        }
        else
        {
            gameObject.transform.localScale = endScale;
        }
        
    }


}