// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

using System;
using System.Collections;
using UnityEngine;

namespace TarodevController {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController {
        [SerializeField] private ScriptableStats _stats;

        #region Internal

        private Rigidbody2D _rb;
        private PlayerInput _input;
        private CapsuleCollider2D[] _cols; // Standing and Crouching colliders
        private CapsuleCollider2D _col; // Current collider
        private Bounds _standingColliderBounds = new(new(0, 0.75f), Vector3.one); // gets overwritten in Awake. When not in play mode, is used for Gizmos
        private bool _cachedTriggerSetting;

        private FrameInput _frameInput;
        private Vector2 _speed;
        private Vector2 _currentExternalVelocity;
        private int _fixedFrame;
        private bool _hasControl = true;

        #endregion

        #region External

        public event Action<bool, float> GroundedChanged;
        public event Action<bool, Vector2> DashingChanged;
        public event Action<bool> WallGrabChanged;
        public event Action<bool> LedgeClimbChanged;
        public event Action<bool> Jumped;
        public event Action DoubleJumped;
        public event Action Attacked;
        public ScriptableStats PlayerStats => _stats;
        public Vector2 Input => _frameInput.Move;
        public Vector2 Speed => _speed;
        public Vector2 GroundNormal => _groundNormal;
        public int WallDirection => _wallDir;
        public bool Crouching => _crouching;
        public bool ClimbingLadder => _onLadder;
        public bool GrabbingLedge => _grabbingLedge;
        public bool ClimbingLedge => _climbingLedge;

        public virtual void ApplyVelocity(Vector2 vel, PlayerForce forceType) {
            if (forceType == PlayerForce.Burst) _speed += vel;
            else _currentExternalVelocity += vel;
        }

        public virtual void TakeAwayControl(bool resetVelocity = true) {
            if (resetVelocity) {
                _rb.velocity = Vector2.zero;
            }

            _hasControl = false;
        }

        public virtual void ReturnControl() {
            _speed = Vector2.zero;
            _hasControl = true;
        }

        #endregion

        protected virtual void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInput>();
            _cols = GetComponents<CapsuleCollider2D>();

            // Colliders cannot be check whilst disabled. Let's cache its bounds
            _standingColliderBounds = _cols[0].bounds;
            _standingColliderBounds.center = _cols[0].offset;

            Physics2D.queriesStartInColliders = false;
            _cachedTriggerSetting = Physics2D.queriesHitTriggers;

            SetCrouching(false);
        }

        protected virtual void Update() {
            GatherInput();
        }

        protected virtual void GatherInput() {
            _frameInput = _input.FrameInput;

            if (_frameInput.JumpDown) {
                _jumpToConsume = true;
                _frameJumpWasPressed = _fixedFrame;
            }

            if (_frameInput.DashDown && _stats.AllowDash) _dashToConsume = true;
            if (_frameInput.AttackDown && _stats.AllowAttacks) _attackToConsume = true;
        }

        protected virtual void FixedUpdate() {
            _fixedFrame++;

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

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[2];
        private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[2];
        private readonly Collider2D[] _wallHits = new Collider2D[5];
        private readonly Collider2D[] _ladderHits = new Collider2D[1];
        private int _groundHitCount;
        private int _ceilingHitCount;
        private int _wallHitCount;
        private int _ladderHitCount;
        private int _frameLeftGrounded = int.MinValue;
        private bool _grounded;

        protected virtual void CheckCollisions() {
            Physics2D.queriesHitTriggers = false;
            
            // Ground and Ceiling
            var origin = (Vector2)transform.position + _col.offset;
            _groundHitCount = Physics2D.CapsuleCastNonAlloc(origin, _col.size, _col.direction, 0, Vector2.down, _groundHits, _stats.GrounderDistance, ~_stats.PlayerLayer);
            _ceilingHitCount = Physics2D.CapsuleCastNonAlloc(origin, _col.size, _col.direction, 0, Vector2.up, _ceilingHits, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Walls and Ladders
            var bounds = GetWallDetectionBounds();
            _wallHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, _wallHits, _stats.ClimbableLayer);

            Physics2D.queriesHitTriggers = true; // Ladders are set to Trigger
            _ladderHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, _ladderHits, _stats.LadderLayer);
            Physics2D.queriesHitTriggers = _cachedTriggerSetting;
        }

        private Bounds GetWallDetectionBounds() {
            var colliderOrigin = transform.position + _standingColliderBounds.center;
            return new Bounds(colliderOrigin, _stats.WallDetectorSize);
        }

