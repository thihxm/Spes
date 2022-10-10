// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

using System;
using System.Collections;
using UnityEngine;

namespace Player
{
  [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
  public class PlayerController : Singleton<PlayerController>, IPlayerController
  {
    [SerializeField] private ScriptableStats stats;

    #region Internal

    private Rigidbody2D rigidBody;
    private InputManager inputManager;
    private CapsuleCollider2D[] colliders; // Standing and Crouching colliders
    private CapsuleCollider2D playerCollider; // Current collider
    private Bounds standingColliderBounds = new(new(0, 0.75f), Vector3.one); // gets overwritten in Awake. When not in play mode, is used for Gizmos
    private bool cachedTriggerSetting;

    private FrameInput frameInput;
    private Vector2 speed;
    private Vector2 currentExternalVelocity;
    private int fixedFrame;
    private bool hasControl = true;

    #endregion

    #region External

    public event Action<bool, float> GroundedChanged;
    public event Action<bool, Vector2> DashingChanged;
    public event Action<bool> WallGrabChanged;
    public event Action<bool> LedgeClimbChanged;
    public event Action<bool> Jumped;
    public event Action DoubleJumped;
    public event Action Attacked;
    public ScriptableStats PlayerStats => stats;
    public Vector2 Input => frameInput.Move;
    public Vector2 Speed => speed;
    public Vector2 GroundNormal => groundNormal;
    public int WallDirection => wallDirection;
    public bool Crouching => crouching;
    public bool ClimbingLadder => onLadder;
    public bool GrabbingLedge => grabbingLedge;
    public bool ClimbingLedge => climbingLedge;

    public virtual void ApplyVelocity(Vector2 vel, PlayerForce forceType)
    {
      if (forceType == PlayerForce.Burst) speed += vel;
      else currentExternalVelocity += vel;
    }

    public virtual void TakeAwayControl(bool resetVelocity = true)
    {
      if (resetVelocity)
      {
        rigidBody.velocity = Vector2.zero;
      }

      hasControl = false;
    }

    public virtual void ReturnControl()
    {
      speed = Vector2.zero;
      hasControl = true;
    }

    #endregion

    protected virtual void Awake()
    {
      rigidBody = GetComponent<Rigidbody2D>();
      inputManager = InputManager.Instance;
      colliders = GetComponents<CapsuleCollider2D>();

      // Colliders cannot be check whilst disabled. Let's cache its bounds
      standingColliderBounds = colliders[0].bounds;
      standingColliderBounds.center = colliders[0].offset;

      Physics2D.queriesStartInColliders = false;
      cachedTriggerSetting = Physics2D.queriesHitTriggers;

      SetCrouching(false);
    }

    protected virtual void Update()
    {
      GatherInput();
    }

    protected virtual void GatherInput()
    {
      frameInput = inputManager.FrameInput;

      if (frameInput.JumpDown)
      {
        jumpToConsume = true;
        frameJumpWasPressed = fixedFrame;
      }
    }

    protected virtual void FixedUpdate()
    {
      fixedFrame++;

      CheckCollisions();
      HandleCollisions();
      HandleWalls();
      HandleLedges();
      HandleLadders();

      HandleCrouching();
      HandleJump();
      HandleDash();
      HandleAttacking();

      HandleHorizontal();
      HandleVertical();
      ApplyVelocity();
    }

    #region Collisions

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[2];
    private readonly RaycastHit2D[] ceilingHits = new RaycastHit2D[2];
    private readonly Collider2D[] wallHits = new Collider2D[5];
    private readonly Collider2D[] ladderHits = new Collider2D[1];
    [SerializeField] private int groundHitCount;
    private int ceilingHitCount;
    private int wallHitCount;
    private int ladderHitCount;
    private int frameLeftGrounded = int.MinValue;
    [SerializeField] private bool grounded;

    protected virtual void CheckCollisions()
    {
      Physics2D.queriesHitTriggers = false;

      // Ground and Ceiling
      var origin = (Vector2)transform.position + playerCollider.offset;
      groundHitCount = Physics2D.CapsuleCastNonAlloc(origin, playerCollider.size, playerCollider.direction, 0, Vector2.down, groundHits, stats.GrounderDistance, ~stats.PlayerLayer);
      ceilingHitCount = Physics2D.CapsuleCastNonAlloc(origin, playerCollider.size, playerCollider.direction, 0, Vector2.up, ceilingHits, stats.GrounderDistance, ~stats.PlayerLayer);

      // Walls and Ladders
      var bounds = GetWallDetectionBounds();
      wallHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, wallHits, stats.ClimbableLayer);

      Physics2D.queriesHitTriggers = true; // Ladders are set to Trigger
      ladderHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, ladderHits, stats.LadderLayer);
      Physics2D.queriesHitTriggers = cachedTriggerSetting;
    }

