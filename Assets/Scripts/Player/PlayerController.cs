using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class PlayerController : MonoBehaviour
{
  #region Entity variables
  private Rigidbody2D body;
  private CapsuleCollider2D playerCollider;
  SpriteRenderer spriteRenderer;
  public bool facingLeft = false;
  public bool shouldFlip = false;
  private float defaultGravityScale;
  public Vector2 Velocity => body.velocity;
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

  // [SerializeField] private float jumpVelocity = 7;
 
  #endregion

  #region Dash variables
  [Header("Dash variables")]
  [SerializeField] private bool canDash = true;
  [SerializeField] private float dashSpeed = 22f;
  [SerializeField] private float dashLength = 0.4f;

  public bool shouldDash = false;
  private bool hasDashed;
  [SerializeField] public bool isDashing;
  private float timeStartedDash;
  public Vector2 dashDirection;
  [SerializeField] private float diagonalDashThreshold = 0.3f;
  #endregion

  #region Jump variables
  [Header("Jump variables")]
  [SerializeField] private bool canJump = true;
  [SerializeField] private float jumpForce = 26f;
  [SerializeField] private float fallMultiplier = 6f;
  [SerializeField] private float jumpVelocityFalloff = 14f;
  [SerializeField] private float coyoteTime = 0.25f;
  [SerializeField] private bool enableDoubleJump = true;
  [SerializeField] public JumpState jumpState = JumpState.Grounded;
  [SerializeField] private float jumpBuffer = 0.1f;
  [SerializeField] private float lastJumpPressed;
  private bool HasBufferedJump => isGrounded && lastJumpPressed + jumpBuffer > Time.time;
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
  [SerializeField] private float grounderRadius = 0.15f;
  [SerializeField] private Vector2 grounderOffset = new Vector2(0.055f, -0.9f);
  [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.45f, 0.3f);
  [SerializeField] private float wallCheckDistance = 0.25f;
  private bool isAgainstWall, isAgainstWallLedge, pushingWall;
  public bool canGroundCheck = true;
  public bool isGrounded;
  public Collider2D footObject;
  #endregion

  #region Touch Ceiling variables
  [Header("Touch Ceiling variables")]
  [SerializeField] private float ceilingRadius = 0.15f;
  [SerializeField] private Vector2 ceilingOffset = new Vector2(0.055f, 0.9f);
  public bool isAgainstCeiling;
  public Collider2D headObject;
  #endregion

  #region Ledge Climb variables
  [Header("Ledge Climb variables")]
  [SerializeField] public bool canLedgeClimb = true;
  [SerializeField] private float ledgeCheckOffsetY = -0.3f;
  [SerializeField] private Vector2 ledgeClimbOffset1 = Vector2.zero;
  [SerializeField] private Vector2 ledgeClimbOffset2 = Vector2.zero;
  private Vector2 ledgePosBot, ledgePos1, ledgePos2;
  [SerializeField] private bool shouldLedgeClimb = false;

  #endregion

  #region Animation variables
  private PlayerRenderer playerRenderer;
  #endregion

  void Awake() {
    playerRenderer = GetComponent<PlayerRenderer>();
    inputManager = InputManager.Instance;
  }

  void OnEnable() {
    inputManager.OnJump += Jump;
    inputManager.OnMove += Move;
    inputManager.OnThrowWind += Dash;
    playerRenderer.OnChangeClimbState += UpdateClimbPosition;
    playerCollider = GetComponent<CapsuleCollider2D>();
  }

  void OnDisable() {
    inputManager.OnJump -= Jump;
    inputManager.OnMove -= Move;
    inputManager.OnThrowWind -= Dash;
    playerRenderer.OnChangeClimbState -= UpdateClimbPosition;
  }


  
  void Start() {
    body = GetComponent<Rigidbody2D>();
    playerCollider = GetComponent<CapsuleCollider2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    defaultGravityScale = body.gravityScale;
  }

  // Update is called once per frame
  void Update() {
    HandleGrounding();
    HandleCeiling();

    HandleWalking();

    HandleJumping();

    HandleDashing();

    HandleLedgeClimbing();
  }

  #region Grounding
  void HandleGrounding() {
    if (shouldLedgeClimb) return;
    if (!canGroundCheck) return;

    var grounded = Physics2D.OverlapCircle(transform.position + new Vector3(grounderOffset.x, grounderOffset.y), grounderRadius, groundMask);
    footObject = grounded;

    // if (grounded && grounded.CompareTag("TraversablePlatform") && Velocity.y != 0) return;

    if (!isGrounded && grounded) {
      isGrounded = true;
      hasJumped = false;
      hasDashed = false;
      if (jumpState != JumpState.Grounded) {
        jumpState = JumpState.Grounded;
      }
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

  void HandleCeiling() {
    var headButting = Physics2D.OverlapCircle(transform.position + new Vector3(ceilingOffset.x, ceilingOffset.y), ceilingRadius, groundMask);
    headObject = headButting;
    
    if (!isAgainstCeiling && headButting) {
      isAgainstCeiling = true;
    } else if (isGrounded && !headButting) {
      isAgainstCeiling = false;
    }
  }
  
  private void DrawGrounderGizmos() {
    Gizmos.color = Color.red;
    if (!playerCollider) return;

    float xPos = playerCollider.transform.root.position.x + (facingLeft ? -1 * grounderOffset.x : grounderOffset.x);
    if (isGrounded) {
      Gizmos.DrawSphere(transform.position + new Vector3(xPos, grounderOffset.y), grounderRadius);
    } else {
      Gizmos.DrawWireSphere(transform.position + new Vector3(xPos, grounderOffset.y), grounderRadius);
    }
    if (isAgainstCeiling) {
      Gizmos.DrawSphere(transform.position + new Vector3(xPos, ceilingOffset.y), ceilingRadius);
    } else {
      Gizmos.DrawWireSphere(transform.position + new Vector3(xPos, ceilingOffset.y), ceilingRadius);
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
    inputX = swipeDelta.x;
    inputY = swipeDelta.y;
  }

  void HandleWalking() {
    if (!canWalk) return;

    if (isDashing) return;
    // if (isDashing && (dashDirection == Vector2.left || dashDirection == Vector2.right)) return;
    
    if (shouldLedgeClimb) return;

    if ((inputX > 0.01f && facingLeft) || (inputX < -0.01f && !facingLeft)) {
      shouldFlip = true;
      if (!isGrounded) {
        Flip();
      }
    }

    body.velocity = new Vector2(inputX * movementVelocity, body.velocity.y);
  }

  public void Flip() {
    Vector3 currentScale = transform.localScale;
    currentScale.x *= -1;
    transform.localScale = currentScale;
    facingLeft = !facingLeft;
    shouldFlip = false;
  }
  #endregion

  #region Jumping

  public void Jump() {
    lastJumpPressed = Time.time;

    if (!canJump) return;

    if (isDashing && (dashDirection == Vector2.up)) return;

    if (inputY <= -0.9f) return;

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

    if (isGravityEnabled && body.velocity.y < 0 && !isGrounded) {
      jumpState = JumpState.Falling;
    }

    if (isGravityEnabled && !isGrounded && body.velocity.y < jumpVelocityFalloff || body.velocity.y > 0|| HasBufferedJump) {
      body.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;
    }
  }
  #endregion

  #region Dashing
  private void HandleDashing() {
    if (!canDash) return;

    if (isDashing && Time.time >= timeStartedDash + dashLength) {
      isDashing = false;
      // Clamp the velocity so they don't keep shooting off
      // body.velocity = new Vector2(body.velocity.x > dashSpeed ? dashSpeed : body.velocity.x, body.velocity.y > dashSpeed ? dashSpeed : body.velocity.y);
      body.gravityScale = defaultGravityScale;
      if (isGrounded) {
        hasDashed = false;
      }
    }
  }

  public void Dash(Direction windDirection, Vector2 swipeDelta) {
    if (!hasDashed && !isGrounded) {
      dashDirection = CalculateDashDirection(swipeDelta);

      isDashing = true;
      hasDashed = true;
      timeStartedDash = Time.time;
    }

    if (isDashing) {
      body.gravityScale = 0;
      Vector2 dashVelocity = dashDirection * dashSpeed;
      
      if (dashDirection == Vector2.up) {
        body.velocity = dashDirection * dashSpeed * 0.6f;
      } else {
        body.velocity = dashVelocity;
      }
    }
  }

  private Vector2 CalculateDashDirection(Vector2 swipeDelta) {
    Vector2 dashDirection = Vector2.zero;

    if (Mathf.Abs(swipeDelta.x) >= diagonalDashThreshold && Mathf.Abs(swipeDelta.y) >= diagonalDashThreshold) {
      float xVelocity = swipeDelta.x > 0 ? 1 : -1;
      float yVelocity = swipeDelta.y > 0 ? 1 : -1;

      dashDirection = new Vector2(xVelocity, yVelocity);
    } else if (Mathf.Abs(swipeDelta.x) >= Mathf.Abs(swipeDelta.y)) {
      if (swipeDelta.x > 0) {
        dashDirection = Vector2.right;
      } else {
        dashDirection = Vector2.left;
      }
    } else {
      if (swipeDelta.y > 0) {
        dashDirection = Vector2.up;
      } else {
        dashDirection = Vector2.down;
      }
    }

    return dashDirection;
  }
  #endregion

  #region Ledge Climbing
  public void HandleLedgeClimbing() {
    if (isGrounded) return;

    if (!canLedgeClimb) return;

    if (shouldLedgeClimb) return;

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
      body.gravityScale = 0;
    }

    if (shouldLedgeClimb) {
      body.velocity = Vector2.zero;
      transform.position = ledgePos1;
      StartCoroutine(LedgeClimb());
    }
  }

  void UpdateClimbPosition(int state) {
    float xMultiplier = facingLeft ? -1f : 1f;
    if (state == 3) {
      transform.position += new Vector3(0f * xMultiplier, .2f);
    } else if (state == 4) {
      // transform.position += new Vector3(.2f * xMultiplier, .1f);
      transform.position += new Vector3(0f * xMultiplier, .1f);
    } else if (state == 5) {
      // transform.position += new Vector3(.3f * xMultiplier, .15f);
      transform.position += new Vector3(.0f * xMultiplier, .15f);
    } else if (state == 6) {
      transform.position += new Vector3(.5f * xMultiplier, .15f);
    } else if (state == 7) {
      transform.position = ledgePos2;
    }
  }

  private IEnumerator LedgeClimb() {
    yield return new WaitForSeconds(8f/18f);
    FinishLedgeClimb();
  }

  public void FinishLedgeClimb() {
    body.gravityScale = defaultGravityScale;
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
    Grounded = 0,
    Jumping = 1,
    LedgeClimbing = 2,
    Falling = 3,
    DoubleJumping = 4
  }
}
