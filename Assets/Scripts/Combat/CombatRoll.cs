using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//-----CombatRoll Class-----
public class CombatRoll : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Sprite[] diceSprites; 
    private GameObject _dicePanel; //Child gameObject which is just the sprite
    private Animator _diceAnimator;
    private Image _diceImage;
    public DiceRoll PhysicalDie { get; private set; } //reference for the physical dice corresponding with the panel
    public bool IsSelected { get; private set; }
    private DiceHandler _handler;
    private bool _isAttackDie;
    
    private void Awake()
    {
        _dicePanel = transform.Find("Dice").gameObject;
        _diceAnimator = _dicePanel.GetComponent<Animator>();
        _diceImage = _dicePanel.GetComponent<Image>();
    }

    public void Initialize(DiceRoll physicalDie, DiceHandler diceHandler, bool isAttack)
    {
        PhysicalDie = physicalDie;  
        _handler = diceHandler;
        _isAttackDie = isAttack;

        int value = physicalDie.GetUpwardFace();
        _diceAnimator.enabled = false;
        _diceImage.sprite = diceSprites[value - 1];

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

        // Check with Dice_handler if max selected panels exceeded
        if (_handler != null && _handler.CanSelectDie(_isAttackDie))
        {
            Select();
        }
    }
    private void Select()
    {
        IsSelected = true;
        _diceImage.color = Color.black; //temp feedback for now

        if (_handler != null)
        {
            _handler.UpdateRerollButtonVisibility();
        }
    }

    private void Deselect()
    {
        IsSelected = false;
        _diceImage.color = Color.white;

        if (_handler != null)
        {
            _handler.UpdateRerollButtonVisibility();
        }
    }
}