using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CombatRoll : MonoBehaviour
{
    [SerializeField]
    Sprite[] diceSprites; 
    private GameObject dicePanel; //Child gameobject which is just the sprite
    private Animator diceAnimator;
    [SerializeField]
    float cycleDelay = 2f; //The delay from the start of the animation to the roll result
    public int rollResult; //the result of a roll. public for access

    private void Awake()
    {
        dicePanel = transform.Find("Dice").gameObject;
        diceAnimator = dicePanel.GetComponent<Animator>();
    }

    public void RollTo(int finalValue)
    {
        rollResult = finalValue;
        StartCoroutine(CycleDice(finalValue));
    }
    
    //rolling animation
    private IEnumerator CycleDice(int result)
    {
        diceAnimator.enabled = true;
        diceAnimator.SetTrigger("Roll");
        yield return new WaitForSeconds(cycleDelay);
        diceAnimator.Play("Idle");
        diceAnimator.enabled = false;

        if (result >= 1 && result <= diceSprites.Length)
        {
            dicePanel.GetComponent<Image>().sprite = diceSprites[result-1];
        }
    }
}
