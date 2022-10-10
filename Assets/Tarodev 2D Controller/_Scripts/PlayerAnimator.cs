using UnityEngine;

namespace TarodevController {
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class PlayerAnimator : MonoBehaviour {
        private IPlayerController _player;
        private Animator _anim;
        private SpriteRenderer _renderer;
        private AudioSource _source;

        private void Awake() {
            _player = GetComponentInParent<IPlayerController>();
            _anim = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _source = GetComponent<AudioSource>();
        }

        private void Start() {
            _player.GroundedChanged += OnGroundedChanged;
            _player.WallGrabChanged += OnWallGrabChanged;
            _player.DashingChanged += OnDashingChanged;
            _player.LedgeClimbChanged += OnLedgeClimbChanged;
            _player.Jumped += OnJumped;
            _player.DoubleJumped += OnDoubleJumped;
            _player.Attacked += OnAttacked;
        }

        private void Update() {
            HandleSpriteFlipping();
            HandleGroundEffects();
            HandleWallSlideEffects();
            SetParticleColor(Vector2.down, _moveParticles);
            HandleAnimations();
        }

        private void HandleSpriteFlipping() {
            if (_player.ClimbingLedge) return;
            if (_player.WallDirection != 0) _renderer.flipX = _player.WallDirection == -1;
            else if (Mathf.Abs(_player.Input.x) > 0.1f) _renderer.flipX = _player.Input.x < 0;
        }

        #region Ground Movement

        [Header("GROUND MOVEMENT")] 
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private float _tiltChangeSpeed = .05f;
        [SerializeField] private AudioClip[] _footstepClips;
        private ParticleSystem.MinMaxGradient _currentGradient;
        private Vector2 _tiltVelocity;

        private void HandleGroundEffects() {
            // Move particles get bigger as you gain momentum
            var speedPoint = Mathf.InverseLerp(0, _player.PlayerStats.MaxSpeed, Mathf.Abs(_player.Speed.x));
            _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * speedPoint, 2 * Time.deltaTime);

            // Tilt with slopes
            transform.up = Vector2.SmoothDamp(transform.up, _grounded ? _player.GroundNormal : Vector2.up, ref _tiltVelocity, _tiltChangeSpeed);
        }

        private int _stepIndex = 0;

        public void PlayFootstep() {
            _stepIndex = (_stepIndex + 1) % _footstepClips.Length;
            PlaySound(_footstepClips[_stepIndex], 0.01f);
        }

        #endregion

        #region Wall Sliding and Climbing

        [Header("WALL")] 
        [SerializeField] private float _wallHitAnimTime = 0.167f;
        [SerializeField] private ParticleSystem _wallSlideParticles;
        [SerializeField] private AudioSource _wallSlideSource;
        [SerializeField] private AudioClip[] _wallClimbClips;
        [SerializeField] private float _maxWallSlideVolume = 0.2f;
        [SerializeField] private float _wallSlideVolumeSpeed = 0.6f;
        [SerializeField] private float _wallSlideParticleOffset = 0.3f;

        private bool _hitWall, _isOnWall, _isSliding, _dismountedWall;

        private void OnWallGrabChanged(bool onWall) {
            _hitWall = _isOnWall = onWall;
            _dismountedWall = !onWall;
        }

        private void HandleWallSlideEffects() {
            var slidingThisFrame = _isOnWall && !_grounded && _player.Speed.y < 0;

            if (!_isSliding && slidingThisFrame) {
                _isSliding = true;
                _wallSlideParticles.Play();
            }
            else if (_isSliding && !slidingThisFrame) {
                _isSliding = false;
                _wallSlideParticles.Stop();
            }

            SetParticleColor(new Vector2(_player.WallDirection, 0), _wallSlideParticles);
            _wallSlideParticles.transform.localPosition = new Vector3(_wallSlideParticleOffset * _player.WallDirection, 0, 0);

            _wallSlideSource.volume = _isSliding || _player.ClimbingLadder && _player.Speed.y < 0
                ? Mathf.MoveTowards(_wallSlideSource.volume, _maxWallSlideVolume, _wallSlideVolumeSpeed * Time.deltaTime)
                : 0;
        }

        private int _wallClimbIndex = 0;

        public void PlayWallClimbSound() {
            _wallClimbIndex = (_wallClimbIndex + 1) % _wallClimbClips.Length;
            PlaySound(_wallClimbClips[_wallClimbIndex], 0.1f);
        }

        #endregion

        #region Ledge Grabbing and Climbing

        //[Header("LEDGE")]
        private bool _isLedgeClimbing;

        private void OnLedgeClimbChanged(bool isLedgeClimbing) {
            _isLedgeClimbing = isLedgeClimbing;
            if (!isLedgeClimbing) _grounded = true;
            UnlockAnimationLock(); // unlocks the LockState, so that ledge climbing animation doesn't get skipped and so we can exit when told to do so

            // maybe play a sound or particle
        }

        #endregion
        
        #region Ladders
        
        [Header("LADDER")]
        [SerializeField] private AudioClip[] _ladderClips;
        private int _climbIndex = 0;

        public void PlayLadderClimbClip() {
            if (_player.Speed.y < 0) return;
            _climbIndex = (_climbIndex + 1) % _ladderClips.Length;
            PlaySound(_ladderClips[_climbIndex], 0.07f);
        }

        #endregion

        #region Dash

