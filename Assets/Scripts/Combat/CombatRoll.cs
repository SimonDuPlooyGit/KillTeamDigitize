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
        RollDice();
    }

    //Rolls a random number, saves the result to rollResult, and starts the rolling animation
    public void RollDice()
    {
        int randomRoll = Random.Range(1, diceSprites.Length+1);
        rollResult = randomRoll;
        StartCoroutine(CycleDice(randomRoll));
      
    }
    
    //rolling animation
    private IEnumerator CycleDice(int rand)
    {
        diceAnimator.enabled = true;
        diceAnimator.SetTrigger("Roll");
        yield return new WaitForSeconds(cycleDelay);
        diceAnimator.Play("Idle");
        diceAnimator.enabled = false;
        dicePanel.GetComponent<Image>().sprite = diceSprites[rand-1];

    }
    
}
