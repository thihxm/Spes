using System;
using UnityEngine;

namespace Player
{
  [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
  public class PlayerAnimator : MonoBehaviour
  {
    private IPlayerController player;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    // private AudioSource source;

    private void Awake()
    {
      player = GetComponentInParent<IPlayerController>();
      anim = GetComponent<Animator>();
      spriteRenderer = GetComponent<SpriteRenderer>();
      // source = GetComponent<AudioSource>();
    }

    private void Start()
    {
      player.GroundedChanged += OnGroundedChanged;
      player.WallGrabChanged += OnWallGrabChanged;
      player.DashingChanged += OnDashingChanged;
      player.LedgeClimbChanged += OnLedgeClimbChanged;
      player.Jumped += OnJumped;
      player.DoubleJumped += OnDoubleJumped;
    }

    private void Update()
    {
      HandleSpriteFlipping();
      HandleGroundEffects();
      HandleWallSlideEffects();
      SetParticleColor(Vector2.down, moveParticles);
      HandleAnimations();
    }

    private void HandleSpriteFlipping()
    {
      if (player.ClimbingLedge) return;
      // if (player.WallDirection != 0) spriteRenderer.flipX = player.WallDirection == -1;
      // else if (Mathf.Abs(player.Input.x) > 0.1f) spriteRenderer.flipX = player.Input.x < 0;
      if (player.ShouldFlip)
      {
        flipping = true;
      }
    }

    #region Ground Movement

    [Header("GROUND MOVEMENT")]
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private float tiltChangeSpeed = .05f;
    // [SerializeField] private AudioClip[] footstepClips;
    private ParticleSystem.MinMaxGradient currentGradient;
    private Vector2 tiltVelocity;

    private void HandleGroundEffects()
    {
      // Move particles get bigger as you gain momentum
      var speedPoint = Mathf.InverseLerp(0, player.PlayerStats.MaxSpeed, Mathf.Abs(player.Speed.x));
      moveParticles.transform.localScale = Vector3.MoveTowards(moveParticles.transform.localScale, Vector3.one * speedPoint, 2 * Time.deltaTime);

      // Tilt with slopes
      transform.up = Vector2.SmoothDamp(transform.up, grounded ? player.GroundNormal : Vector2.up, ref tiltVelocity, tiltChangeSpeed);

      // 
      if (player.Input.x == 0)
      {
        shouldWalk = false;
      }
    }

    private bool shouldWalk = false;

    private void StartRunningEnd()
    {
      shouldWalk = true;
    }

    private int stepIndex = 0;

    public void PlayFootstep()
    {
      // stepIndex = (stepIndex + 1) % footstepClips.Length;
      // PlaySound(footstepClips[stepIndex], 0.01f);
    }

    [SerializeField] private bool flipping;

    public void ChangingDirectionEnd()
    {
      Debug.Log("ChangingDirectionEnd");
      flipping = false;
      player.Flip();
    }

    #endregion

    #region Wall Sliding and Climbing

    [Header("WALL")]
    [SerializeField] private float wallHitAnimTime = 0.167f;
    [SerializeField] private ParticleSystem wallSlideParticles;
    // [SerializeField] private AudioSource wallSlideSource;
    // [SerializeField] private AudioClip[] wallClimbClips;
    [SerializeField] private float maxWallSlideVolume = 0.2f;
    [SerializeField] private float wallSlideVolumeSpeed = 0.6f;
    [SerializeField] private float wallSlideParticleOffset = 0.3f;

    private bool hitWall, isOnWall, isSliding, dismountedWall;

    private void OnWallGrabChanged(bool onWall)
    {
      hitWall = isOnWall = onWall;
      dismountedWall = !onWall;
    }

    private void HandleWallSlideEffects()
    {
      var slidingThisFrame = isOnWall && !grounded && player.Speed.y < 0;

      if (!isSliding && slidingThisFrame)
      {
        isSliding = true;
        wallSlideParticles.Play();
      }
      else if (isSliding && !slidingThisFrame)
      {
        isSliding = false;
        wallSlideParticles.Stop();
      }

      SetParticleColor(new Vector2(player.WallDirection, 0), wallSlideParticles);
      wallSlideParticles.transform.localPosition = new Vector3(wallSlideParticleOffset * player.WallDirection, 0, 0);

      // wallSlideSource.volume = isSliding && player.Speed.y < 0
      //     ? Mathf.MoveTowards(wallSlideSource.volume, maxWallSlideVolume, wallSlideVolumeSpeed * Time.deltaTime)
      //     : 0;
    }

    private int wallClimbIndex = 0;

    public void PlayWallClimbSound()
    {
      // wallClimbIndex = (wallClimbIndex + 1) % wallClimbClips.Length;
      // PlaySound(wallClimbClips[wallClimbIndex], 0.1f);
    }

    #endregion

    #region Ledge Grabbing and Climbing

    [Header("LEDGE")]
    private bool isLedgeClimbing;

    private void OnLedgeClimbChanged(bool isLedgeClimbing)
    {
      this.isLedgeClimbing = isLedgeClimbing;
      if (!isLedgeClimbing) grounded = true;
      UnlockAnimationLock(); // unlocks the LockState, so that ledge climbing animation doesn't get skipped and so we can exit when told to do so

      // maybe play a sound or particle
    }

    #endregion

    #region Dash

    [Header("DASHING")]
    // [SerializeField] private AudioClip dashClip;
    [SerializeField] private ParticleSystem dashParticles, dashRingParticles;
    [SerializeField] private Transform dashRingTransform;
    private bool isDashing;
    private bool endedDash;

    private void OnDashingChanged(bool dashing, Vector2 dir)
    {
      isDashing = dashing;
      if (dashing)
      {
        dashRingTransform.up = dir;
        dashRingParticles.Play();
        dashParticles.Play();
        // PlaySound(dashClip, 0.1f);
      }
      else
      {
        dashParticles.Stop();
        endedDash = true;
      }
    }

    #endregion

    #region Jumping and Landing

    [Header("JUMPING")]
    [SerializeField] private float minImpactForce = 20;
    [SerializeField] private float landAnimDuration = 0.1f;
    // [SerializeField] private AudioClip landClip, jumpClip, doubleJumpClip;
    [SerializeField] private ParticleSystem jumpParticles, launchParticles, doubleJumpParticles, landParticles;
    [SerializeField] private Transform jumpParticlesParent;

    private bool jumpTriggered;
    private bool landed;
    private bool grounded;
    private bool wallJumped;

    private void OnJumped(bool wallJumped)
    {
      if (player.ClimbingLedge) return;

      jumpTriggered = true;
      this.wallJumped = wallJumped;
      // PlaySound(jumpClip, 0.05f, Random.Range(0.98f, 1.02f));

      jumpParticlesParent.localRotation = Quaternion.Euler(0, 0, player.WallDirection * 60f);

      SetColor(jumpParticles);
      SetColor(launchParticles);
      jumpParticles.Play();
    }

    private void OnDoubleJumped()
    {
      // PlaySound(doubleJumpClip, 0.1f);
      doubleJumpParticles.Play();
    }

    private void OnGroundedChanged(bool grounded, float impactForce)
    {
      this.grounded = grounded;

      if (impactForce >= minImpactForce)
      {
        var p = Mathf.InverseLerp(0, minImpactForce, impactForce);
        landed = true;
        landParticles.transform.localScale = p * Vector3.one;
        landParticles.Play();
        SetColor(landParticles);
        // PlaySound(landClip, p * 0.1f);
      }

      if (grounded) moveParticles.Play();
      else moveParticles.Stop();
    }

    #endregion

    #region Animation

    private float lockedTill;

    private void HandleAnimations()
    {
      var state = GetState();
      ResetFlags();
      if (state == currentState) return;

      anim.Play(state, 0); //anim.CrossFade(state, 0, 0);
      currentState = state;

      int GetState()
      {
        if (Time.time < lockedTill) return currentState;

        if (isLedgeClimbing) return LockState(LedgeClimb, player.PlayerStats.LedgeClimbDuration);

        if (!grounded)
        {
          if (hitWall) return LockState(WallHit, wallHitAnimTime);
          if (isOnWall)
          {
            if (player.Speed.y < 0) return WallSlide;
            if (player.GrabbingLedge) return LedgeGrab; // does this priority order give the right feel/look?
            // if (player.Speed.y > 0) return WallClimb;
            if (player.Speed.y == 0) return WallIdle;
          }
        }

        if (player.Crouching) return player.Input.x == 0 || !grounded ? Crouch : Crawl;
        if (landed) return LockState(Land, landAnimDuration);
        if (jumpTriggered) return wallJumped ? Backflip : Jump;
        if (isDashing) return Dash;
        if (endedDash) return LockState(EndDash, 0.167f);

        if (grounded)
        {
          if (flipping) return LockState(ChangingDirection, 0.3f);
          if (player.Input.x == 0)
          {
            return Idle;
          }
          else if (!shouldWalk)
          {
            return StartRunning;
          }
          if (shouldWalk) return Walk;
        }

        if (player.Speed.y > 0) return wallJumped ? Backflip : Jump;
        return dismountedWall ? LockState(WallDismount, 0.167f) : Fall;
        // TODO: determine if WallDismount looks good enough to use. Looks off to me. If it's fine, add clip duration (0.167f) to Stats

        int LockState(int s, float t)
        {
          lockedTill = Time.time + t;
          endedDash = false;
          flipping = false;
          return s;
        }
      }

      void ResetFlags()
      {
        jumpTriggered = false;
        landed = false;
        hitWall = false;
        dismountedWall = false;
      }
    }

    private void UnlockAnimationLock() => lockedTill = 0f;

    #region Cached Properties

    private int currentState;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int StartRunning = Animator.StringToHash("StartRunning");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int ChangingDirection = Animator.StringToHash("ChangingDirection");
    private static readonly int Crouch = Animator.StringToHash("Crouch");
    private static readonly int Crawl = Animator.StringToHash("Crawl");

    private static readonly int Dash = Animator.StringToHash("Dash");
    private static readonly int EndDash = Animator.StringToHash("EndDash");

    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Fall = Animator.StringToHash("Fall");
    private static readonly int Land = Animator.StringToHash("Land");

    private static readonly int ClimbIdle = Animator.StringToHash("ClimbIdle");
    private static readonly int Climb = Animator.StringToHash("Climb");

    private static readonly int WallHit = Animator.StringToHash("WallHit");
    private static readonly int WallIdle = Animator.StringToHash("WallIdle");
    private static readonly int WallClimb = Animator.StringToHash("WallClimb");
    private static readonly int WallSlide = Animator.StringToHash("WallSlide");
    private static readonly int WallDismount = Animator.StringToHash("WallDismount");
    private static readonly int Backflip = Animator.StringToHash("Backflip");

    private static readonly int LedgeGrab = Animator.StringToHash("LedgeGrab");
    private static readonly int LedgeClimb = Animator.StringToHash("LedgeClimb");

    private static readonly int Attack = Animator.StringToHash("Attack");
    #endregion

    #endregion

    #region Particles

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[2];

    private void SetParticleColor(Vector2 detectionDir, ParticleSystem system)
    {
      var hitCount = Physics2D.RaycastNonAlloc(transform.position, detectionDir, groundHits, 2);
      for (var i = 0; i < hitCount; i++)
      {
        var hit = groundHits[i];
        if (!hit.collider || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) continue;
        var color = r.color;
        currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
        SetColor(system);
        return;
      }
    }

    private void SetColor(ParticleSystem ps)
    {
      var main = ps.main;
      main.startColor = currentGradient;
    }

    #endregion

    #region Audio

    // private void PlaySound(AudioClip clip, float volume = 1, float pitch = 1)
    // {
    //   source.pitch = pitch;
    //   source.PlayOneShot(clip, volume);
    // }

    #endregion
  }
}