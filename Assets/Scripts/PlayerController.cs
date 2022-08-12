using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  #region Entity variables
  private Rigidbody2D body;
  private BoxCollider2D collider;
  SpriteRenderer spriteRenderer;
  private bool facingLeft = false;
  private float defaultGravityScale;
  #endregion
  
  #region Input variables
  public Direction actionDirection;
  public Direction lastActionDirection = Direction.Stationary;
  private FrameInputs inputs;
  #endregion

  #region Movement variables
  [Header("Movement variables")]
  [SerializeField] private bool isActive = true;
  [SerializeField] private float movementVelocity = 7f;
  // [SerializeField] private float jumpVelocity = 7;
  #endregion

  #region Dash variables
  [Header("Dash variables")]
  [SerializeField] private float dashSpeed = 15f;
  [SerializeField] private float dashLength = 0.4f;

  public bool shouldDash = false;
  private bool hasDashed;
  private bool isDashing;
  private float timeStartedDash;
  public Vector2 dashDirection;
  #endregion

  #region Jump variables
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
  #endregion

  #region Touch Ground variables
  [Header("Touch Ground variables")]
  [SerializeField] private LayerMask groundMask;
  [SerializeField] private float grounderOffset = -1f, grounderRadius = 0.2f;
  [SerializeField] private float wallCheckOffset = 0.5f, wallCheckRadius = 0.05f;
  private bool isAgainstLeftWall, isAgainstRightWall, pushingLeftWall, pushingRightWall;
  public bool isGrounded;
  #endregion
  
  void Start() {
    body = GetComponent<Rigidbody2D>();
    collider = GetComponent<BoxCollider2D>();
    transform.position = new Vector3(0, 0, 0);
    spriteRenderer = GetComponent<SpriteRenderer>();
    defaultGravityScale = body.gravityScale;
  }

  // Update is called once per frame
  void Update() {

    GatherInputs();

    HandleGrounding();

    HandleWalking();

    HandleJumping();

    HandleDashing();
  }

  void GatherInputs() {
    // inputs.RawX = 
  }

  #region Grounding
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
  #endregion

  #region Walking
  void HandleWalking() {
    if (isDashing) return;

    if (inputX > 0.01f) {
      facingLeft = false;
    } else if (inputX < -0.01f) {
      facingLeft = true;
    }
    spriteRenderer.flipX = facingLeft;

    body.velocity = new Vector2(inputX * movementVelocity, body.velocity.y);
  }
  #endregion

  #region Jumping
  public void HandleJumping() {
    if (isDashing) {
      return;
    }

    if (isGrounded || Time.time < timeLeftGrounded + coyoteTime) {
      Jump();
    } else if (!isGrounded && !enableDoubleJump) {
      shouldJump = false;
    }

    void Jump() {}

    if (shouldJump) {
      if (isGrounded || Time.time < timeLeftGrounded + coyoteTime || enableDoubleJump && !hasDoubleJumped) {
        if (!hasJumped || hasJumped && !hasDoubleJumped) {
          ExecuteJump(new Vector2(body.velocity.x, jumpForce), hasJumped);
        }
      }
    }

    void ExecuteJump(Vector2 dir, bool doubleJump = false) {
      body.velocity = dir;
      // hasDoubleJumped = doubleJump;
      hasDoubleJumped = true;
      hasJumped = true;
      shouldJump = false;
      if (doubleJump) {
        jumpState = JumpState.DoubleJumping;
      } else {
        jumpState = JumpState.Jumping;
      }
    }

    bool isGravityEnabled = body.gravityScale > 0;
    if (isGravityEnabled && body.velocity.y < jumpVelocityFalloff || body.velocity.y > 0 && actionDirection == Direction.Tap) {
      body.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;
    }
  }
  #endregion

  #region Dashing
  private void HandleDashing() {
    if (shouldDash && !hasDashed && !isGrounded) {
      // dashDirection = new Vector2(inputs.RawX, inputs.RawY).normalized;
      // if (dashDirection == Vector2.zero) {
      //   dashDirection = facingLeft ? Vector2.left : Vector2.right;
      // }
      isDashing = true;
      hasDashed = true;
      timeStartedDash = Time.time;
      body.gravityScale = 0;
      shouldDash = false;
    }

    if (isDashing) {
      Debug.Log("Dashing direction: " + dashDirection);
      body.velocity = dashDirection * dashSpeed;

      if (Time.time >= timeStartedDash + dashLength) {
        isDashing = false;
        // Clamp the velocity so they don't keep shooting off
        body.velocity = new Vector2(body.velocity.x, body.velocity.y > 3 ? 3 : body.velocity.y);
        body.gravityScale = defaultGravityScale;
        if (isGrounded) {
          hasDashed = false;
        }
      }
    }
  }
  #endregion

  #region Collisions
  private void OnCollisionEnter2D(Collision2D collision) {}
  private void OnTriggerEnter2D(Collider2D other) {
    hasDashed = false;
    dashDirection = Vector2.zero;
  }
  #endregion

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
