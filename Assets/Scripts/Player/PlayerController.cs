using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Player
{
  public class PlayerController : MonoBehaviour, IPlayerController
  {
    // Public for external hooks
    public Vector3 Velocity { get; private set; }
    public FrameInput MovementInput { get; private set; }
    public bool JumpingThisFrame { get; private set; }
    public bool LandingThisFrame { get; private set; }
    public Vector3 RawMovement { get; private set; }
    public bool Grounded => collisionDown;
    public JumpState JumpState => jumpState;

    private Vector3 lastPosition;
    private float currentHorizontalSpeed, currentVerticalSpeed;

    #region Animation variables
    private PlayerRenderer playerRenderer;
    #endregion

    // This is horrible, but for some reason colliders are not fully established when update starts...
    private bool active;
    void Awake() => Invoke(nameof(Activate), 0.5f);
    void Activate() => active = true;
    void OnEnable()
    {
      playerRenderer = GetComponent<PlayerRenderer>();
      inputManager = InputManager.Instance;
      inputManager.OnMove += HandleMoveInput;
      inputManager.OnJump += HandleJumpInput;
      playerRenderer.OnChangeClimbState += UpdateClimbPosition;
    }

    void OnDisable()
    {
      inputManager.OnMove -= HandleMoveInput;
      inputManager.OnJump -= HandleJumpInput;
      playerRenderer.OnChangeClimbState -= UpdateClimbPosition;
    }

    private void Update()
    {
      if (!active) return;
      // Calculate velocity
      Velocity = (transform.position - lastPosition) / Time.deltaTime;
      lastPosition = transform.position;

      // GatherInput();
      RunCollisionChecks();
      RunWallCollisionChecks();

      CalculateWalk(); // Horizontal movement
      CalculateVelocityDirection(); // Vertical movement
      CalculateJumpApex(); // Affects fall speed, so calculate before gravity
      CalculateGravity(); // Vertical movement
      CalculateJump(); // Possibly overrides vertical
      CalculateLedgeClimb();

      MoveCharacter(); // Actually perform the axis movement
    }


    #region Gather Input
    private InputManager inputManager;
    private bool jumpPressed = false;

    public void HandleMoveInput(Vector2 swipeDelta)
    {
      MovementInput = new FrameInput
      {
        X = swipeDelta.x,
        Y = swipeDelta.y
      };
    }

    private void HandleJumpInput()
    {
      jumpPressed = true;
      lastJumpPressed = Time.time;
    }

    #endregion

    #region Collisions

    [Header("COLLISION")][SerializeField] private Bounds characterBounds;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int detectorCount = 3;
    [SerializeField] private float detectionRayLength = 0.1f;
    [SerializeField][Range(0.1f, 0.3f)] private float rayBuffer = 0.1f; // Prevents side detectors hitting the ground

    private RayRange raysUp, raysRight, raysDown, raysLeft;
    private bool collisionUp, collisionRight, collisionDown, collisionLeft;

    private float timeLeftGrounded;

    #region Wall Check
    [Header("WALL CHECK")]
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.45f, 0.3f);
    [SerializeField] private float wallCheckDistance = 0.25f;
    private bool isAgainstWall, isAgainstWallLedge, pushingWall;
    [SerializeField] public bool canLedgeClimb = true;
    [SerializeField] private float ledgeCheckOffsetY = -0.3f;
    [SerializeField] private Vector2 ledgeClimbOffset1 = Vector2.zero;
    [SerializeField] private Vector2 ledgeClimbOffset2 = Vector2.zero;
    private Vector2 ledgePositionBottom, ledgeStartPosition, ledgeEndPosition;
    [SerializeField] private bool isLedgeClimbing = false;

    #endregion

    void RunWallCollisionChecks()
    {
      if (isLedgeClimbing) return;

      float xOffset = facingRight ? wallCheckOffset.x : -wallCheckOffset.x;
      Vector2 checkDirection = facingRight ? Vector2.right : Vector2.left;

      isAgainstWall = Physics2D.Raycast(transform.position + new Vector3(xOffset, wallCheckOffset.y), checkDirection, wallCheckDistance, groundLayer);

      isAgainstWallLedge = Physics2D.Raycast(transform.position + new Vector3(xOffset, ledgeCheckOffsetY), checkDirection, wallCheckDistance, groundLayer);

      pushingWall = (isAgainstWall || isAgainstWallLedge) && Mathf.Abs(MovementInput.X) > 0f;
    }

    // We use these raycast checks for pre-collision information
    private void RunCollisionChecks()
    {
      // Generate ray ranges. 
      CalculateRayRanged();

      // Ground
      LandingThisFrame = false;
      var groundedCheck = RunDetection(raysDown);
      if (collisionDown && !groundedCheck) timeLeftGrounded = Time.time; // Only trigger when first leaving
      else if (!collisionDown && groundedCheck)
      {
        coyoteUsable = true; // Only trigger when first touching
        LandingThisFrame = true;
      }

      collisionDown = groundedCheck;

      // The rest
      collisionUp = RunDetection(raysUp);
      collisionLeft = RunDetection(raysLeft);
      collisionRight = RunDetection(raysRight);

      bool RunDetection(RayRange range)
      {
        return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, detectionRayLength, groundLayer));
      }
    }

    private void CalculateRayRanged()
    {
      // This is crying out for some kind of refactor. 
      var b = new Bounds(transform.position, characterBounds.size);

      raysDown = new RayRange(b.min.x + rayBuffer, b.min.y, b.max.x - rayBuffer, b.min.y, Vector2.down);
      raysUp = new RayRange(b.min.x + rayBuffer, b.max.y, b.max.x - rayBuffer, b.max.y, Vector2.up);
      raysLeft = new RayRange(b.min.x, b.min.y + rayBuffer, b.min.x, b.max.y - rayBuffer, Vector2.left);
      raysRight = new RayRange(b.max.x, b.min.y + rayBuffer, b.max.x, b.max.y - rayBuffer, Vector2.right);
    }


    private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
    {
      for (var i = 0; i < detectorCount; i++)
      {
        var t = (float)i / (detectorCount - 1);
        yield return Vector2.Lerp(range.Start, range.End, t);
      }
    }

    private void OnDrawGizmos()
    {
      DrawLedgeClimbingGizmos();
      DrawWallCheckGizmos();

      // Bounds
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireCube(transform.position + characterBounds.center, characterBounds.size);

      // Rays
      if (!Application.isPlaying)
      {
        CalculateRayRanged();
        Gizmos.color = Color.blue;
        foreach (var range in new List<RayRange> { raysUp, raysRight, raysDown, raysLeft })
        {
          foreach (var point in EvaluateRayPositions(range))
          {
            Gizmos.DrawRay(point, range.Dir * detectionRayLength);
          }
        }
      }

      if (!Application.isPlaying) return;

      // Draw the future position. Handy for visualizing gravity
      Gizmos.color = Color.red;
      var move = new Vector3(currentHorizontalSpeed, currentVerticalSpeed) * Time.deltaTime;
      Gizmos.DrawWireCube(transform.position + move, characterBounds.size);
    }

    #endregion


    #region Walk

    [Header("WALKING")][SerializeField] private float acceleration = 90;
    [SerializeField] private float moveClamp = 13;
    [SerializeField] private float deAcceleration = 60f;
    [SerializeField] private float apexBonus = 2;
    private bool enableWalk = true;

    private void CalculateWalk()
    {
      if (!enableWalk) return;

      if (MovementInput.X != 0)
      {
        // Set horizontal move speed
        currentHorizontalSpeed += MovementInput.X * acceleration * Time.deltaTime;

        // clamped by max frame movement
        currentHorizontalSpeed = Mathf.Clamp(currentHorizontalSpeed, -moveClamp, moveClamp);

        // Apply bonus at the apex of a jump
        var apexBonus = Mathf.Sign(MovementInput.X) * this.apexBonus * apexPoint;
        currentHorizontalSpeed += apexBonus * Time.deltaTime;

        // Flip logic
        if ((MovementInput.X > 0.01f && !facingRight) || (MovementInput.X < -0.01f && facingRight))
        {
          shouldFlip = true;
          if (!Grounded)
          {
            Flip();
          }
        }
      }
      else
      {
        // No input. Let's slow the character down
        currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, 0, deAcceleration * Time.deltaTime);
      }

      if (currentHorizontalSpeed > 0 && collisionRight || currentHorizontalSpeed < 0 && collisionLeft)
      {
        // Don't walk through walls
        currentHorizontalSpeed = 0;
      }
    }

    #endregion

    #region Gravity

    [Header("GRAVITY")]
    [SerializeField] private float fallClamp = -40f;
    [SerializeField] private float minFallSpeed = 80f;
    [SerializeField] private float maxFallSpeed = 120f;
    private float fallSpeed;
    private bool enableGravity = true;

    private void CalculateGravity()
    {
      if (!enableGravity) return;

      if (collisionDown)
      {
        // Move out of the ground
        if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
        jumpState = JumpState.Grounded;
      }
      else
      {
        // Add downward force while ascending if we ended the jump early
        var fallSpeed = endedJumpEarly && currentVerticalSpeed > 0 ? this.fallSpeed * jumpEndEarlyGravityModifier : this.fallSpeed;

        // Fall
        currentVerticalSpeed -= fallSpeed * Time.deltaTime;

        // Clamp
        if (currentVerticalSpeed < fallClamp) currentVerticalSpeed = fallClamp;
      }
    }

    #endregion

    #region Jump

    [Header("JUMPING")][SerializeField] private float jumpHeight = 30;
    [SerializeField] private float jumpApexThreshold = 10f;
    [SerializeField] private float coyoteTimeThreshold = 0.1f;
    [SerializeField] private float jumpBuffer = 0.1f;
    [SerializeField] private float jumpEndEarlyGravityModifier = 3;
    private bool coyoteUsable;
    private bool endedJumpEarly = true;
    private float apexPoint; // Becomes 1 at the apex of a jump
    private float lastJumpPressed;
    private bool CanUseCoyote => coyoteUsable && !collisionDown && timeLeftGrounded + coyoteTimeThreshold > Time.time;
    private bool HasBufferedJump => collisionDown && lastJumpPressed + jumpBuffer > Time.time;

    private JumpState jumpState;
    private bool enableJump = true;

    private void CalculateJumpApex()
    {
      if (!collisionDown)
      {
        // Gets stronger the closer to the top of the jump
        apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
        fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, apexPoint);
      }
      else
      {
        apexPoint = 0;
      }
    }

    private void CalculateVelocityDirection()
    {
      if (currentVerticalSpeed > 0)
      {
        // Going up
        jumpState = JumpState.Jumping;
      }
      else if (currentVerticalSpeed < 0)
      {
        // Going down
        jumpState = JumpState.Falling;
      }
    }

    private void CalculateJump()
    {
      if (!enableJump) return;

      // Jump if: grounded or within coyote threshold || sufficient jump buffer
      if (jumpPressed && CanUseCoyote || HasBufferedJump)
      {
        currentVerticalSpeed = jumpHeight;
        endedJumpEarly = false;
        coyoteUsable = false;
        timeLeftGrounded = float.MinValue;
        JumpingThisFrame = true;
        jumpPressed = false;
        jumpState = JumpState.Jumping;
      }
      else
      {
        JumpingThisFrame = false;
      }

      if (collisionUp)
      {
        if (currentVerticalSpeed > 0) currentVerticalSpeed = 0;
      }
    }

    #endregion

    #region Move

    [Header("MOVE")]
    [SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
    private int freeColliderIterations = 10;
    private bool facingRight = true;
    public bool shouldFlip = false;

    // We cast our bounds before moving to avoid future collisions
    private void MoveCharacter()
    {
      var pos = transform.position;
      RawMovement = new Vector3(currentHorizontalSpeed, currentVerticalSpeed); // Used externally
      var move = RawMovement * Time.deltaTime;
      var furthestPoint = pos + move;

      // check furthest movement. If nothing hit, move and don't do extra checks
      var hit = Physics2D.OverlapBox(furthestPoint, characterBounds.size, 0, groundLayer);
      if (!hit)
      {
        transform.position += move;
        return;
      }

      // otherwise increment away from current pos; see what closest position we can move to
      var positionToMoveTo = transform.position;
      for (int i = 1; i < freeColliderIterations; i++)
      {
        // increment to check all but furthestPoint - we did that already
        var t = (float)i / freeColliderIterations;
        var posToTry = Vector2.Lerp(pos, furthestPoint, t);

        if (Physics2D.OverlapBox(posToTry, characterBounds.size, 0, groundLayer))
        {
          transform.position = positionToMoveTo;

          // We've landed on a corner or hit our head on a ledge. Nudge the player gently
          if (i == 1)
          {
            if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
            // var dir = transform.position - hit.transform.position;
            // transform.position += dir.normalized * move.magnitude;
          }

          return;
        }

        positionToMoveTo = posToTry;
      }
    }

    public void Flip()
    {
      Vector3 currentScale = transform.localScale;
      currentScale.x *= -1;
      transform.localScale = currentScale;
      facingRight = !facingRight;
      shouldFlip = false;
    }

    #endregion

    #region Ledge Climbing
    public void CalculateLedgeClimb()
    {
      if (Grounded) return;

      if (!canLedgeClimb) return;

      if (isLedgeClimbing) return;

      if (pushingWall && !isAgainstWall)
      {
        if (facingRight)
        {
          ledgePositionBottom = transform.position + new Vector3(wallCheckOffset.x, wallCheckOffset.y);

          ledgeStartPosition = new Vector2(Mathf.Ceil(ledgePositionBottom.x - wallCheckDistance) + ledgeClimbOffset1.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset1.y);
          ledgeEndPosition = new Vector2(Mathf.Ceil(ledgePositionBottom.x - wallCheckDistance) - ledgeClimbOffset2.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset2.y);
        }
        else
        {
          ledgePositionBottom = transform.position + new Vector3(-wallCheckOffset.x, wallCheckOffset.y);

          ledgeStartPosition = new Vector2(Mathf.Floor(ledgePositionBottom.x + wallCheckDistance) - ledgeClimbOffset1.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset1.y);
          ledgeEndPosition = new Vector2(Mathf.Floor(ledgePositionBottom.x + wallCheckDistance) + ledgeClimbOffset2.x, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbOffset2.y);
        }

        isLedgeClimbing = true;
        // canDash = false;
        enableJump = false;
        enableWalk = false;
        jumpState = JumpState.LedgeClimbing;
        enableGravity = false;
      }

      if (isLedgeClimbing)
      {
        currentVerticalSpeed = 0;
        currentHorizontalSpeed = 0;
        transform.position = ledgeStartPosition;
        StartCoroutine(LedgeClimb());
      }
    }

    void UpdateClimbPosition(int state)
    {
      float xMultiplier = facingRight ? 1f : -1f;
      if (state == 3)
      {
        transform.position += new Vector3(0f * xMultiplier, .2f);
      }
      else if (state == 4)
      {
        // transform.position += new Vector3(.2f * xMultiplier, .1f);
        transform.position += new Vector3(0f * xMultiplier, .1f);
      }
      else if (state == 5)
      {
        // transform.position += new Vector3(.3f * xMultiplier, .15f);
        transform.position += new Vector3(.0f * xMultiplier, .15f);
      }
      else if (state == 6)
      {
        transform.position += new Vector3(.3f * xMultiplier, .15f);
      }
      else if (state == 7)
      {
        transform.position = ledgeEndPosition;
      }
    }

    private IEnumerator LedgeClimb()
    {
      yield return new WaitForSeconds(8f / 18f);
      FinishLedgeClimb();
    }

    public void FinishLedgeClimb()
    {
      enableGravity = true;
      transform.position = ledgeEndPosition;
      isLedgeClimbing = false;
      // enableDash = true;
      enableJump = true;
      enableWalk = true;
      jumpState = JumpState.Grounded;
    }

    private void DrawLedgeClimbingGizmos()
    {
      Gizmos.color = Color.cyan;
      Gizmos.DrawLine(ledgeStartPosition, ledgeEndPosition);
    }
    private void DrawWallCheckGizmos()
    {
      float xOffset = facingRight ? wallCheckOffset.x : -wallCheckOffset.x;
      float xDistance = facingRight ? wallCheckDistance : -wallCheckDistance;

      Gizmos.color = Color.magenta;
      // Wall check
      Gizmos.DrawLine(transform.position + new Vector3(xOffset, wallCheckOffset.y), transform.position + new Vector3(xOffset, wallCheckOffset.y) + new Vector3(xDistance, 0));

      Gizmos.color = Color.yellow;
      // Ledge check
      Gizmos.DrawLine(transform.position + new Vector3(xOffset, ledgeCheckOffsetY), transform.position + new Vector3(xOffset, ledgeCheckOffsetY) + new Vector3(xDistance, 0));
    }
    #endregion
  }
}