        protected virtual void HandleCollisions() {
            // Hit a Ceiling
            if (_speed.y > 0 && _ceilingHitCount > 0) _speed.y = 0;

            // Landed on the Ground
            if (!_grounded && _groundHitCount > 0) {
                _grounded = true;
                ResetDash();
                ResetJump();
                GroundedChanged?.Invoke(true, Mathf.Abs(_speed.y));
            }
            // Left the Ground
            else if (_grounded && _groundHitCount == 0) {
                _grounded = false;
                _frameLeftGrounded = _fixedFrame;
                GroundedChanged?.Invoke(false, 0);
            }
        }

        #endregion

        #region Walls

        private float _currentWallJumpMoveMultiplier = 1f; // aka "Horizontal input influence"
        private int _wallDir;
        private bool _isOnWall;

        protected virtual void HandleWalls() {
            if (!_stats.AllowWalls) return;

            _currentWallJumpMoveMultiplier = Mathf.MoveTowards(_currentWallJumpMoveMultiplier, 1f, 1f / _stats.WallJumpInputLossFrames);

            // May need to prioritize the nearest wall here... But who is going to make a climbable wall that tight?
            _wallDir = _wallHitCount > 0 ? (int)Mathf.Sign(_wallHits[0].transform.position.x - transform.position.x) : 0;

            if (!_isOnWall && ShouldStickToWall()) SetOnWall(true);
            else if (_isOnWall && !ShouldStickToWall()) SetOnWall(false);

            bool ShouldStickToWall() {
                if (_wallDir == 0 || _grounded) return false;
                if (_stats.RequireInputPush) return Mathf.Sign(_frameInput.Move.x) == _wallDir;
                return true;
            }
        }

        private void SetOnWall(bool on) {
            _isOnWall = on;
            if (on) _speed = Vector2.zero;
            WallGrabChanged?.Invoke(on);
        }

        #endregion

        #region Ladders

        private Vector2 _ladderSnapVel; // TODO: determine if we need to reset this when leaving a ladder, or use a different kind of Lerp/MoveTowards
        private int _frameLeftLadder = int.MinValue;
        private bool _onLadder;

        private bool CanEnterLadder => _ladderHitCount > 0 && _fixedFrame > _frameLeftLadder + _stats.LadderCooldownFrames;
        private bool LadderInputReached => Mathf.Abs(_frameInput.Move.y) > _stats.LadderClimbThreshold;

        protected virtual void HandleLadders() {
            if (!_onLadder && CanEnterLadder && LadderInputReached) ToggleClimbingLadders(true);
            else if (_onLadder && _ladderHitCount == 0) ToggleClimbingLadders(false);

            // Snap to center of ladder
            if (_onLadder && _frameInput.Move.x == 0 && _stats.SnapToLadders && _hasControl) {
                var pos = _rb.position;
                _rb.position = Vector2.SmoothDamp(pos, new Vector2(_ladderHits[0].transform.position.x, pos.y), ref _ladderSnapVel, _stats.LadderSnapSpeed);
            }
        }

        private void ToggleClimbingLadders(bool on) {
            if (on) {
                _onLadder = true;
                _speed = Vector2.zero;
            }
            else {
                if (!_onLadder) return;
                _frameLeftLadder = _fixedFrame;
                _onLadder = false;
            }
        }

        #endregion

        #region Ledges

        private Vector2 _ledgeCornerPos;
        private bool _grabbingLedge;
        private bool _climbingLedge;

        protected virtual void HandleLedges() {
            if (_climbingLedge || !_isOnWall) return;

            _grabbingLedge = TryGetLedgeCorner(out _ledgeCornerPos);

            if (_grabbingLedge) HandleLedgeGrabbing();
        }
        
        protected virtual bool TryGetLedgeCorner(out Vector2 cornerPos) {
            cornerPos = Vector2.zero;
            Vector2 grabHeight = _rb.position + _stats.LedgeGrabPoint.y * Vector2.up;

            var hit1 = Physics2D.Raycast(grabHeight - _stats.LedgeRaycastSpacing * Vector2.up, _wallDir * Vector2.right, 0.5f, _stats.ClimbableLayer);
            if (!hit1.collider) return false; // Should hit below the ledge. Only used to determine xPos accurately

            var hit2 = Physics2D.Raycast(grabHeight + _stats.LedgeRaycastSpacing * Vector2.up, _wallDir * Vector2.right, 0.5f, _stats.ClimbableLayer);
            if (hit2.collider) return false; // we only are within ledge-grab range when the first hits and second doesn't

            var hit3 = Physics2D.Raycast(grabHeight + new Vector2(_wallDir * 0.5f, _stats.LedgeRaycastSpacing), Vector2.down, 0.5f, _stats.ClimbableLayer);
            if (!hit3.collider) return false; // gets our yPos of the corner
            
            cornerPos = new Vector2(hit1.point.x, hit3.point.y);
            return true;
        }

