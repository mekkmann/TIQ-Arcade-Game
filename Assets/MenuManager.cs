using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    const int MAINMENUINDEX = 0;
    const int GAMEINDEX = 1;
    public static MenuManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void LoadGameScene()
    {
        LoadSceneByIndex(GAMEINDEX);
    }

    public void LoadMainMenu()
    {
        LoadSceneByIndex(MAINMENUINDEX);
    }
}
