using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private Slider _healthBar;
    [SerializeField] private Slider _staminaBar;
    [SerializeField] private Image _heart1;
    [SerializeField] private Image _heart2;
    private PlayerController _player;
    public bool isPlayer1 = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _healthBar.value = _healthBar.maxValue;
        _staminaBar.value = _staminaBar.maxValue;
        if (isPlayer1)
        {
            _player = GameObject.Find("PlayerPrefab").GetComponent<PlayerController>();
        }
        else
        {
            _player = GameObject.Find("PlayerPrefab (1)").GetComponent<PlayerController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_player == null)
        {
            Debug.Log("tf");
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

        switch (_player.LivesRemaining)
        {
            case 0:
                _heart2.enabled = false;
                break;
            case 1:
                _heart1.enabled = false;
                break;
        }

    }
    public void ResetHearts()
    {
        StartCoroutine(nameof(ResetHeartsRoutine));
    }

    private IEnumerator ResetHeartsRoutine()
    {
        yield return new WaitForSeconds(2f);
        _heart1.enabled = true;
        _heart2.enabled = true;
    }
}

public static class PlayerManager
{
    public static PlayerController player1;
    public static PlayerController player2;

    public static Action<PlayerController> playerSpawned;
}
