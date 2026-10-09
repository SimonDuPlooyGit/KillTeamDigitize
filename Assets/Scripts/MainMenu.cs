using UnityEngine;

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
    void Start()
    {
       animator = GetComponent<Animator>();
        CloseBook(); 
    }

    public void CloseBook()
    {
        animator.SetBool(IsOpenHash, false);
        canvasFront.gameObject.SetActive(true);
        canvasInside.gameObject.SetActive(false);
    }

    public void OpenBook()
    {
        animator.SetBool(IsOpenHash, true);
        canvasFront.gameObject.SetActive(false);
        canvasInside.gameObject.SetActive(true);
    }

    
}