        [Header("DASHING")] 
        [SerializeField] private AudioClip _dashClip;
        [SerializeField] private ParticleSystem _dashParticles, _dashRingParticles;
        [SerializeField] private Transform _dashRingTransform;

        private void OnDashingChanged(bool dashing, Vector2 dir) {
            if (dashing) {
                _dashRingTransform.up = dir;
                _dashRingParticles.Play();
                _dashParticles.Play();
                PlaySound(_dashClip, 0.1f);
            }
            else {
                _dashParticles.Stop();
            }
        }

        #endregion

        #region Jumping and Landing

        [Header("JUMPING")] 
        [SerializeField] private float _minImpactForce = 20;
        [SerializeField] private float _landAnimDuration = 0.1f;
        [SerializeField] private AudioClip _landClip, _jumpClip, _doubleJumpClip;
        [SerializeField] private ParticleSystem _jumpParticles, _launchParticles, _doubleJumpParticles, _landParticles;
        [SerializeField] private Transform _jumpParticlesParent;

        private bool _jumpTriggered;
        private bool _landed;
        private bool _grounded;
        private bool _wallJumped;

        private void OnJumped(bool wallJumped) {
            if (_player.ClimbingLedge) return;
            
            _jumpTriggered = true;
            _wallJumped = wallJumped;
            PlaySound(_jumpClip, 0.05f, Random.Range(0.98f, 1.02f));

            _jumpParticlesParent.localRotation = Quaternion.Euler(0, 0, _player.WallDirection * 60f);

            SetColor(_jumpParticles);
            SetColor(_launchParticles);
            _jumpParticles.Play();
        }

        private void OnDoubleJumped() {
            PlaySound(_doubleJumpClip, 0.1f);
            _doubleJumpParticles.Play();
        }

        private void OnGroundedChanged(bool grounded, float impactForce) {
            _grounded = grounded;

            if (impactForce >= _minImpactForce) {
                var p = Mathf.InverseLerp(0, _minImpactForce, impactForce);
                _landed = true;
                _landParticles.transform.localScale = p * Vector3.one;
                _landParticles.Play();
                SetColor(_landParticles);
                PlaySound(_landClip, p * 0.1f);
            }

            if (_grounded) _moveParticles.Play();
            else _moveParticles.Stop();
        }

        #endregion

        #region Attack

        [Header("ATTACK")] 
        [SerializeField] private float _attackAnimTime = 0.25f;
        [SerializeField] private AudioClip _attackClip;
        private bool _attacked;

        private void OnAttacked() {
            _attacked = true;
            PlaySound(_attackClip, 0.1f, Random.Range(0.97f, 1.03f));
        }

        #endregion

        #region Animation

        private float _lockedTill;

        private void HandleAnimations() {
            var state = GetState();
            ResetFlags();
            if (state == _currentState) return;
            
            _anim.Play(state, 0); //_anim.CrossFade(state, 0, 0);
            _currentState = state;

            int GetState() {
                if (Time.time < _lockedTill) return _currentState;

                if (_isLedgeClimbing) return LockState(LedgeClimb, _player.PlayerStats.LedgeClimbDuration);
                if (_attacked) return LockState(Attack, _attackAnimTime);
                if (_player.ClimbingLadder) return _player.Speed.y == 0 ? ClimbIdle : Climb;

                if (!_grounded) {
                    if (_hitWall) return LockState(WallHit, _wallHitAnimTime);
                    if (_isOnWall) {
                        if (_player.Speed.y < 0) return WallSlide;
                        if (_player.GrabbingLedge) return LedgeGrab; // does this priority order give the right feel/look?
                        if (_player.Speed.y > 0) return WallClimb;
                        if (_player.Speed.y == 0) return WallIdle;
                    }
                }

                if (_player.Crouching) return _player.Input.x == 0 || !_grounded ? Crouch : Crawl;
                if (_landed) return LockState(Land, _landAnimDuration);
                if (_jumpTriggered) return _wallJumped ? Backflip : Jump;

                if (_grounded) return _player.Input.x == 0 ? Idle : Walk;
                if (_player.Speed.y > 0) return _wallJumped ? Backflip : Jump;
                return _dismountedWall ? LockState(WallDismount, 0.167f) : Fall;
                // TODO: determine if WallDismount looks good enough to use. Looks off to me. If it's fine, add clip duration (0.167f) to Stats

                int LockState(int s, float t) {
                    _lockedTill = Time.time + t;
                    return s;
                }
            }

            void ResetFlags() {
                _jumpTriggered = false;
                _landed = false;
                _attacked = false;
                _hitWall = false;
                _dismountedWall = false;
            }
        }

        private void UnlockAnimationLock() => _lockedTill = 0f;

        #region Cached Properties

        private int _currentState;

        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Crouch = Animator.StringToHash("Crouch");
        private static readonly int Crawl = Animator.StringToHash("Crawl");

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

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[2];

        private void SetParticleColor(Vector2 detectionDir, ParticleSystem system) {
            var hitCount = Physics2D.RaycastNonAlloc(transform.position, detectionDir, _groundHits, 2);
            for (var i = 0; i < hitCount; i++) {
                var hit = _groundHits[i];
                if (!hit.collider || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) continue;
                var color = r.color;
                _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
                SetColor(system);
                return;
            }
        }

        private void SetColor(ParticleSystem ps) {
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        #endregion

        #region Audio

        private void PlaySound(AudioClip clip, float volume = 1, float pitch = 1) {
            _source.pitch = pitch;
            _source.PlayOneShot(clip, volume);
        }

        #endregion
    }
}