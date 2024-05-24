using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private CapsuleCollider2D _collisionCollider;
    private Vector2 _moveDirection;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;

    public float moveSpeed = 1f; //PUT THIS IN SCRIPTABLESTATS
    public float jumpForce = 1f; //PUT THIS IN SCRIPTABLESTATS


    #region INPUTS
    public PlayerInputActions playerControls;
    public InputAction move;
    public InputAction attack;
    public InputAction jump;
    public InputAction dash;
    #endregion

    public bool isPlayer1 = true;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collisionCollider = GetComponent<CapsuleCollider2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        playerControls = new PlayerInputActions();

        _currentHealth = _maxHealth;
    }

    private void OnEnable()
    {
        if (isPlayer1)
        {
            move = playerControls.Player1.Move;
            attack = playerControls.Player1.Attack;
            jump = playerControls.Player1.Jump;
            dash = playerControls.Player1.Dash;
        }
        else
        {
            move = playerControls.Player2.Move;
            attack = playerControls.Player2.Attack;
            jump = playerControls.Player2.Jump;
            dash = playerControls.Player2.Dash;
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
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

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
    }

    private void HandleDirection()
    {
        _rb.velocity = new Vector2(_moveDirection.x * moveSpeed, _rb.velocity.y);
        if (_rb.velocity.sqrMagnitude > 0)
        {
            if (_rb.velocity.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
            else
            {
                _spriteRenderer.flipX = false;
            }
            _animator.SetBool("isMoving", true);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
    }

    private void HandleInput()
    {
        _moveDirection = move.ReadValue<Vector2>();
    }

    private void Attack(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Attacking");
        _animator.SetTrigger("attack");
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Jumping");
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    private void ClampYVelocity()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Clamp(_rb.velocity.y, -19.62f, jumpForce));
    }

    private void Dash(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Dashing");
    }
}
