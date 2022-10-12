using UnityEngine;

namespace TarodevController {
    public class RadialPlatform : PlatformBase {
        [SerializeField] private float _speed = 1.5f;
        [SerializeField] private float _size = 2;

        protected override void FixedUpdate()
        {
            var newPos = _startPos + new Vector2(Mathf.Cos(Time.time * _speed), Mathf.Sin(Time.time * _speed)) * _size;
            _rb.MovePosition(newPos);

            base.FixedUpdate();
        }

        private void OnDrawGizmosSelected() {
            if (Application.isPlaying)
                Gizmos.DrawWireSphere(_startPos, _size);
            else
                Gizmos.DrawWireSphere(transform.position, _size);
        }
    }
}