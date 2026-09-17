using System.Collections;
using UnityEngine;

public class MenuAnimation : MonoBehaviour
{
    private Animator animator;
    private Vector3 previousScale;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
    }
    private void Start()
    {
        // Capture exact scale after all Awake initializations finish
        previousScale = transform.localScale;
        Debug.Log(previousScale);
    }

    private void Update()
    {
        Vector3 currentScale = transform.localScale;

        // Detect when scale transitions from 0 to 1
        if (previousScale == Vector3.zero && currentScale != Vector3.zero)
        {
            Debug.Log(currentScale);
            PlayMenuAnimation();
        }

        previousScale = currentScale;
    }

    private void PlayMenuAnimation()
    {
        if (animator != null)
        {
            //Reset bindings and force playback from frame 0
            animator.Rebind();
            animator.Update(0f);
            animator.Play(0, 0, 0f);
            //Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        }
    }
}