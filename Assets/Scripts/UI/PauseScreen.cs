using UnityEngine;
using UnityEngine.SceneManagement;

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
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseScreen();
        }
    }

    public void TogglePauseScreen()
    {
        if (gameObject.transform.localScale == Vector3.zero)
        {
            menu.OpenMenu(menu.PauseScreen);
            SoundManager.SetMusicVolumes(0.15f, 0.05f);
        }
        else if (gameObject.transform.localScale == Vector3.one)
        {
            menu.CloseMenu(menu.PauseScreen);
            SoundManager.SetMusicVolumes(0.4f, 0.2f);
        }
    }

    public void GoToScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
