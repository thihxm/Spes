using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aarthificial.Reanimation;

namespace Player
{
  public class PlayerRenderer : Singleton<PlayerRenderer>
  {
    private static class Drivers
    {
      public const string JumpState = "jumpState";
      public const string IsGrounded = "isGrounded";
      public const string IsDashing = "isDashing";
      public const string State = "state";
      public const string IsMoving = "isMoving";
      public const string Falling = "falling";
      public const string Climbing = "climbing";
      public const string ShouldFlip = "shouldFlip";
      public const string FlipEvent = "flipEvent";
    }

    public delegate void ClimbAction(int state);
    public event ClimbAction OnChangeClimbState;

    private Reanimator reanimator;
    private PlayerController player;

    // private bool landed;
    // private bool grounded;

    private void Awake()
    {
      reanimator = GetComponent<Reanimator>();
      player = PlayerController.Instance;
      // player.GroundedChanged += OnGroundedChanged;
    }

    private void OnEnable()
    {
      player.GroundedChanged += OnGroundedChanged;
      // reanimator.AddListener(Drivers.Climbing, OnClimbing);
      reanimator.AddListener(Drivers.FlipEvent, FlipEvent);
    }

    private void OnDisable()
    {
      player.GroundedChanged -= OnGroundedChanged;
      // reanimator.RemoveListener(Drivers.Climbing, OnClimbing);
      reanimator.RemoveListener(Drivers.FlipEvent, FlipEvent);
    }

    private void Update()
    {
      HandleAnimations();
      //   var velocity = player.Speed;
      bool moving = Mathf.Abs(player.Input.x) > 0 && Mathf.Abs(player.Speed.x) > 0.01f;

      // reanimator.Flip = !player.facingRight;
      reanimator.Set(Drivers.IsMoving, moving);
      reanimator.Set(Drivers.IsGrounded, grounded);
      reanimator.Set(Drivers.IsDashing, dashing);
      reanimator.Set(Drivers.JumpState, (int)jumpState);
      reanimator.Set(Drivers.ShouldFlip, player.shouldFlip);

      bool didLandInThisFrame = reanimator.WillChange(Drivers.IsGrounded, true);

      if (didLandInThisFrame)
      {
        reanimator.ForceRerender();
      }
    }

    #region Animation
    private void HandleAnimations()
    {
      GetState();
      Debug.Log("jumpState: " + jumpState);
      Debug.Log("flip: " + player.WallDirection);
      ResetFlags();

      void GetState()
      {
        if (isLedgeClimbing)
        {
          jumpState = JumpState.LedgeClimbing;
          return;
        }

        if (!grounded)
        {
          // if (hitWall) return LockState(WallHit, wallHitAnimTime);
          // if (isOnWall)
          // {
          //   if (player.Speed.y < 0) return WallSlide;
          //   if (player.GrabbingLedge) return LedgeGrab; // does this priority order give the right feel/look?
          //   if (player.Speed.y > 0) return WallClimb;
          //   if (player.Speed.y == 0) return WallIdle;
          // }
          if (player.Speed.y > 0)
          {
            jumpState = JumpState.Jumping;
            return;
          }

          if (player.Speed.y < 0)
          {
            jumpState = JumpState.Falling;
            return;
          }
        }

        if (landed)
        {
          jumpState = JumpState.Grounded;
          return;
        }

        if (jumpTriggered)
        {
          jumpState = JumpState.Jumping;
          return;
        }

        if (grounded)
        {
          jumpState = JumpState.Grounded;
          return;
        }

        // if (player.Crouching) return player.Input.x == 0 || !grounded ? Crouch : Crawl;
        // if (jumpTriggered) return wallJumped ? Backflip : Jump;

        // return dismountedWall ? LockState(WallDismount, 0.167f) : Fall;
        // TODO: determine if WallDismount looks good enough to use. Looks off to me. If it's fine, add clip duration (0.167f) to Stats
      }

      void ResetFlags()
      {
        jumpTriggered = false;
        landed = false;
        // attacked = false;
        // hitWall = false;
        // dismountedWall = false;
      }
    }

    #endregion



    #region Dash

    [Header("DASHING")]
    private bool dashing;

    private void OnDashingChanged(bool dashing, Vector2 dir)
    {
      this.dashing = dashing;
    }

    #endregion

    #region Jumping and Landing

    [Header("JUMPING")]
    [SerializeField] private float minImpactForce = 20;
    [SerializeField] private float landAnimDuration = 0.1f;
    // [SerializeField] private AudioClip landClip, jumpClip, doubleJumpClip;
    // [SerializeField] private ParticleSystem jumpParticles, launchParticles, doubleJumpParticles, landParticles;
    // [SerializeField] private Transform jumpParticlesParent;

    private bool jumpTriggered;
    private bool landed;
    private bool grounded;
    private bool wallJumped;

    private JumpState jumpState;

    private void OnJumped(bool wallJumped)
    {
      if (player.ClimbingLedge) return;

      jumpTriggered = true;
      this.wallJumped = wallJumped;
    }

    private void OnGroundedChanged(bool grounded, float impactForce)
    {
      this.grounded = grounded;

      if (impactForce >= minImpactForce)
      {
        var p = Mathf.InverseLerp(0, minImpactForce, impactForce);
        landed = true;
      }

      // if (this.grounded) moveParticles.Play();
      // else moveParticles.Stop();
    }

    #endregion

    #region Ledge Grabbing and Climbing

    //[Header("LEDGE")]
    private bool isLedgeClimbing;

    private void OnLedgeClimbChanged(bool isLedgeClimbing)
    {
      this.isLedgeClimbing = isLedgeClimbing;
      if (!this.isLedgeClimbing) grounded = true;
    }

    #endregion

    private void FlipEvent()
    {
      player.Flip();
    }

    // private void OnClimbing()
    // {
    //   int climbingState = reanimator.State.Get(Drivers.Climbing, 0);
    //   if (climbingState > 0)
    //   {
    //     OnChangeClimbState?.Invoke(climbingState);
    //   }
    // }
  }
}
