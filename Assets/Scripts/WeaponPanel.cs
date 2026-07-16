using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] public TextMeshProUGUI name;
    [SerializeField] public TextMeshProUGUI atk;
    [SerializeField] public TextMeshProUGUI hit;
    [SerializeField] public TextMeshProUGUI dmg;
    [SerializeField] public TextMeshProUGUI wr;

    //Callback pattern (C# Actions and delegates)
    private WeaponTemplate _weapon;
    private Action<WeaponTemplate> _onSelectedCallback;

    public void Setup(WeaponTemplate weapon, Action<WeaponTemplate> onSelected)
    {
        _weapon = weapon;
        _onSelectedCallback = onSelected;

        name.text = weapon.name;
        atk.text = weapon.ATK.ToString();
        hit.text = weapon.HIT.ToString();
        dmg.text = $"{weapon.DMGnorm}/{weapon.DMGcrit}";

        string rulesText = "";
        foreach (var rule in weapon.rules)
        {
            rulesText += rule.GetType().Name + "";
        }
        wr.text = rulesText;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnPanelClicked);
    }

    private void OnPanelClicked()
    {
        _onSelectedCallback?.Invoke(_weapon); //Invoke event ? is a null check for if something is subscribed
    }
}
