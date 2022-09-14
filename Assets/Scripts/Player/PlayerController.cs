using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  #region Entity variables
  private Rigidbody2D body;
  private CapsuleCollider2D collider;
  SpriteRenderer spriteRenderer;
  private bool facingLeft = false;
  private float defaultGravityScale;
  #endregion
  
  #region Input variables
  private InputManager inputManager;
  public Direction actionDirection;
  public Direction lastActionDirection = Direction.Stationary;
  #endregion

  #region Movement variables
  [Header("Movement variables")]
  [SerializeField] private bool canWalk = true;
  [SerializeField] private float movementVelocity = 7f;

  private float joyStickMaxTravel = 150f;
  // [SerializeField] private float jumpVelocity = 7;
  #endregion

  #region Dash variables
  [Header("Dash variables")]
  [SerializeField] private bool canDash = true;
  [SerializeField] private float dashSpeed = 22f;
  [SerializeField] private float dashLength = 0.4f;

  public bool shouldDash = false;
  private bool hasDashed;
  [SerializeField] private bool isDashing;
  private float timeStartedDash;
  public Vector2 dashDirection;
  #endregion

  #region Jump variables
  [Header("Jump variables")]
  [SerializeField] private bool canJump = true;
  [SerializeField] private float jumpForce = 26f;
  [SerializeField] private float fallMultiplier = 6f;
  [SerializeField] private float jumpVelocityFalloff = 14f;
  [SerializeField] private float coyoteTime = 0.25f;
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
  [SerializeField] private float grounderRadius = 0.5f;
  [SerializeField] private Vector2 grounderOffset = new Vector2(0.055f, -0.9f);
  [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.45f, 0.6f);
  [SerializeField] private float wallCheckDistance = 0.25f;
  private bool isAgainstWall, isAgainstWallLedge, pushingWall;
  public bool isGrounded;
  #endregion

  #region Ledge Climb variables
  [Header("Ledge Climb variables")]
  [SerializeField] private bool canLedgeClimb = true;
  [SerializeField] private float ledgeCheckOffsetY = -0.15f;
  [SerializeField] private Vector2 ledgeClimbOffset1 = Vector2.zero;
  [SerializeField] private Vector2 ledgeClimbOffset2 = Vector2.zero;
  private Vector2 ledgePosBot, ledgePos1, ledgePos2;
  [SerializeField] private bool shouldLedgeClimb = false;

  #endregion

  #region Animation variables
    private Animator animator;
  #endregion

  void Awake() {
    inputManager = InputManager.Instance;
  }

  void OnEnable() {
    inputManager.OnJump += Jump;
    inputManager.OnMove += Move;
    inputManager.OnThrowWind += ThrowWind;
  }
  
  void Start() {
    body = GetComponent<Rigidbody2D>();
    collider = GetComponent<CapsuleCollider2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    defaultGravityScale = body.gravityScale;
    animator = GetComponent<Animator>();
  }

  // Update is called once per frame
  void Update() {
    HandleGrounding();

    HandleWalking();

    HandleJumping();

    HandleDashing();

    HandleLedgeClimbing();
  }

  #region Grounding
  void HandleGrounding() {
    var grounded = Physics2D.OverlapCircle(transform.position + new Vector3(grounderOffset.x, grounderOffset.y), grounderRadius, groundMask);

    if (!isGrounded && grounded) {
      isGrounded = true;
      hasJumped = false;
      hasDashed = false;
      if (jumpState != JumpState.Grounded) {
        jumpState = JumpState.Grounded;
        animator.SetTrigger("Landed");
      }
      animator.SetBool("Jumping", false);
    } else if (isGrounded && !grounded) {
      isGrounded = false;
      timeLeftGrounded = Time.time;
    }



    float xOffset = facingLeft ? -wallCheckOffset.x : wallCheckOffset.x;
    Vector2 checkDirection = facingLeft ? Vector2.left : Vector2.right;

    isAgainstWall = Physics2D.Raycast(transform.position + new Vector3(xOffset, wallCheckOffset.y), checkDirection, wallCheckDistance, groundMask);

    isAgainstWallLedge = Physics2D.Raycast(transform.position + new Vector3(xOffset, ledgeCheckOffsetY), checkDirection, wallCheckDistance, groundMask);

    pushingWall = (isAgainstWall || isAgainstWallLedge) && Mathf.Abs(inputX) > 0f;
  }
  
  private void DrawGrounderGizmos() {
    Gizmos.color = Color.red;
    float xPos = collider.transform.root.position.x + (facingLeft ? -1 * grounderOffset.x : grounderOffset.x);
    if (isGrounded) {
      Gizmos.DrawSphere(transform.position + new Vector3(xPos, grounderOffset.y), grounderRadius);
    } else {
      Gizmos.DrawWireSphere(transform.position + new Vector3(xPos, grounderOffset.y), grounderRadius);
    }
  }
  private void DrawWallCheckGizmos() {
    float xOffset = facingLeft ? -wallCheckOffset.x : wallCheckOffset.x;
    float xDistance = facingLeft ? -wallCheckDistance : wallCheckDistance;

    Gizmos.color = Color.magenta;
    // Wall check
    Gizmos.DrawLine(transform.position + new Vector3(xOffset, wallCheckOffset.y), transform.position + new Vector3(xOffset, wallCheckOffset.y) + new Vector3(xDistance, 0));

    Gizmos.color = Color.yellow;
    // Ledge check
    Gizmos.DrawLine(transform.position + new Vector3(xOffset, ledgeCheckOffsetY), transform.position + new Vector3(xOffset, ledgeCheckOffsetY) + new Vector3(xDistance, 0));
  }
  #endregion

  #region Walking
  public void Move(Vector2 swipeDelta) {
    float x = swipeDelta.x;

    if (swipeDelta == Vector2.zero) {
      inputX = 0;
      return;
    }

    inputX = x;
  }
  void HandleWalking() {
    if (!canWalk) return;
    
    animator.SetFloat("Speed", Mathf.Abs(inputX));

    if (isDashing && (dashDirection == Vector2.left || dashDirection == Vector2.right)) return;
    
    if (shouldLedgeClimb) return;

    if ((inputX > 0.01f && facingLeft) || (inputX < -0.01f && !facingLeft)) {
      Flip();
    }

    body.velocity = new Vector2(inputX * movementVelocity, body.velocity.y);
  }

  void Flip() {
    Vector3 currentScale = transform.localScale;
    currentScale.x *= -1;
    transform.localScale = currentScale;
    facingLeft = !facingLeft;
  }
  #endregion

  #region Jumping

  public void Jump() {
    if (!canJump) return;

    if (isDashing && (dashDirection == Vector2.up)) return;

    if (isGrounded || Time.time < timeLeftGrounded + coyoteTime || enableDoubleJump && !hasDoubleJumped) {
      if (!hasJumped || hasJumped && !hasDoubleJumped) {
        ExecuteJump(new Vector2(body.velocity.x, jumpForce), hasJumped);
      }
    }

    void ExecuteJump(Vector2 dir, bool doubleJump = false) {
      body.velocity = dir;
      // hasDoubleJumped = doubleJump;
      hasDoubleJumped = true;
      hasJumped = true;
      shouldJump = false;
      animator.SetBool("Jumping", true);

      if (doubleJump) {
        jumpState = JumpState.DoubleJumping;
      } else {
        jumpState = JumpState.Jumping;
      }
    }
  }
  public void HandleJumping() {
    if (!canJump) return;

    if (isDashing && (dashDirection == Vector2.up)) {
      return;
    }

    bool isGravityEnabled = body.gravityScale > 0;

    if (isGravityEnabled && body.velocity.y < 0 && !isGrounded && jumpState == JumpState.Jumping) {
      jumpState = JumpState.Falling;
      animator.SetTrigger("Falling");
    }

    if (isGravityEnabled && body.velocity.y < jumpVelocityFalloff || body.velocity.y > 0) {
      body.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;
    }
  }
  #endregion

  #region Dashing
  private void HandleDashing() {
    if (!canDash) return;

    if (isDashing && Time.time >= timeStartedDash + dashLength) {
      Debug.Log("Dash finished");
      isDashing = false;
      // Clamp the velocity so they don't keep shooting off
      // body.velocity = new Vector2(body.velocity.x > dashSpeed ? dashSpeed : body.velocity.x, body.velocity.y > dashSpeed ? dashSpeed : body.velocity.y);
      // body.gravityScale = defaultGravityScale;
      if (isGrounded) {
        hasDashed = false;
      }
    }
  }
  #endregion

  #region Throw Wind
  public void ThrowWind(Direction windDirection) {
    if (!hasDashed && !isGrounded) {
      switch (windDirection)
      {
        case Direction.Right:
          dashDirection = Vector2.right;
          break;
        case Direction.Left:
          dashDirection = Vector2.left;
          break;
        case Direction.Up:
          dashDirection = Vector2.up;
          break;
        case Direction.Down:
          dashDirection = Vector2.down;
          break;
        default:
          return;
      }

      isDashing = true;
      hasDashed = true;
      timeStartedDash = Time.time;
    }

    if (isDashing) {
      Vector2 dashVelocity = dashDirection * dashSpeed;

      float xVelocity = body.velocity.x + dashVelocity.x; 
      if ((dashDirection == Vector2.left || dashDirection == Vector2.right) && Mathf.Abs(xVelocity) > dashSpeed) {
        xVelocity = xVelocity > 0 ? dashSpeed : -dashSpeed;
      }

      float yVelocity = body.velocity.y + dashVelocity.y;
      if ((dashDirection == Vector2.up || dashDirection == Vector2.down) && Mathf.Abs(yVelocity) > dashSpeed) {
        yVelocity = yVelocity > 0 ? dashSpeed * 0.5f : -dashSpeed;
      }
      if (dashDirection == Vector2.up && yVelocity < dashSpeed) {
        yVelocity = dashSpeed * 0.6f;
      }

      body.velocity = new Vector2(xVelocity, yVelocity);
    }
  }
  #endregion

  #region Ledge Climbing
  public void HandleLedgeClimbing() {
    if (isGrounded) return;

    if (!canLedgeClimb && shouldLedgeClimb) return;

    if (pushingWall && !isAgainstWall) {
      if (facingLeft) {
        ledgePosBot = transform.position + new Vector3(-wallCheckOffset.x, wallCheckOffset.y);
        
        ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbOffset1.x, Mathf.Floor(ledgePosBot.y) + ledgeClimbOffset1.y);
        ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbOffset2.x, Mathf.Floor(ledgePosBot.y) + ledgeClimbOffset2.y);
      } else {
        ledgePosBot = transform.position + new Vector3(wallCheckOffset.x, wallCheckOffset.y);

        ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbOffset1.x, Mathf.Floor(ledgePosBot.y) + ledgeClimbOffset1.y);
        ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbOffset2.x, Mathf.Floor(ledgePosBot.y) + ledgeClimbOffset2.y);
      }

      shouldLedgeClimb = true;
      canDash = false;
      canJump = false;
      canWalk = false;
      jumpState = JumpState.LedgeClimbing;
    }

    if (shouldLedgeClimb) {
      transform.position = ledgePos1;
      StartCoroutine(LedgeClimb());
    }
  }

  private IEnumerator LedgeClimb() {
    yield return new WaitForSeconds(0.3f);
    FinishLedgeClimb();
  }

  public void FinishLedgeClimb() {
    transform.position = ledgePos2;
    shouldLedgeClimb = false;
    canDash = true;
    canJump = true;
    canWalk = true;
    jumpState = JumpState.Grounded;
  }

  private void DrawClimbingLedgeGizmos() {
    Gizmos.color = Color.cyan;
    Gizmos.DrawLine(ledgePos1, ledgePos2);
  }
  #endregion

  #region Collisions
  private void OnCollisionEnter2D(Collision2D collision) {}
  private void OnTriggerEnter2D(Collider2D other) {
    hasDashed = false;
    dashDirection = Vector2.zero;
  }
  #endregion

  private void OnDrawGizmos() {
    DrawGrounderGizmos();
    DrawWallCheckGizmos();
    DrawClimbingLedgeGizmos();
  }

  public bool IsFacingLeft() {
    return facingLeft;
  }

  public enum JumpState
  {
    Grounded,
    PrepareToJump,
    Jumping,
    DoubleJumping,
    InFlight,
    Falling,
    Landed,
    LedgeClimbing
  }
}
