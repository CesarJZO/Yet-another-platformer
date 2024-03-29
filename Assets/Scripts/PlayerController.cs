using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float speed;
    public float deadZone;
    public float dieForce;
    private float _horizontalInput;
    public bool isAlive = true;

    [Space]
    
    #region Components
    
    private PlayerMovement _movement;
    public GameManager gameManager;
    public Rigidbody2D rb;
    private BoxCollider2D _boxCollider;
    private CircleCollider2D _circleCollider;
    public PlayerInput playerInput;
    private InputAction _move;

    #endregion

    [Space]
    
    #region Animation

    private Animator _animator;
    private int _moveAnimId;
    private int _jumpAnimId;
    private int _groundAnimId;
    private int _isAliveAnimId;
    private int _dieAnimId;
    private int _vertAnimId;

    #endregion

    [Space]
        
    #region Audio

    private AudioSource _audioSource;
    public AudioClip coinAudio;
    public AudioClip hurtAudio;
    public AudioClip jumpAudio;

    #endregion

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _circleCollider = GetComponent<CircleCollider2D>();
        playerInput = GetComponent<PlayerInput>();
        _move = playerInput.actions[ActionName.Move.ToString()];

        _animator = GetComponent<Animator>();
        _moveAnimId = Animator.StringToHash("Move");
        _jumpAnimId = Animator.StringToHash("Jump");
        _groundAnimId = Animator.StringToHash("Grounded");
        _isAliveAnimId = Animator.StringToHash("Is Alive");
        _dieAnimId = Animator.StringToHash("Die");
        _vertAnimId = Animator.StringToHash("Vertical Velocity");
        
        _audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (gameManager.levelFinished || !isAlive) return;
        _horizontalInput = _move.ReadValue<float>();
        _movement.Move(_horizontalInput * speed);
    }

    private void LateUpdate()
    {
        _animator.SetBool(_moveAnimId, Mathf.Abs(rb.velocity.x) > deadZone);
        _animator.SetFloat(_vertAnimId, rb.velocity.y);
        _animator.SetBool(_groundAnimId, _movement.grounded);
        _animator.SetBool(_isAliveAnimId, isAlive);
    }

    public void OnJump()
    {
        Application.targetFrameRate = 45;
        if (gameManager.levelFinished || !isAlive) return;
        _audioSource.PlayOneShot(jumpAudio);
        _movement.Jump();
        _animator.SetTrigger(_jumpAnimId);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var otherObject = col.gameObject;
        if (col.CompareTag("Cherry"))
        {
            Destroy(otherObject);
            _audioSource.PlayOneShot(coinAudio);
        }

        if (col.CompareTag("Gem"))
        {
            _audioSource.PlayOneShot(coinAudio);
            Destroy(otherObject);
        }

        if (col.CompareTag("Level End"))
        {
            gameManager.FinishLevel();
        }

        if (col.CompareTag("Poisoned"))
        {
            Destroy(otherObject);
            Die();
        }

        if (col.CompareTag("Checkpoint"))
            gameManager.spawnPoint.position = col.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        var otherObject = col.gameObject;
        if (otherObject.CompareTag("Weak Point"))
        {
            var parent = col.transform.parent;
            parent.GetComponent<BoxCollider2D>().enabled = false;
            Destroy(parent.gameObject);
        }

        if (otherObject.CompareTag("Spikes") || 
            otherObject.CompareTag("Enemy") || 
            otherObject.CompareTag("Abyss"))
            Die();
    }

    public void Respawn()
    {
        isAlive = true;
        _circleCollider.enabled = _boxCollider.enabled = true;
    }

    public void Die()
    {
        isAlive = false;
        _audioSource.PlayOneShot(hurtAudio);
        _circleCollider.enabled = _boxCollider.enabled = false;
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * dieForce);
        _animator.SetTrigger(_dieAnimId);
    }
}