    private Bounds GetWallDetectionBounds()
    {
      var colliderOrigin = transform.position + standingColliderBounds.center;
      return new Bounds(colliderOrigin, stats.WallDetectorSize);
    }

    protected virtual void HandleCollisions()
    {
      // Hit a Ceiling
      if (speed.y > 0 && ceilingHitCount > 0) speed.y = 0;

      // Landed on the Ground
      if (!grounded && groundHitCount > 0)
      {
        grounded = true;
        ResetDash();
        ResetJump();
        GroundedChanged?.Invoke(true, Mathf.Abs(speed.y));
      }
      // Left the Ground
      else if (grounded && groundHitCount == 0)
      {
        grounded = false;
        frameLeftGrounded = fixedFrame;
        GroundedChanged?.Invoke(false, 0);
      }
    }

    #endregion

    #region Walls

    private float currentWallJumpMoveMultiplier = 1f; // aka "Horizontal input influence"
    private int wallDirection;
    private bool isOnWall;

    protected virtual void HandleWalls()
    {
      if (!stats.AllowWalls) return;

      currentWallJumpMoveMultiplier = Mathf.MoveTowards(currentWallJumpMoveMultiplier, 1f, 1f / stats.WallJumpInputLossFrames);

      // May need to prioritize the nearest wall here... But who is going to make a climbable wall that tight?
      wallDirection = wallHitCount > 0 ? (int)Mathf.Sign(wallHits[0].transform.position.x - transform.position.x) : 0;

      if (!isOnWall && ShouldStickToWall()) SetOnWall(true);
      else if (isOnWall && !ShouldStickToWall()) SetOnWall(false);

      bool ShouldStickToWall()
      {
        if (wallDirection == 0 || grounded) return false;
        if (stats.RequireInputPush) return Mathf.Sign(frameInput.Move.x) == wallDirection;
        return true;
      }
    }

    private void SetOnWall(bool on)
    {
      isOnWall = on;
      if (on) speed = Vector2.zero;
      WallGrabChanged?.Invoke(on);
    }

    #endregion

    #region Ladders

    private Vector2 ladderSnapVel; // TODO: determine if we need to reset this when leaving a ladder, or use a different kind of Lerp/MoveTowards
    private int frameLeftLadder = int.MinValue;
    private bool onLadder;

    private bool CanEnterLadder => ladderHitCount > 0 && fixedFrame > frameLeftLadder + stats.LadderCooldownFrames;
    private bool LadderInputReached => Mathf.Abs(frameInput.Move.y) > stats.LadderClimbThreshold;

    protected virtual void HandleLadders()
    {
      if (!onLadder && CanEnterLadder && LadderInputReached) ToggleClimbingLadders(true);
      else if (onLadder && ladderHitCount == 0) ToggleClimbingLadders(false);

      // Snap to center of ladder
      if (onLadder && frameInput.Move.x == 0 && stats.SnapToLadders && hasControl)
      {
        var pos = rigidBody.position;
        rigidBody.position = Vector2.SmoothDamp(pos, new Vector2(ladderHits[0].transform.position.x, pos.y), ref ladderSnapVel, stats.LadderSnapSpeed);
      }
    }

    private void ToggleClimbingLadders(bool on)
    {
      if (on)
      {
        onLadder = true;
        speed = Vector2.zero;
      }
      else
      {
        if (!onLadder) return;
        frameLeftLadder = fixedFrame;
        onLadder = false;
      }
    }

    #endregion

    #region Ledges

    private Vector2 ledgeCornerPos;
    private bool grabbingLedge;
    private bool climbingLedge;

    protected virtual void HandleLedges()
    {
      if (climbingLedge || !isOnWall) return;

      grabbingLedge = TryGetLedgeCorner(out ledgeCornerPos);

      if (grabbingLedge) HandleLedgeGrabbing();
    }

