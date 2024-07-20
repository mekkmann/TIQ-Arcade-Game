using System;
using System.Collections;
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
    [SerializeField] private TextMeshProUGUI _restartText;
    [SerializeField] private Light2D _light2D;
    [SerializeField] private UIHandler _player1UIHandler;
    [SerializeField] private UIHandler _player2UIHandler;

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
            Debug.Log("Should restart game");
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
    private void HandlePauseMenu()
    {
        if (_pauseUI != null)
        {
            _pauseUI.SetActive(!_pauseUI.activeInHierarchy);
            if (!_pauseUI.activeInHierarchy)
            {
                Time.timeScale = 1.0f;
            } else
            {
                Time.timeScale = 0f;
            }

        } else
        {
            _pauseUI = GameObject.Find("Pause_UI");
        }
    }
    //private void RestartGame()
    //{
    //    _player1.SpawnAt(_player1SpawnPoint);
    //    _player2.SpawnAt(_player2SpawnPoint);
    //    _player1.GameReset();
    //    _player2.GameReset();
    //    _player1UIHandler.ResetHearts();
    //    _player2UIHandler.ResetHearts();
    //    _winnerText.enabled = false;
    //    _restartText.enabled = false;
    //}
    public event Action RoundResetEvent;
    public void StartRound()
    {
        if (CheckIfPlayerHasWon())
        {
            return;
        }
        _player1.SpawnAt(_player1SpawnPoint);
        _player2.SpawnAt(_player2SpawnPoint);
        _player1.RoundReset();
        _player2.RoundReset();
    }

    private void PlayerDied(int test)
    {
        StartRound();
    }
    public bool CheckIfPlayerHasWon()
    {
        if (_player1.LivesRemaining == 0)
        {
            Debug.Log("Player 2");
            return true;
        }
        else if (_player2.LivesRemaining == 0)
        {
            Debug.Log("Player 1");
            return true;
        }

        return false;
    }

}
