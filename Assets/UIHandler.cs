using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private Slider _healthBar;
    [SerializeField] private Slider _staminaBar;
    private PlayerController _player;
    public bool isPlayer1 = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _healthBar.value = _healthBar.maxValue;
        _staminaBar.value = _staminaBar.maxValue;
        if (isPlayer1)
        {
            _player = PlayerManager.player1;
        }
        else
        {
            _player = PlayerManager.player2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_player == null)
        {
            return;
        }

        if (_healthBar.value != _player.HealthPercentage)
        {
            _healthBar.value = _player.HealthPercentage;
        }

        if (_staminaBar.value != _player.StaminaPercentage)
        {
            _staminaBar.value = _player.StaminaPercentage;
        }

    }
}

public static class PlayerManager
{
    public static PlayerController player1;
    public static PlayerController player2;

    public static Action<PlayerController> playerSpawned;
}
