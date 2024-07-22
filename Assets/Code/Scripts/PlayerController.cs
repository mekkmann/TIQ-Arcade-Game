using System;
using System.Collections;
using UnityEngine;
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

    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip[] _walkSFX;
    [SerializeField] AudioClip _jumpSFX;

    public VisualEffect _hitVFX;
    public VisualEffect _dashVFX;

    private readonly int _maxHealth = 100;
    public int MaxHealth => _maxHealth;
    public float HealthPercentage => (float)CurrentHealth / _maxHealth;
    private readonly int _maxStamina = 100;
    public int MaxStamina => _maxStamina;
    public float StaminaPercentage => (float)CurrentStamina / _maxStamina;
    [SerializeField] public int CurrentHealth { get; private set; }
    [SerializeField] public int CurrentStamina { get; private set; }

    private int _livesRemaining = 2;
    public int LivesRemaining => _livesRemaining;

    private PlayerController _otherPlayer;
    private string _playerToSearchFor;
    private float _currentMoveSpeed;
    public float moveSpeed = 1f;
    public float dashSpeed = 3f;
    public float jumpForce = 1f;


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

    public event Action<int> PlayerDiedEvent;
    public event Action<int> TakeDamageEvent;

    private float _attackDirection;

    #region COYOTE TIME
    private float _coyoteTime = 0.2f;
    private float _coyoteTimeCounter;
    #endregion

    #region JUMP BUFFERING
    private float _jumpBufferTime = 0.2f;
    private float _jumpBufferTimeCounter;
    #endregion

    #region BETTER JUMP
    private bool _jumpHeld = false;
    [Range(0, 5f)][SerializeField] private float fallLongMultiplier = 0.85f;
    [Range(0, 5f)][SerializeField] private float fallShortMultiplier = 1.55f;
    #endregion

    private void Awake()
    {
        _dashVFX.Stop();
        _hitVFX.Stop();
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

        _dashVFX.SetVector4("color", (Vector4)_spriteRenderer.color);
    }
    public void PlayWalkSound()
    {
        int randomIndex = UnityEngine.Random.Range(0, 3);
        _audioSource.PlayOneShot(_walkSFX[randomIndex], 0.7f);
    }
    public void PlayJumpSound()
    {
        _audioSource.PlayOneShot(_jumpSFX, 0.2f);
    }
    private void Start()
    {
        StartCoroutine(nameof(RecoverStaminaRoutine));
        TryGetOtherPlayer();
    }

    private void TryGetOtherPlayer()
    {
        if (_otherPlayer != null) return;

        _playerToSearchFor = isPlayer1 ? "Player2" : "Player1";
        try
        {
            _otherPlayer = GameObject.Find(_playerToSearchFor).GetComponent<PlayerController>();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        TryGetOtherPlayer();
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


    }

    private void FixedUpdate()
    {
        HandleDirection();
        ClampYVelocity();

        _animator.SetFloat("yVelocity", _rigidbody.velocity.y);

        //// jump
        //if (_canJump && _coyoteTimeCounter > 0f && _jumpBufferTimeCounter > 0f)
        //{
        //    _canJump = false;
        //    _coyoteTimeCounter = 0f;
        //    _jumpBufferTimeCounter = 0f;
        //    _animator.SetTrigger("jump");
        //    _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        //}

        // test jump
        if (_canJump && _coyoteTimeCounter > 0f && _jumpBufferTimeCounter > 0f)
        {
            _canJump = false;
            _coyoteTimeCounter = 0f;
            _jumpBufferTimeCounter = 0f;
            _isGrounded = false;
            _animator.SetTrigger("jump");
            _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        // long jump
        if (_jumpHeld && _rigidbody.velocity.y > 0)
        {
            _rigidbody.velocity += (fallLongMultiplier - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }
        // short jump
        else if (!_jumpHeld && _rigidbody.velocity.y > 0)
        {
            _rigidbody.velocity += (fallShortMultiplier - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }
    }

    private void HandleDirection()
    {
        _rigidbody.velocity = new Vector2(_moveDirection.x * _currentMoveSpeed, _rigidbody.velocity.y);
        if (_rigidbody.velocity.magnitude > 0)
        {
            if (_rigidbody.velocity.x < 0)
            {
                _spriteRenderer.flipX = true;
                _dashVFX.gameObject.transform.Rotate(Vector3.forward);

                _lastFacedDirection = -1;
            }
            else if (_rigidbody.velocity.x > 0)
            {
                _spriteRenderer.flipX = false;
                _dashVFX.gameObject.transform.Rotate(Vector3.back);
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
        damage = 90;
        CurrentHealth -= damage;
        TakeDamageEvent.Invoke(damage);
        if (CurrentHealth <= 0)
        {
            Death();
        }
        else
        {
            _animator.SetTrigger("hurt");
        }
    }

    public void Death()
    {
        _livesRemaining -= 1;
        _spriteRenderer.enabled = false;
        PlayerDiedEvent.Invoke(_livesRemaining);
        if (_livesRemaining > 0)
        {
            GameManager.Instance.StartStartRound();
        }
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
                CurrentStamina += 3;
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
        _dashVFX.SetBool("rotate", _lastFacedDirection == 1);
        _dashVFX.Play();
        _animator.SetBool("isDashing", true);
        _rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        _currentMoveSpeed = dashSpeed;
        DrainStamina(_staminaDashDrain);
        _hurtboxCollider.enabled = false;
        yield return new WaitForSeconds(0.2f);
        _hurtboxCollider.enabled = true;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _animator.SetBool("isDashing", false);
        _dashVFX.Stop();
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
        if (_otherPlayer.transform.position.x < transform.position.x)
        {
            _hitVFX.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        }
        else if (_otherPlayer.transform.position.x > transform.position.x)
        {
            _hitVFX.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
        }
        _hitVFX.Play();
    }
}