    protected virtual bool TryGetLedgeCorner(out Vector2 cornerPos)
    {
      cornerPos = Vector2.zero;
      Vector2 grabHeight = rigidBody.position + stats.LedgeGrabPoint.y * Vector2.up;

      var hit1 = Physics2D.Raycast(grabHeight - stats.LedgeRaycastSpacing * Vector2.up, wallDirection * Vector2.right, 0.5f, stats.ClimbableLayer);
      if (!hit1.collider) return false; // Should hit below the ledge. Only used to determine xPos accurately

      var hit2 = Physics2D.Raycast(grabHeight + stats.LedgeRaycastSpacing * Vector2.up, wallDirection * Vector2.right, 0.5f, stats.ClimbableLayer);
      if (hit2.collider) return false; // we only are within ledge-grab range when the first hits and second doesn't

      var hit3 = Physics2D.Raycast(grabHeight + new Vector2(wallDirection * 0.5f, stats.LedgeRaycastSpacing), Vector2.down, 0.5f, stats.ClimbableLayer);
      if (!hit3.collider) return false; // gets our yPos of the corner

      cornerPos = new Vector2(hit1.point.x, hit3.point.y);
      return true;
    }

    protected virtual void HandleLedgeGrabbing()
    {
      // Snap to ledge position
      var xInput = frameInput.Move.x;
      var yInput = frameInput.Move.y;
      if (yInput != 0 && (xInput == 0 || Mathf.Sign(xInput) == wallDirection) && hasControl)
      {
        var pos = rigidBody.position;
        var targetPos = ledgeCornerPos - Vector2.Scale(stats.LedgeGrabPoint, new(wallDirection, 1f));
        rigidBody.position = Vector2.MoveTowards(pos, targetPos, stats.LedgeGrabDeceleration * Time.fixedDeltaTime);
      }

      // TODO: Create new stat variable instead of using Ladders or rename it to "vertical deadzone", "deadzone threshold", etc.
      if (yInput > stats.LadderClimbThreshold)
        StartCoroutine(ClimbLedge());
    }

    protected virtual IEnumerator ClimbLedge()
    {
      LedgeClimbChanged?.Invoke(true);
      climbingLedge = true;

      TakeAwayControl();
      var targetPos = ledgeCornerPos - Vector2.Scale(stats.LedgeGrabPoint, new(wallDirection, 1f));
      transform.position = targetPos;

      float lockedUntil = Time.time + stats.LedgeClimbDuration;
      while (Time.time < lockedUntil)
        yield return new WaitForFixedUpdate();

      LedgeClimbChanged?.Invoke(false);
      climbingLedge = false;
      grabbingLedge = false;
      SetOnWall(false);

      targetPos = ledgeCornerPos + Vector2.Scale(stats.StandUpOffset, new(wallDirection, 1f));
      transform.position = targetPos;
      ReturnControl();
    }

    #endregion

    #region Crouching

    private readonly Collider2D[] crouchHits = new Collider2D[5];
    private int frameStartedCrouching;
    private bool crouching;

    protected virtual bool CrouchPressed => frameInput.Move.y <= stats.CrouchInputThreshold;

    protected virtual void HandleCrouching()
    {
      if (crouching && onLadder) SetCrouching(false); // use standing collider when on ladder
      else if (crouching != CrouchPressed) SetCrouching(!crouching);
    }

    protected virtual void SetCrouching(bool active)
    {
      if (!crouching && (onLadder || isOnWall)) return; // Prevent crouching if climbing
      if (crouching && !CanStandUp()) return; // Prevent standing into colliders

      crouching = active;
      playerCollider = colliders[active ? 1 : 0];
      colliders[0].enabled = !active;
      colliders[1].enabled = active;

      if (crouching) frameStartedCrouching = fixedFrame;
    }

    protected bool CanStandUp()
    {
      var pos = rigidBody.position + (Vector2)standingColliderBounds.center + new Vector2(0, standingColliderBounds.extents.y);
      var size = new Vector2(standingColliderBounds.size.x, stats.CrouchBufferCheck);

      Physics2D.queriesHitTriggers = false;
      var hits = Physics2D.OverlapBoxNonAlloc(pos, size, 0, crouchHits, ~stats.PlayerLayer);
      Physics2D.queriesHitTriggers = cachedTriggerSetting;

      return hits == 0;
    }

    #endregion

