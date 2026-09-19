using UnityEngine;
using UnityEngine.InputSystem;

public class ToolTipScroll : MonoBehaviour
{
    private GameObject content;
    [SerializeField]
    private float scrollSpeed;
    private RectTransform contentRect;
    private float startingPos;
    [SerializeField] 
    private float scrollClamp = 250f;
    private float extraClamp = 0;
    void Start()
    {
        content = GameObject.Find("WeaponRuleHolder");

        if (content != null)
        {
            contentRect = content.GetComponent<RectTransform>();
            startingPos = -877.0505f;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(Mouse.current != null)
        {
            float scrollInput = Mouse.current.scroll.y.ReadValue();

            if (Mathf.Abs(scrollInput) > 0.01f && content.transform.childCount >=4)
            {
                if (content.transform.childCount > 4)
                {
                    int extraChildren = content.transform.childCount -4;
                    extraClamp = scrollClamp + (extraChildren *200);
                }
                else
                {
                    extraClamp = 0f; 
                }

                float normalizedScroll = Mathf.Sign(scrollInput);

                Vector2 currentPos = contentRect.anchoredPosition;
                float newX = currentPos.x + (normalizedScroll * scrollSpeed * Time.deltaTime * 100f);

                newX = Mathf.Clamp(newX, startingPos - (scrollClamp + extraClamp), startingPos + 100);

                contentRect.anchoredPosition = new Vector2(newX, currentPos.y);
            }
        }

        
    }
}
