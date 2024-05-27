using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController _player1;
    [SerializeField] private Transform _player1SpawnPoint;
    [SerializeField] private PlayerController _player2;
    [SerializeField] private Transform _player2SpawnPoint;

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
        _player1.SpawnAt(_player1SpawnPoint);
        _player2.SpawnAt(_player2SpawnPoint);
        _player1.RoundReset();
        _player2.RoundReset();
    }
}