    #region Jump

    private bool jumpToConsume;
    private bool coyoteUsable;
    private bool doubleJumpUsable;
    private bool bufferedJumpUsable;
    private int frameJumpWasPressed = int.MinValue;

    private bool CanUseCoyote => coyoteUsable && !grounded && fixedFrame < frameLeftGrounded + stats.CoyoteFrames;
    private bool HasBufferedJump => bufferedJumpUsable && fixedFrame < frameJumpWasPressed + stats.JumpBufferFrames;
    private bool CanDoubleJump => doubleJumpUsable && stats.AllowDoubleJump;

    protected virtual void HandleJump()
    {
      if (jumpToConsume || HasBufferedJump)
      {
        if (grounded || onLadder || CanUseCoyote) NormalJump();
        else if (isOnWall) WallJump();
        else if (jumpToConsume && CanDoubleJump) DoubleJump();
      }

      jumpToConsume = false; // Always consume the flag
    }

    protected virtual void NormalJump()
    {
      bufferedJumpUsable = false;
      coyoteUsable = false;
      doubleJumpUsable = true;
      ToggleClimbingLadders(false);
      speed.y = stats.JumpPower;
      Jumped?.Invoke(false);
    }

    protected virtual void WallJump()
    {
      bufferedJumpUsable = false;
      doubleJumpUsable = true; // note: double jump isn't currently refreshed after detaching from wall w/o jumping
      currentWallJumpMoveMultiplier = 0;
      SetOnWall(false);
      speed = Vector2.Scale(stats.WallJumpPower, new(-wallDirection, 1));
      Jumped?.Invoke(true);
    }

    protected virtual void DoubleJump()
    {
      doubleJumpUsable = false;
      speed.y = stats.JumpPower;
      DoubleJumped?.Invoke();
    }

    protected virtual void ResetJump()
    {
      coyoteUsable = true;
      bufferedJumpUsable = true;
      doubleJumpUsable = false;
    }

    #endregion

    #region Dash

    private bool dashToConsume;
    private bool canDash;
    private Vector2 dashVel;
    private bool dashing;
    private int startedDashing;

    protected virtual void HandleDash()
    {
      if (dashToConsume && canDash && !crouching)
      {
        var dir = new Vector2(frameInput.Move.x, Mathf.Max(frameInput.Move.y, 0f)).normalized;
        if (dir == Vector2.zero)
        {
          dashToConsume = false;
          return;
        }

        dashVel = dir * stats.DashVelocity;
        dashing = true;
        canDash = false;
        startedDashing = fixedFrame;
        DashingChanged?.Invoke(true, dir);

        // Strip external buildup
        currentExternalVelocity = Vector2.zero;
      }

      if (dashing)
      {
        speed = dashVel;
        // Cancel when the time is out or we've reached our max safety distance
        if (fixedFrame > startedDashing + stats.DashDurationFrames)
        {
          dashing = false;
          DashingChanged?.Invoke(false, Vector2.zero);
          if (speed.y > 0) speed.y = 0;
          speed.x *= stats.DashEndHorizontalMultiplier;
          if (grounded) canDash = true;
        }
      }

      dashToConsume = false;
    }

    protected virtual void ResetDash()
    {
      canDash = true;
    }

    #endregion

    #region Attacking

    private bool attackToConsume;
    private int frameLastAttacked = int.MinValue;

    protected virtual void HandleAttacking()
    {
      if (!attackToConsume) return;

      if (fixedFrame > frameLastAttacked + stats.AttackFrameCooldown)
      {
        frameLastAttacked = fixedFrame;
        Attacked?.Invoke();
      }

      attackToConsume = false;
    }

    #endregion

    #region Horizontal

