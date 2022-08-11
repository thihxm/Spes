using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  // Start is called before the first frame update
  private Rigidbody2D body;
  private BoxCollider2D collider;

  SpriteRenderer spriteRenderer;

  public Vector2 velocity;
  
  public float dashDistance = 100f;
  private float lastActionTime = 0f;
  public Direction actionDirection;
  public Direction lastActionDirection = Direction.Stationary;

  private FrameInputs inputs;


  [Header("Movement variables")]
  [SerializeField] private bool isActive = true;
  [SerializeField] private float movementVelocity = 7f;
  // [SerializeField] private float jumpVelocity = 7;


  [Header("Dashing")]
  [SerializeField] private float dashingVelocity = 14f;
  [SerializeField] private float dashingTime = 0.5f;
  private Vector2 dashingDirection;
  private bool isDashing;
  private bool canDash;

  [Header("Jump variables")]
  [SerializeField] private float jumpForce = 11f;
  [SerializeField] private float fallMultiplier = 6f;
  [SerializeField] private float jumpVelocityFalloff = 6f;
  [SerializeField] private float coyoteTime = 0.15f;
  [SerializeField] private bool enableDoubleJump = true;
  [SerializeField] private JumpState jumpState = JumpState.Grounded;
  private float timeLeftGrounded = -10;
  private bool hasJumped;
  private bool hasDoubleJumped;

  public bool shouldJump = false;

  public float inputX = 0f;
  public float inputY = 0f;

  [Header("Touch Ground variables")]
  [SerializeField] private LayerMask groundMask;
  [SerializeField] private float grounderOffset = -1f, grounderRadius = 0.2f;
  [SerializeField] private float wallCheckOffset = 0.5f, wallCheckRadius = 0.05f;
  private bool isAgainstLeftWall, isAgainstRightWall, pushingLeftWall, pushingRightWall;
  public bool isGrounded;
  
  void Start() {
    body = GetComponent<Rigidbody2D>();
    collider = GetComponent<BoxCollider2D>();
    transform.position = new Vector3(0, 0, 0);
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  // Update is called once per frame
  void Update() {

    GatherInputs();

    HandleGrounding();

    HandleWalking();

    HandleJumping();

    // ComputeDash();
  }

  void GatherInputs() {
    // inputs.RawX = 
  }

  void HandleGrounding() {
    var grounded = Physics2D.OverlapCircle(transform.position + new Vector3(0, grounderOffset), grounderRadius, groundMask);

    if (!isGrounded && grounded) {
      isGrounded = true;
      hasJumped = false;
      jumpState = JumpState.Grounded;
    } else if (isGrounded && !grounded) {
      isGrounded = false;
      timeLeftGrounded = Time.time;
    }

    isAgainstLeftWall = Physics2D.OverlapCircle(transform.position + new Vector3(-wallCheckOffset, 0), wallCheckRadius, groundMask);
    isAgainstRightWall = Physics2D.OverlapCircle(transform.position + new Vector3(wallCheckOffset, 0), wallCheckRadius, groundMask);
    pushingLeftWall = isAgainstLeftWall && inputX < 0.01f;
    pushingRightWall = isAgainstRightWall && inputX > 0.01f;
  }

  void HandleWalking() {
    if (inputX > 0.01f) {
      spriteRenderer.flipX = false;
    } else if (inputX < -0.01f) {
      spriteRenderer.flipX = true;
    }

    body.velocity = new Vector2(inputX * movementVelocity, body.velocity.y);
  }

  // public void Jump() {
  //   body.velocity = new Vector2(body.velocity.x, 10f);
  //   // body.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
  // }

  public void HandleJumping() {
    if (isGrounded || Time.time < timeLeftGrounded + coyoteTime) {
      Jump();
    }

    void Jump() {}

    if (isDashing) {
      return;
    }

    //Verificar estado do input
    // if (inputY > 0.25f) {
    if (shouldJump) {
      if (isGrounded || Time.time < timeLeftGrounded + coyoteTime || enableDoubleJump && !hasDoubleJumped) {
        if (!hasJumped || hasJumped && !hasDoubleJumped) {
          ExecuteJump(new Vector2(body.velocity.x, jumpForce), hasJumped);
        }
      }
    }

    void ExecuteJump(Vector2 dir, bool doubleJump = false) {
      body.velocity = dir;
      hasDoubleJumped = doubleJump;
      hasJumped = true;
      shouldJump = false;
      if (doubleJump) {
        jumpState = JumpState.DoubleJumping;
      } else {
        jumpState = JumpState.Jumping;
      }
    }

    if (body.velocity.y < jumpVelocityFalloff || body.velocity.y > 0 && actionDirection == Direction.Up) {
      body.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;
    }
  }

  private struct FrameInputs {
    public float X, Y;
    public int RawX, RawY;
  }

  public enum JumpState
  {
    Grounded,
    PrepareToJump,
    Jumping,
    DoubleJumping,
    InFlight,
    Landed
  }

  public enum Direction {
    Left,
    Right,
    Up,
    Down,
    Tap,
    Stationary
  }
}
