using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _collisionCollider;
    private Vector2 _moveDirection;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _canJump = true;
    [SerializeField] private bool _isGrounded = false;
    private float _lastFacedDirection;
    [SerializeField, Range(0f, 2f)] private float _attackCooldown;
    private float _remainingAttackCooldown;
    private bool _canAttack = true;

    private readonly int _maxHealth = 100;
    public int MaxHealth => _maxHealth;
    public float HealthPercentage => (float)CurrentHealth / _maxHealth;
    private readonly int _maxStamina = 100;
    public int MaxStamina => _maxStamina;
    public float StaminaPercentage => (float)CurrentStamina / _maxStamina;
    [field: SerializeField] public int CurrentHealth { get; private set; }
    [field: SerializeField] public int CurrentStamina { get; private set; }

    //public int Test
    //{
    //    get
    //    {
    //        return _currentHealth;
    //    }
    //    set
    //    {
    //        if(value < 0)
    //            _currentHealth = 0;

    //    }
    //}

    public float moveSpeed = 1f; //PUT THIS IN SCRIPTABLESTATS
    public float jumpForce = 1f; //PUT THIS IN SCRIPTABLESTATS


    #region INPUTS
    public PlayerInputActions playerControls;
    public InputAction move;
    public InputAction attack;
    public InputAction jump;
    public InputAction dash;
    #endregion

    public GameObject rightHitbox;
    public GameObject leftHitbox;

    public bool isPlayer1 = true;

    public UnityEvent<PlayerController> PlayerTookDamage;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collisionCollider = GetComponent<CapsuleCollider2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        playerControls = new PlayerInputActions();

        CurrentHealth = _maxHealth;
        CurrentStamina = _maxStamina;
        _lastFacedDirection = isPlayer1 ? 1 : -1;
    }

    private void OnEnable()
    {
        if (isPlayer1)
        {
            move = playerControls.Player1.Move;
            attack = playerControls.Player1.Attack;
            jump = playerControls.Player1.Jump;
            dash = playerControls.Player1.Dash;
            PlayerManager.player1 = this;
        }
        else
        {
            move = playerControls.Player2.Move;
            attack = playerControls.Player2.Attack;
            jump = playerControls.Player2.Jump;
            dash = playerControls.Player2.Dash;
            PlayerManager.player2 = this;
        }
        move.Enable();

        attack.Enable();
        attack.performed += Attack;

        jump.Enable();
        jump.performed += Jump;

        dash.Enable();
        dash.performed += Dash;
    }

    private void OnDisable()
    {
        move?.Disable();
        attack?.Disable();
        if (isPlayer1)
        {
            PlayerManager.player1 = null;
        }
        else
        {
            PlayerManager.player2 = null;
        }
    }


    // Update is called once per frame
    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandleDirection();
        ClampYVelocity();

        _animator.SetFloat("yVelocity", _rigidbody.velocity.y);
    }

    private void HandleDirection()
    {
        _rigidbody.velocity = new Vector2(_moveDirection.x * moveSpeed, _rigidbody.velocity.y);
        if (_rigidbody.velocity.sqrMagnitude > 0)
        {
            if (_rigidbody.velocity.x < 0)
            {
                _spriteRenderer.flipX = true;
                _lastFacedDirection = -1;
            }
            else
            {
                _spriteRenderer.flipX = false;
                _lastFacedDirection = 1;
            }
            _animator.SetBool("isMoving", true);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            _animator.SetBool("isGrounded", _isGrounded);
            _canJump = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && (_rigidbody.velocity.y > 0f || _rigidbody.velocity.y < -0f))
        {
            _isGrounded = false;
            _animator.SetBool("isGrounded", _isGrounded);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            Debug.Log("hit someone");
            TakeDamage(10);
        }
    }

    private void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        //PlayerTookDamage.Invoke(this);
    }

    private void RecoverStamina()
    {

    }

    private void DrainStamina(int stamina)
    {
        CurrentStamina -= stamina;
    }

    private void HandleInput()
    {
        _moveDirection = move.ReadValue<Vector2>();
    }

    private void Attack(InputAction.CallbackContext callbackContext)
    {
        if (!_canAttack) return;

        _canAttack = false;
        _animator.SetTrigger("attack");
        StartCoroutine(nameof(AttackCooldownRoutine));
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    public void ActivateAttackHitbox()
    {
        if (_lastFacedDirection == -1)
        {
            leftHitbox.SetActive(true);
        }
        else
        {
            rightHitbox.SetActive(true);
        }
    }

    public void DeactivateAttackHitbox()
    {
        if (_lastFacedDirection == -1)
        {
            leftHitbox.SetActive(false);
        }
        else
        {
            rightHitbox.SetActive(false);
        }
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        if (!_canJump) return;

        _animator.SetTrigger("jump");
        _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    private void ClampYVelocity()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Clamp(_rigidbody.velocity.y, -19.62f, jumpForce));
    }

    private void Dash(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Dashing");
    }
}
