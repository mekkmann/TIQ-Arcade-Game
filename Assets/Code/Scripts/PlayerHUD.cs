using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;

    [SerializeField] private Image _heart1;
    [SerializeField] private Image _heart2;

    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerController.TakeDamageEvent += (damage) => UpdateHealthBar(damage);
        _playerController.PlayerDiedEvent += (livesRemaining) => UpdateLifeBar(livesRemaining);
        _healthSlider.value = _playerController.CurrentHealth;
        UpdateStaminaBar(_playerController.CurrentStamina);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStaminaBar(_playerController.CurrentStamina);
    }
    private void UpdateLifeBar(int livesRemaining)
    {
        switch (livesRemaining)
        {
            case 1:
                _heart1.enabled = false;
                break;
            case 0:
                _heart2.enabled = false;
                break;
            default:
                break;
        }
    }
    private void UpdateHealthBar(int damage)
    {
        _healthSlider.value -= damage;
        switch (_healthSlider.value)
        {
            case < 0:
                _healthSlider.value = 0;
                break;
            case > 100:
                _healthSlider.value = 100;
                break;
            default:
                break;
        }
    }

    private void UpdateStaminaBar(int value)
    {
        _staminaSlider.value = value;
        switch (_staminaSlider.value)
        {
            case < 0:
                _staminaSlider.value = 0;
                break;
            case > 100:
                _staminaSlider.value = 100;
                break;
            default:
                break;
        }
    }
}
