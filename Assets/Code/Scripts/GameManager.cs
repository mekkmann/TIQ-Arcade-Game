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
    [SerializeField] private Light2D _light2D;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartRound();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartRound()
    {
        if (CheckIfPlayerHasWon())
        {
            return;
        }
        Debug.Log("Shouldnt see this");
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
