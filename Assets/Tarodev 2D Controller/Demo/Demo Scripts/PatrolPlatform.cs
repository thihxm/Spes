using UnityEngine;

namespace TarodevController {
    public class PatrolPlatform : PlatformBase {
        [Tooltip("Local offsets from starting position")]
        [SerializeField] private Vector2[] _points = new Vector2[] { Vector2.zero, Vector2.right };
        [SerializeField] private float _speed = 1.5f;
        [SerializeField] private bool _looped;
        [SerializeField] private bool _ascending;

        private Vector2 Pos => _rb.position;
        private int _index = 0;

        protected override void FixedUpdate() {
            var target = _points[_index] + _startPos;
            var newPos = Vector2.MoveTowards(Pos, target, _speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            if (Pos == target) {
                if (_looped)
                    _index = (_ascending ? _index + 1 : _index + _points.Length - 1) % _points.Length;
                else { // ping-pong
                    if (_index >= _points.Length - 1){
                        _ascending = false;
                        _index--;
                    } 
                    else if (_index <= 0) {
                        _ascending = true;
                        _index++;
                    }
                    _index = Mathf.Clamp(_index, 0, _points.Length - 1);
                }
            }

            base.FixedUpdate();
        }

        private void OnDrawGizmosSelected() {
            var offset = Application.isPlaying ? _startPos : (Vector2)transform.position;
            
            var previous = _points[0] + offset;
            Gizmos.DrawWireSphere(previous, 0.2f);
            if (_looped) Gizmos.DrawLine(previous, _points[^1] + offset); // ^1 is last index, or _points.Length - 1
            
            for (var i = 1; i < _points.Length; i++) {
                var p = _points[i] + offset;
                Gizmos.DrawWireSphere(p, 0.2f);
                Gizmos.DrawLine(previous, p);

                previous = p;
            }
        }
    }
}