using UnityEngine;

namespace TarodevController {
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class PlatformBase : MonoBehaviour {
        [Tooltip("If velocity is above this threshold the platform will not affect the player")] 
        [SerializeField] private float _unlockThreshold = 2.5f;

        private Rigidbody2D _player;
        protected Rigidbody2D _rb;
        protected Vector2 _startPos;
        protected Vector2 _lastPos;

        protected virtual void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _startPos = _rb.position;
        }

        protected virtual void FixedUpdate() {
            var newPos = _rb.position;
            var change = newPos - _lastPos;
            _lastPos = newPos;

            MovePlayer(change);
        }

        protected virtual void OnCollisionEnter2D(Collision2D col) {
            if (col.transform.TryGetComponent(out IPlayerController _))
            {
                var normal = col.GetContact(0).normal;
                if (Vector2.Dot(normal, Vector2.down) > 0.5f) // player is on top
                    _player = col.transform.GetComponent<Rigidbody2D>();
            }
        }

        protected virtual void OnCollisionExit2D(Collision2D col) {
            if (col.transform.TryGetComponent(out IPlayerController _))
                _player = null;
        }

        protected virtual void MovePlayer(Vector2 change) {
            if (!_player || _player.velocity.magnitude >= _unlockThreshold) return;
            
            _player.MovePosition(_player.position + change);
        }
    }
}