        protected virtual void HandleLedgeGrabbing() {
            // Snap to ledge position
            var xInput = _frameInput.Move.x;
            var yInput = _frameInput.Move.y;
            if (yInput != 0 && (xInput == 0 || Mathf.Sign(xInput) == _wallDir) && _hasControl) {
                var pos = _rb.position;
                var targetPos = _ledgeCornerPos - Vector2.Scale(_stats.LedgeGrabPoint, new(_wallDir, 1f));
                _rb.position = Vector2.MoveTowards(pos, targetPos, _stats.LedgeGrabDeceleration * Time.fixedDeltaTime);
            }

            // TODO: Create new stat variable instead of using Ladders or rename it to "vertical deadzone", "deadzone threshold", etc.
            if (yInput > _stats.LadderClimbThreshold)
                StartCoroutine(ClimbLedge());
        }

        protected virtual IEnumerator ClimbLedge() {
            LedgeClimbChanged?.Invoke(true);
            _climbingLedge = true;

            TakeAwayControl();
            var targetPos = _ledgeCornerPos - Vector2.Scale(_stats.LedgeGrabPoint, new(_wallDir, 1f));
            transform.position = targetPos;

            float lockedUntil = Time.time + _stats.LedgeClimbDuration;
            while (Time.time < lockedUntil)
                yield return new WaitForFixedUpdate();

            LedgeClimbChanged?.Invoke(false);
            _climbingLedge = false;
            _grabbingLedge = false;
            SetOnWall(false);

            targetPos = _ledgeCornerPos +  Vector2.Scale(_stats.StandUpOffset, new(_wallDir, 1f));
            transform.position = targetPos;
            ReturnControl();
        }

        #endregion

        #region Crouching

        private readonly Collider2D[] _crouchHits = new Collider2D[5];
        private int _frameStartedCrouching;
        private bool _crouching;

        protected virtual bool CrouchPressed => _frameInput.Move.y <= _stats.CrouchInputThreshold;

        protected virtual void HandleCrouching() {
            if (_crouching && _onLadder) SetCrouching(false); // use standing collider when on ladder
            else if (_crouching != CrouchPressed) SetCrouching(!_crouching);
        }

        protected virtual void SetCrouching(bool active) {
            if (!_crouching && (_onLadder || _isOnWall)) return; // Prevent crouching if climbing
            if (_crouching && !CanStandUp()) return; // Prevent standing into colliders

            _crouching = active;
            _col = _cols[active ? 1 : 0];
            _cols[0].enabled = !active;
            _cols[1].enabled = active;

            if (_crouching) _frameStartedCrouching = _fixedFrame;
        }

        protected bool CanStandUp() {
            var pos = _rb.position + (Vector2)_standingColliderBounds.center + new Vector2(0, _standingColliderBounds.extents.y);
            var size = new Vector2(_standingColliderBounds.size.x, _stats.CrouchBufferCheck);
            
            Physics2D.queriesHitTriggers = false;
            var hits = Physics2D.OverlapBoxNonAlloc(pos, size, 0, _crouchHits, ~_stats.PlayerLayer);
            Physics2D.queriesHitTriggers = _cachedTriggerSetting;

            return hits == 0;
        }

        #endregion

        #region Jump

        private bool _jumpToConsume;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private bool _doubleJumpUsable;
        private bool _bufferedJumpUsable;
        private int _frameJumpWasPressed = int.MinValue;

        private bool CanUseCoyote => _coyoteUsable && !_grounded && _fixedFrame < _frameLeftGrounded + _stats.CoyoteFrames;
        private bool HasBufferedJump => _bufferedJumpUsable && _fixedFrame < _frameJumpWasPressed + _stats.JumpBufferFrames;
        private bool CanDoubleJump => _doubleJumpUsable &&  _stats.AllowDoubleJump;

        protected virtual void HandleJump() {
            if (_jumpToConsume || HasBufferedJump) {
                if (_grounded || _onLadder || CanUseCoyote) NormalJump();
                else if (_isOnWall) WallJump();
                else if (_jumpToConsume && CanDoubleJump) DoubleJump();
            }
            
            _jumpToConsume = false; // Always consume the flag

            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true; // Early end detection
        }

