using UnityEngine;

namespace TarodevController {
    public class Bouncer : MonoBehaviour {
        [SerializeField] private float _bounceForce = 70;

        private void OnCollisionStay2D(Collision2D other) {
            if (other.collider.TryGetComponent(out IPlayerController controller)) controller.ApplyVelocity(transform.up * _bounceForce, PlayerForce.Burst);
        }
    }
}