using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    [SerializeField]
    private MenuPanel menu;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            TogglePauseScreen();
        }
    }

    public void TogglePauseScreen()
    {
        if (gameObject.transform.localScale == Vector3.zero)
        {
            menu.OpenMenu(menu.PauseScreen);
            Time.timeScale = 0f;
        }
        else if (gameObject.transform.localScale == Vector3.one)
        {
            menu.CloseMenu(menu.PauseScreen);
            Time.timeScale = 1f;
        }
    }
}