        protected virtual void NormalJump() {
            _endedJumpEarly = false;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _doubleJumpUsable = true;
            ToggleClimbingLadders(false);
            _speed.y = _stats.JumpPower;
            Jumped?.Invoke(false);
        }

        protected virtual void WallJump() {
            _endedJumpEarly = false;
            _bufferedJumpUsable = false;
            _doubleJumpUsable = true; // note: double jump isn't currently refreshed after detaching from wall w/o jumping
            _currentWallJumpMoveMultiplier = 0;
            SetOnWall(false);
            _speed = Vector2.Scale(_stats.WallJumpPower, new(-_wallDir, 1));
            Jumped?.Invoke(true);
        }

        protected virtual void DoubleJump() {
            _endedJumpEarly = false;
            _doubleJumpUsable = false;
            _speed.y = _stats.JumpPower;
            DoubleJumped?.Invoke();
        }

        protected virtual void ResetJump() {
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _doubleJumpUsable = false;
            _endedJumpEarly = false;
        }

        #endregion

        #region Dash

        private bool _dashToConsume;
        private bool _canDash;
        private Vector2 _dashVel;
        private bool _dashing;
        private int _startedDashing;

        protected virtual void HandleDash() {
            if (_dashToConsume && _canDash && !_crouching) {
                var dir = new Vector2(_frameInput.Move.x, Mathf.Max(_frameInput.Move.y, 0f)).normalized;
                if (dir == Vector2.zero) {
                    _dashToConsume = false;
                    return;
                }

                _dashVel = dir * _stats.DashVelocity;
                _dashing = true;
                _canDash = false;
                _startedDashing = _fixedFrame;
                DashingChanged?.Invoke(true, dir);

                // Strip external buildup
                _currentExternalVelocity = Vector2.zero;
            }

            if (_dashing) {
                _speed = _dashVel;
                // Cancel when the time is out or we've reached our max safety distance
                if (_fixedFrame > _startedDashing + _stats.DashDurationFrames) {
                    _dashing = false;
                    DashingChanged?.Invoke(false, Vector2.zero);
                    if (_speed.y > 0) _speed.y = 0;
                    _speed.x *= _stats.DashEndHorizontalMultiplier;
                    if (_grounded) _canDash = true;
                }
            }

            _dashToConsume = false;
        }

        protected virtual void ResetDash() {
            _canDash = true;
        }

        #endregion

        #region Attacking

        private bool _attackToConsume;
        private int _frameLastAttacked = int.MinValue;

        protected virtual void HandleAttacking() {
            if (!_attackToConsume) return;

            if (_fixedFrame > _frameLastAttacked + _stats.AttackFrameCooldown) {
                _frameLastAttacked = _fixedFrame;
                Attacked?.Invoke();
            }

            _attackToConsume = false;
        }

        #endregion

        #region Horizontal

        protected virtual void HandleHorizontal() {
            if (_dashing) return;
            
            if (_frameInput.Move.x != 0) {
                if (_crouching && _grounded) {
                    var crouchPoint = Mathf.InverseLerp(0, _stats.CrouchSlowdownFrames, _fixedFrame - _frameStartedCrouching);
                    var diminishedMaxSpeed = _stats.MaxSpeed * Mathf.Lerp(1, _stats.CrouchSpeedPenalty, crouchPoint);

                    _speed.x = Mathf.MoveTowards(_speed.x, diminishedMaxSpeed * _frameInput.Move.x, _stats.GroundDeceleration * Time.fixedDeltaTime);
                }
                else {
                    // Prevent useless horizontal speed buildup when against a wall
                    if (_wallHitCount > 0 && Mathf.Approximately(_rb.velocity.x, 0) && Mathf.Sign(_frameInput.Move.x) == Mathf.Sign(_speed.x))
                        _speed.x = 0;

                    var inputX = _frameInput.Move.x * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
                    _speed.x = Mathf.MoveTowards(_speed.x, inputX * _stats.MaxSpeed, _currentWallJumpMoveMultiplier * _stats.Acceleration * Time.fixedDeltaTime);
                }
            }
            else
                _speed.x = Mathf.MoveTowards(_speed.x, 0, (_grounded ? _stats.GroundDeceleration : _stats.AirDeceleration) * Time.fixedDeltaTime);
        }

        #endregion

        #region Vertical

        private Vector2 _groundNormal;

