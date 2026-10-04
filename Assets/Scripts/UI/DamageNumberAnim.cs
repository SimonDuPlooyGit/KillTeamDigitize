using System.Collections;
using UnityEngine;

public class DamageNumberAnim : MonoBehaviour
{
    private Animator animator;
    private int trigger;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        trigger = Animator.StringToHash("PlayAnim");
    }

    // Update is called once per frame
    private void OnEnable()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        animator.ResetTrigger("PlayAnim");
        animator.SetTrigger("PlayAnim");
        yield return new WaitForSeconds(2);
        gameObject.SetActive(false);
    }
}
