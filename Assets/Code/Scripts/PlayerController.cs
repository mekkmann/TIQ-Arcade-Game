using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _hurtboxCollider;
    private BoxCollider2D _collisionCollider;
    private Vector2 _moveDirection;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _canJump = true;
    [SerializeField] private bool _isGrounded = false;
    private float _lastFacedDirection;
    [SerializeField, Range(0f, 2f)] private float _attackCooldown;
    private bool _canAttack = true;
    [SerializeField] private readonly int _staminaAttackDrain = 20;
    [SerializeField] private readonly int _staminaDashDrain = 15;

    public VisualEffect _hitVFX;

    private readonly int _maxHealth = 100;
    public int MaxHealth => _maxHealth;
    public float HealthPercentage => (float)CurrentHealth / _maxHealth;
    private readonly int _maxStamina = 100;
    public int MaxStamina => _maxStamina;
    public float StaminaPercentage => (float)CurrentStamina / _maxStamina;
    [field: SerializeField] public int CurrentHealth { get; private set; }
    [field: SerializeField] public int CurrentStamina { get; private set; }

    private int _livesRemaining = 2;
    public int LivesRemaining => _livesRemaining;

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

    private float _currentMoveSpeed;
    public float moveSpeed = 1f; //PUT THIS IN SCRIPTABLESTATS
    public float dashSpeed = 4f; //PUT THIS IN SCRIPTABLESTATS
    public float jumpForce = 1f; //PUT THIS IN SCRIPTABLESTATS


    #region INPUTS
    private PlayerInputActions playerControls;
    private InputAction move;
    private InputAction attack;
    private InputAction jump;
    private InputAction dash;
    #endregion

    public GameObject rightHitbox;
    public GameObject leftHitbox;

    [SerializeField] private bool isPlayer1 = true;

    public UnityEvent<PlayerController> PlayerDied;

    private float _attackDirection;

    #region COYOTE TIME
    private float _coyoteTime = 0.2f;
    private float _coyoteTimeCounter;
    #endregion

    #region JUMP BUFFERING
    private float _jumpBufferTime = 0.2f;
    private float _jumpBufferTimeCounter;
    #endregion
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _hurtboxCollider = GetComponent<CapsuleCollider2D>();
        _collisionCollider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        playerControls = new PlayerInputActions();

        CurrentHealth = _maxHealth;
        CurrentStamina = _maxStamina;
        _lastFacedDirection = isPlayer1 ? 1 : -1;
        _currentMoveSpeed = moveSpeed;
    }

    private void Start()
    {
        StartCoroutine(nameof(RecoverStaminaRoutine));
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
        if (_isGrounded)
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        _jumpBufferTimeCounter -= Time.deltaTime;

        if (_canJump && _coyoteTimeCounter > 0f && _jumpBufferTimeCounter > 0f)
        {
            _canJump = false;
            _coyoteTimeCounter = 0f;
            _jumpBufferTimeCounter = 0f;
            _animator.SetTrigger("jump");
            HandleDirection();
            _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void FixedUpdate()
    {
        HandleDirection();
        ClampYVelocity();



        _animator.SetFloat("yVelocity", _rigidbody.velocity.y);
    }

    private void HandleDirection()
    {
        _rigidbody.velocity = new Vector2(_moveDirection.x * _currentMoveSpeed, _rigidbody.velocity.y);
        if (_rigidbody.velocity.magnitude > 0)
        {
            if (_rigidbody.velocity.x < 0)
            {
                _spriteRenderer.flipX = true;
                _lastFacedDirection = -1;
            }
            else if (_rigidbody.velocity.x > 0)
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
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            _isGrounded = true;
            _animator.SetBool("isGrounded", _isGrounded);
            _canJump = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform")) && (_rigidbody.velocity.y > 0f || _rigidbody.velocity.y < -0f))
        {
            _isGrounded = false;
            _animator.SetBool("isGrounded", _isGrounded);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            TakeDamage(10);
        }
    }

    private void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Death();
        }
        else
        {
            _animator.SetTrigger("hurt");
        }
    }

    private void Death()
    {
        _livesRemaining -= 1;
        _spriteRenderer.enabled = false;
        PlayerDied.Invoke(this);
    }

    public void RoundReset()
    {
        DeactivateControls();
        StopCoroutine(nameof(RecoverStaminaRoutine));
        StartCoroutine(nameof(RoundResetRoutine));
    }
    private IEnumerator RoundResetRoutine()
    {
        yield return new WaitForSeconds(2f);
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        _spriteRenderer.enabled = true;
        ActivateControls();
        StartCoroutine(nameof(RecoverStaminaRoutine));
    }

    public void GameReset()
    {
        DeactivateControls();
        StopCoroutine(nameof(RecoverStaminaRoutine));
        _spriteRenderer.enabled = false;
        StartCoroutine(nameof(GameResetRoutine));
    }
    private IEnumerator GameResetRoutine()
    {
        yield return new WaitForSeconds(2f);
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        _spriteRenderer.enabled = true;
        ResetLives();
        ResetHitboxes();
        ActivateControls();
        StartCoroutine(nameof(RecoverStaminaRoutine));
    }

    public void ResetHitboxes()
    {
        rightHitbox.SetActive(false);
        leftHitbox.SetActive(false);
    }

    private void ResetLives()
    {
        _livesRemaining = 2;
    }
    private IEnumerator RecoverStaminaRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (CurrentStamina >= _maxStamina)
            {
                CurrentStamina = _maxStamina;
            }
            else
            {
                CurrentStamina += 1;
            }
        }
    }

    private void DrainStamina(int stamina)
    {
        CurrentStamina -= stamina;
        if (CurrentStamina <= 0)
        {
            CurrentStamina = 0;

        }
    }

    private void HandleInput()
    {
        _moveDirection = move.ReadValue<Vector2>();
    }

    private void Attack(InputAction.CallbackContext callbackContext)
    {
        if (!_canAttack || CurrentStamina < _staminaAttackDrain) return;

        _canAttack = false;
        _animator.SetTrigger("attack");
        DrainStamina(_staminaAttackDrain);
        StartCoroutine(nameof(AttackCooldownRoutine));
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    public void ActivateAttackHitbox()
    {
        _attackDirection = _lastFacedDirection;
        if (_attackDirection == -1)
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
        if (_attackDirection == -1)
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
        _jumpBufferTimeCounter = _jumpBufferTime;
    }
    private void ClampYVelocity()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Clamp(_rigidbody.velocity.y, -19.62f, jumpForce));
    }

    private void Dash(InputAction.CallbackContext callbackContext)
    {
        if (CurrentStamina < _staminaDashDrain) return;
        StartCoroutine(nameof(DashRoutine));
    }

    private IEnumerator DashRoutine()
    {
        _rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
        _currentMoveSpeed = dashSpeed;
        DrainStamina(_staminaDashDrain);
        _hurtboxCollider.enabled = false;
        yield return new WaitForSeconds(0.125f);
        _rigidbody.constraints = RigidbodyConstraints2D.None;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _hurtboxCollider.enabled = true;
        _currentMoveSpeed = moveSpeed;
    }

    public void SpawnAt(Transform transform)
    {
        this.transform.position = transform.position;
        _spriteRenderer.flipX = !isPlayer1;
    }

    public void ActivateControls()
    {
        playerControls.Enable();
    }
    private void DeactivateControls()
    {
        playerControls.Disable();
    }

    public void PlayHitVFX()
    {
        _hitVFX.Play();
    }
}
