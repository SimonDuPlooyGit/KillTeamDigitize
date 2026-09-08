using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatRoll : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Sprite[] diceSprites; 
    private GameObject dicePanel; //Child gameobject which is just the sprite
    private Animator diceAnimator;
    private Image diceImage;
    public DiceRoll PhysicalDie { get; private set; } //reference for the physical dice correspondong with the panel
    public bool IsSelected { get; private set; }
    private DiceHandler handler;
    private bool isAttackDie;

    private void Awake()
    {
        dicePanel = transform.Find("Dice").gameObject;
        diceAnimator = dicePanel.GetComponent<Animator>();
        diceImage = dicePanel.GetComponent<Image>();
    }

    public void Initialize(DiceRoll physicalDie, DiceHandler diceHandler, bool isAttack)
    {
        PhysicalDie = physicalDie;  
        handler = diceHandler;
        isAttackDie = isAttack;

        int value = physicalDie.GetUpwardFace();
        diceAnimator.enabled = false;
        diceImage.sprite = diceSprites[value - 1];

        Deselect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Toggle off if already selected
        if (IsSelected)
        {
            Deselect();
            return;
        }

        // Check with DiceHandler if max selected panels exceeded
        if (handler != null && handler.CanSelectDie(isAttackDie))
        {
            Select();
        }
    }
    public void Select()
    {
        IsSelected = true;
        diceImage.color = Color.black; //temp feedback for now
    }

    public void Deselect()
    {
        IsSelected = false;
        diceImage.color = Color.white;
    }

    //might not be needed anymore, may delete later
    public void RollTo(int finalValue)
    {
        diceAnimator.enabled = false;
        dicePanel.GetComponent<Image>().sprite = diceSprites[finalValue - 1];
    }
}