        protected virtual void HandleVertical() {
            if (_dashing) return;

            // Ladder
            if (_onLadder) {
                var inputY = _frameInput.Move.y;
                _speed.y = inputY * (inputY > 0 ? _stats.LadderClimbSpeed : _stats.LadderSlideSpeed);

                return;
            }

            // Grounded & Slopes
            if (_grounded && _speed.y <= 0f) {
                _speed.y = _stats.GroundingForce;

                // We use a raycast here as the groundHits from capsule cast act a bit weird.
                Physics2D.queriesHitTriggers = false;
                var hit = Physics2D.Raycast(transform.position, Vector2.down, _stats.GrounderDistance * 2, ~_stats.PlayerLayer);
                Physics2D.queriesHitTriggers = _cachedTriggerSetting;
                if (hit.collider != null) {
                    _groundNormal = hit.normal;

                    if (!Mathf.Approximately(_groundNormal.y, 1f)) { // on a slope
                        _speed.y = _speed.x * -_groundNormal.x / _groundNormal.y;
                        if (_speed.x != 0) _speed.y += _stats.GroundingForce;
                    }
                }
                else
                    _groundNormal = Vector2.zero;

                return;
            }

            // Wall Climbing & Sliding
            if (_isOnWall) {
                if (_frameInput.Move.y > 0) _speed.y = _stats.WallClimbSpeed;
                else if (_frameInput.Move.y < 0) _speed.y = -_stats.MaxWallFallSpeed; // TODO: new stat variable for better feel?
                else if (_grabbingLedge) _speed.y = Mathf.MoveTowards(_speed.y, 0, _stats.LedgeGrabDeceleration * Time.fixedDeltaTime);
                else _speed.y = Mathf.MoveTowards(Mathf.Min(_speed.y, 0), -_stats.MaxWallFallSpeed, _stats.WallFallAcceleration * Time.fixedDeltaTime);

                return;
            }

            // In Air
            var fallSpeed = _stats.FallAcceleration;
            if (_endedJumpEarly && _speed.y > 0) fallSpeed *= _stats.JumpEndEarlyGravityModifier;
            _speed.y = Mathf.MoveTowards(_speed.y, -_stats.MaxFallSpeed, fallSpeed * Time.fixedDeltaTime);
        }

        #endregion

        protected virtual void ApplyVelocity() {
            if (!_hasControl) return;
            _rb.velocity = _speed + _currentExternalVelocity;

            _currentExternalVelocity = Vector2.MoveTowards(_currentExternalVelocity, Vector2.zero, _stats.ExternalVelocityDecay * Time.fixedDeltaTime);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (_stats.ShowWallDetection) {
                Gizmos.color = Color.white;
                var bounds = GetWallDetectionBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            if (_stats.ShowLedgeDetection) {
                Gizmos.color = Color.red;
                var facingDir = Mathf.Sign(_wallDir);
                var grabHeight = transform.position + _stats.LedgeGrabPoint.y * Vector3.up;
                var grabPoint = grabHeight + facingDir * _stats.LedgeGrabPoint.x * Vector3.right;
                Gizmos.DrawWireSphere(grabPoint, 0.05f);
                Gizmos.DrawWireSphere(grabPoint + Vector3.Scale(_stats.StandUpOffset, new(facingDir, 1)), 0.05f);
                Gizmos.DrawRay(grabHeight - _stats.LedgeRaycastSpacing * Vector3.up, 0.5f * facingDir * Vector3.right);
                Gizmos.DrawRay(grabHeight + _stats.LedgeRaycastSpacing * Vector3.up, 0.5f * facingDir * Vector3.right);
            }
        }
#endif
    }

    public interface IPlayerController {
        /// <summary>
        /// true = Landed. false = Left the Ground. float is Impact Speed
        /// </summary>
        public event Action<bool, float> GroundedChanged;
        public event Action<bool, Vector2> DashingChanged; // Dashing - Dir
        public event Action<bool> WallGrabChanged;
        public event Action<bool> LedgeClimbChanged;
        public event Action<bool> Jumped; // Is wall jump
        public event Action DoubleJumped;
        public event Action Attacked;

        public ScriptableStats PlayerStats { get; }
        public Vector2 Input { get; }
        public Vector2 Speed { get; }
        public Vector2 GroundNormal { get; }
        public int WallDirection { get; }
        public bool Crouching { get; }
        public bool ClimbingLadder { get; }
        public bool GrabbingLedge { get; }
        public bool ClimbingLedge { get; }
        public void ApplyVelocity(Vector2 vel, PlayerForce forceType);
    }

    public enum PlayerForce {
        /// <summary>
        /// Added directly to the players movement speed, to be controlled by the standard deceleration
        /// </summary>
        Burst,

        /// <summary>
        /// An additive force handled by the decay system
        /// </summary>
        Decay
    }
}