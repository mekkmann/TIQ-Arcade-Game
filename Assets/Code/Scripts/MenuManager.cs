using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    const int MAINMENUINDEX = 0;
    const int GAMEINDEX = 1;

#nullable enable
    private GameObject? _mainMenuUI;
    private GameObject? _settingsMenuUI;
#nullable disable

    [SerializeField] private AudioSource _centerAudioSource;

    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Image _checkmark;

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

        if (SceneManager.GetActiveScene().buildIndex == MAINMENUINDEX)
        {
            _mainMenuUI = GameObject.Find("MainMenu");
            _mainMenuUI.SetActive(true);
            _settingsMenuUI = GameObject.Find("SettingsMenu");
            _settingsMenuUI.SetActive(false);
            _volumeSlider.onValueChanged.AddListener((call) => HandleVolumeChange(call));
        }
        
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

    public void HandleMute()
    {
        if (!_centerAudioSource.mute)
        {
            _centerAudioSource.mute = true;
            _checkmark.enabled = false;
        }
        else
        {
            _centerAudioSource.mute = false;
            _checkmark.enabled = true;
        }
    }

    public void HandleVolumeChange(float newVolume)
    {
        if (newVolume < 0f || newVolume > 1f)
        {
            return;
        }

        if (_centerAudioSource.mute) _centerAudioSource.mute = !_centerAudioSource.mute;

        _centerAudioSource.volume = newVolume;

        if (!_centerAudioSource.isPlaying)
        {
            _centerAudioSource.Play();
        }
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

}