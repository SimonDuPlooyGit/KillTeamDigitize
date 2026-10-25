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
    public Transform hitIndicator;
    public Transform blockIndicator;
    private bool _isAttack;

    private void Awake()
    {
        _dicePanel = transform.Find("Dice").gameObject;
        _diceAnimator = _dicePanel.GetComponent<Animator>();
        _diceImage = _dicePanel.GetComponent<Image>();
        hitIndicator = transform.Find("Hit");
        blockIndicator = transform.Find("Block");
        hitIndicator.gameObject.SetActive(false);
        blockIndicator.gameObject.SetActive(false);
    }

    public void Initialize(DiceRoll physicalDie, DiceHandler diceHandler, bool isAttack)
    {
        PhysicalDie = physicalDie;  
        _handler = diceHandler;
        _isAttackDie = isAttack;
        _isAttack = isAttack;

        int value = physicalDie.GetUpwardFace();
        _diceAnimator.enabled = false;
        _diceImage.sprite = diceSprites[value - 1];

        IsSelected = false;
        _diceImage.color = Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_handler == null) return;

        if (IsSelected)
        {
            Deselect();
        }
        else
        {
            
            if (_handler.CanSelectDie(this, _isAttack))
            {
                Select();
            }
        }
    }
    public void Select()
    {
        IsSelected = true;
        _diceImage.color = Color.black; //temp feedback for now

        if (_handler != null)
        {
            _handler.UpdateRerollButtonVisibility();
            _handler.OnDieSelectedStateChanged(this, _isAttack, true);
        }
    }

    public void Deselect()
    {
        IsSelected = false;
        _diceImage.color = Color.white;

        if (_handler != null)
        {
            _handler.UpdateRerollButtonVisibility();
            _handler.OnDieSelectedStateChanged(this, _isAttack, false);
        }
    }

    public int GetValue()
    {
        for(int i =0; i<diceSprites.Length; i++)
        {
            if (diceSprites[i] == _diceImage.sprite)
            {
                return i+1;
            }
        }
        return 0;
    }

    public void ToggleHit()
    {
        hitIndicator.gameObject.SetActive(true);
    }

    public void ToggleBlock()
    {
        blockIndicator.gameObject.SetActive(true);
    }
}