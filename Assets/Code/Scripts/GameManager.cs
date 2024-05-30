using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController _player1;
    [SerializeField] private Transform _player1SpawnPoint;
    [SerializeField] private PlayerController _player2;
    [SerializeField] private Transform _player2SpawnPoint;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _restartText;
    [SerializeField] private Light2D _light2D;
    [SerializeField] private UIHandler _player1UIHandler;
    [SerializeField] private UIHandler _player2UIHandler;

    void Start()
    {
        StartRound();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        _player1.SpawnAt(_player1SpawnPoint);
        _player2.SpawnAt(_player2SpawnPoint);
        _player1.GameReset();
        _player2.GameReset();
        _player1UIHandler.ResetHearts();
        _player2UIHandler.ResetHearts();
        _winnerText.enabled = false;
        _restartText.enabled = false;
    }

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

    public bool CheckIfPlayerHasWon()
    {
        if (_player1.LivesRemaining == 0)
        {
            ShowWinner("Player 2");
            return true;
        }
        else if (_player2.LivesRemaining == 0)
        {
            ShowWinner("Player 1");
            return true;
        }

        return false;
    }

    private void ShowWinner(string winner)
    {
        _winnerText.SetText(winner + " wins!");
        _winnerText.gameObject.SetActive(true);
        _restartText.gameObject.SetActive(true);
    }

    private IEnumerator GlobalLightRoutine()
    {
        while (_light2D.intensity < 500)
        {
            _light2D.intensity++;
            yield return new WaitForSeconds(0.1f);

        }
    }
}
