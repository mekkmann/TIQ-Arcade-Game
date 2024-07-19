using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    const int MAINMENUINDEX = 0;
    const int GAMEINDEX = 1;

    private GameObject _mainMenuUI;
    private GameObject _settingsMenuUI;

    [SerializeField] private AudioSource _centerAudioSource;

    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Image _muteImage;

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
        _volumeSlider.onValueChanged.AddListener(HandleVolumeChange);
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
        if (!_centerAudioSource.isPlaying)
        {
            _centerAudioSource.Play();
            _muteImage.enabled = true;
        }
        else
        {
            _centerAudioSource.Stop();
            _muteImage.enabled = false;
        }
        Debug.Log($"audioSource is stopped: [{_centerAudioSource.isPlaying} and then we have the image, is it showing: {_muteImage.enabled}]");

    }

    public void HandleVolumeChange(float newVolume)
    {
        if (newVolume < 0f || newVolume > 100f)
        {
            return;
        }

        _centerAudioSource.volume = newVolume;

        if (!_centerAudioSource.isPlaying)
        {
            _centerAudioSource.Play();
        }

        Debug.Log(_centerAudioSource.volume);
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
        HandleMute();
    }
}