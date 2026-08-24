using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CombatRoll : MonoBehaviour
{
    [SerializeField]
    Sprite[] diceSprites; 
    private GameObject dicePanel; //Child gameobject which is just the sprite
    private Animator diceAnimator;
    
    private void Awake()
    {
        dicePanel = transform.Find("Dice").gameObject;
        diceAnimator = dicePanel.GetComponent<Animator>();
    }

    public void RollTo(int finalValue)
    {
        diceAnimator.enabled = false;
        dicePanel.GetComponent<Image>().sprite = diceSprites[finalValue - 1];
    }
}