    protected virtual void HandleHorizontal()
    {
      if (dashing) return;

      if (frameInput.Move.x != 0)
      {
        if (crouching && grounded)
        {
          var crouchPoint = Mathf.InverseLerp(0, stats.CrouchSlowdownFrames, fixedFrame - frameStartedCrouching);
          var diminishedMaxSpeed = stats.MaxSpeed * Mathf.Lerp(1, stats.CrouchSpeedPenalty, crouchPoint);

          speed.x = Mathf.MoveTowards(speed.x, diminishedMaxSpeed * frameInput.Move.x, stats.GroundDeceleration * Time.fixedDeltaTime);
        }
        else
        {
          // Prevent useless horizontal speed buildup when against a wall
          if (wallHitCount > 0 && Mathf.Approximately(rigidBody.velocity.x, 0) && Mathf.Sign(frameInput.Move.x) == Mathf.Sign(speed.x))
            speed.x = 0;

          var inputX = frameInput.Move.x * (onLadder ? stats.LadderShimmySpeedMultiplier : 1);
          speed.x = Mathf.MoveTowards(speed.x, inputX * stats.MaxSpeed, currentWallJumpMoveMultiplier * stats.Acceleration * Time.fixedDeltaTime);
        }
      }
      else
        speed.x = Mathf.MoveTowards(speed.x, 0, (grounded ? stats.GroundDeceleration : stats.AirDeceleration) * Time.fixedDeltaTime);
    }

    #endregion

    #region Vertical

    private Vector2 groundNormal;

    protected virtual void HandleVertical()
    {
      if (dashing) return;

      // Ladder
      if (onLadder)
      {
        var inputY = frameInput.Move.y;
        speed.y = inputY * (inputY > 0 ? stats.LadderClimbSpeed : stats.LadderSlideSpeed);

        return;
      }

      // Grounded & Slopes
      if (grounded && speed.y <= 0f)
      {
        speed.y = stats.GroundingForce;

        // We use a raycast here as the groundHits from capsule cast act a bit weird.
        Physics2D.queriesHitTriggers = false;
        var hit = Physics2D.Raycast(transform.position, Vector2.down, stats.GrounderDistance * 2, ~stats.PlayerLayer);
        Physics2D.queriesHitTriggers = cachedTriggerSetting;
        if (hit.collider != null)
        {
          groundNormal = hit.normal;

          if (!Mathf.Approximately(groundNormal.y, 1f))
          { // on a slope
            speed.y = speed.x * -groundNormal.x / groundNormal.y;
            if (speed.x != 0) speed.y += stats.GroundingForce;
          }
        }
        else
          groundNormal = Vector2.zero;

        return;
      }

      // Wall Climbing & Sliding
      if (isOnWall)
      {
        if (frameInput.Move.y > 0) speed.y = stats.WallClimbSpeed;
        else if (frameInput.Move.y < 0) speed.y = -stats.MaxWallFallSpeed; // TODO: new stat variable for better feel?
        else if (grabbingLedge) speed.y = Mathf.MoveTowards(speed.y, 0, stats.LedgeGrabDeceleration * Time.fixedDeltaTime);
        else speed.y = Mathf.MoveTowards(Mathf.Min(speed.y, 0), -stats.MaxWallFallSpeed, stats.WallFallAcceleration * Time.fixedDeltaTime);

        return;
      }

      // In Air
      var fallSpeed = stats.FallAcceleration;
      speed.y = Mathf.MoveTowards(speed.y, -stats.MaxFallSpeed, fallSpeed * Time.fixedDeltaTime);
    }

    #endregion

    protected virtual void ApplyVelocity()
    {
      if (!hasControl) return;
      rigidBody.velocity = speed + currentExternalVelocity;

      currentExternalVelocity = Vector2.MoveTowards(currentExternalVelocity, Vector2.zero, stats.ExternalVelocityDecay * Time.fixedDeltaTime);
    }


    private void OnDrawGizmos()
    {
      if (stats.ShowWallDetection)
      {
        Gizmos.color = Color.white;
        var bounds = GetWallDetectionBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
      }
      if (stats.ShowLedgeDetection)
      {
        Gizmos.color = Color.red;
        var facingDir = Mathf.Sign(wallDirection);
        var grabHeight = transform.position + stats.LedgeGrabPoint.y * Vector3.up;
        var grabPoint = grabHeight + facingDir * stats.LedgeGrabPoint.x * Vector3.right;
        Gizmos.DrawWireSphere(grabPoint, 0.05f);
        Gizmos.DrawWireSphere(grabPoint + Vector3.Scale(stats.StandUpOffset, new(facingDir, 1)), 0.05f);
        Gizmos.DrawRay(grabHeight - stats.LedgeRaycastSpacing * Vector3.up, 0.5f * facingDir * Vector3.right);
        Gizmos.DrawRay(grabHeight + stats.LedgeRaycastSpacing * Vector3.up, 0.5f * facingDir * Vector3.right);
      }
    }
  }
}