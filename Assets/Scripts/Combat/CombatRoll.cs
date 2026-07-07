using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CombatRoll : MonoBehaviour
{
    [SerializeField]
    Sprite[] diceSprites;
    private GameObject dicePanel;
    [SerializeField]
    float flipDelay = 0.001f;
    void Start()
    {
        dicePanel = transform.Find("Dice").gameObject;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void rollDice()
    {
        int randomRoll = Random.Range(0, diceSprites.Length);
    }
    
   
    
}
