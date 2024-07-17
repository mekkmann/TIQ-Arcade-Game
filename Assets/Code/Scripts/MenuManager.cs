using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    const int MAINMENUINDEX = 0;
    const int GAMEINDEX = 1;

    private GameObject _mainMenuUI;
    private GameObject _settingsMenuUI;
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

        _mainMenuUI = GameObject.Find("MainMenu");
        _mainMenuUI.SetActive(true);
        _settingsMenuUI = GameObject.Find("SettingsMenu");
        _settingsMenuUI.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case MAINMENUINDEX:
                    ToggleSettingsMenu();
                    break;
                case GAMEINDEX:
                    TogglePauseMenu();
                    break;
                default:
                    Debug.LogError("Somehow an unaccounted and non-existent Scene caused this issue");
                    break;
            }
        }
    }

    private void TogglePauseMenu()
    {
        throw new NotImplementedException();
    }

    public void ToggleSettingsMenu()
    {
        _mainMenuUI.SetActive(!_mainMenuUI.activeSelf);
        _settingsMenuUI.SetActive(!_settingsMenuUI.activeSelf);
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadGameScene()
    {
        LoadScene(GAMEINDEX);
    }

    public void LoadMainMenu()
    {
        LoadScene(MAINMENUINDEX);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleSound()
    {
        AudioListener.pause = !AudioListener.pause;
        Debug.Log($"AudioListener is paused: [{AudioListener.pause}]");
    }
}