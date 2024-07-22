using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;


public class GameManager : TransientSingleton<GameManager>
{
    const int MAINMENU_INDEX = 0;
    const int ARENA_INDEX = 1;

    [SerializeField] private PlayerController _player1;
    [SerializeField] private Transform _player1SpawnPoint;
    [SerializeField] private PlayerController _player2;
    [SerializeField] private Transform _player2SpawnPoint;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private GameObject _winnerDisplay;
    [SerializeField] private Light2D _light2D;
    [SerializeField] private PlayerHUD _player1HUD;
    [SerializeField] private PlayerHUD _player2HUD;

    [SerializeField] private GameObject _pauseUI;

    void Start()
    {
        _player1.PlayerDiedEvent += (livesRemaining) => PlayerDied(livesRemaining);
        _player2.PlayerDiedEvent += (livesRemaining) => PlayerDied(livesRemaining);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandlePauseMenu();
        }
    }

    public void LoadArena()
    {
        SceneManager.LoadScene(ARENA_INDEX);
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MAINMENU_INDEX);
    }
    public void HandlePauseMenu()
    {
        if (_pauseUI != null)
        {
            _pauseUI.SetActive(!_pauseUI.activeInHierarchy);
            if (!_pauseUI.activeInHierarchy)
            {
                Time.timeScale = 1.0f;
            }
            else
            {
                Time.timeScale = 0f;
            }

        }
        else
        {
            _pauseUI = GameObject.Find("Pause_UI");
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(ARENA_INDEX);
    }
    //public event Action RoundResetEvent;
    public void StartStartRound() => StartCoroutine(nameof(StartRound));
    public void StartRound()
    {
        if (CheckIfPlayerHasWon())
        {
            return;
        }
        else
        {
            _player1.SpawnAt(_player1SpawnPoint);
            _player2.SpawnAt(_player2SpawnPoint);
            _player1.RoundReset();
            _player2.RoundReset();
        }

    }

    private void PlayerDied(int i)
    {
        StartRound();
    }
    public bool CheckIfPlayerHasWon()
    {
        if (_player1.LivesRemaining == 0)
        {
            HandleWinnerScreen(false);
            return true;
        }
        else if (_player2.LivesRemaining == 0)
        {
            HandleWinnerScreen(true);
            return true;
        }

        return false;
    }

    private void HandleWinnerScreen(bool p1)
    {
        _winnerDisplay.SetActive(!_winnerDisplay.activeInHierarchy);
        _winnerText.text = $"Player {(p1 ? "1" : "2")} wins!";
    }

}
