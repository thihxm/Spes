using System;
using UnityEngine;

namespace Player
{
  public class WindShooter : MonoBehaviour
  {
    [SerializeField] private Transform frontPoint;
    [SerializeField] private Transform backPoint;
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform windPointsParent;
    [SerializeField] private GameObject windPrefab;

    [SerializeField] private ParticleSystem windRingParticles;
    [SerializeField] private ParticleSystem windSmallRingParticles;
    [SerializeField] private Transform windRingTransform;

    private PlayerController player;
    private bool grounded = false;

    private InputManager inputManager;

    [SerializeField] private float tiltChangeSpeed = .05f;
    private Vector2 tiltVelocity;

    public event Action OnWindShoot;

    void Awake()
    {
      inputManager = InputManager.Instance;
      player = PlayerController.Instance;
    }

    void OnEnable()
    {
      inputManager.OnThrowWind += ThrowWind;
      player.GroundedChanged += OnGroundedChanged;
    }

    void OnDisable()
    {
      inputManager.OnThrowWind -= ThrowWind;
      player.GroundedChanged -= OnGroundedChanged;
    }

    void Update()
    {
      HandleGroundEffects();
    }

    private void OnGroundedChanged(bool grounded, float impactForce)
    {
      this.grounded = grounded;
    }

    public void ThrowWind(Vector2 swipeDelta)
    {
      if (!grounded) return;

      var windDirection = GetDirection(swipeDelta);

      if (windDirection == Vector2.zero || windDirection == Vector2.down) return;

      OnWindShoot?.Invoke();

      windRingTransform.up = windDirection;
      windRingParticles.Play();
      windSmallRingParticles.Play();

      if (windDirection == Vector2.left)
      {
        if (player.FacingRight)
        {
          windRingTransform.position = backPoint.position;
          Shoot(backPoint, windDirection);
        }
        else
        {
          windRingTransform.position = frontPoint.position;
          Shoot(frontPoint, windDirection);
        }
      }
      else if (windDirection == Vector2.right)
      {
        if (player.FacingRight)
        {
          windRingTransform.position = frontPoint.position;
          Shoot(frontPoint, windDirection);
        }
        else
        {
          windRingTransform.position = backPoint.position;
          Shoot(backPoint, windDirection);
        }
      }
      else if (windDirection == Vector2.up)
      {
        windRingTransform.position = topPoint.position;
        Shoot(topPoint, windDirection);
      }
    }

    void Shoot(Transform origin, Vector2 direction)
    {
      Wind wind = Instantiate(windPrefab, origin.position, origin.rotation).GetComponent<Wind>();
      wind.Shoot(direction, player.Speed.x);
    }

    private Vector2 GetDirection(Vector2 windDelta)
    {
      float x = windDelta.x;
      float y = windDelta.y;
      Vector2 actionDirection;

      if (x == 0 && y == 0)
      {
        return Vector2.zero;
      }

      if (Mathf.Abs(y) > Mathf.Abs(x))
      {
        if (y > 0)
        {
          actionDirection = Vector2.up;
        }
        else
        {
          actionDirection = Vector2.down;
        }
      }
      else
      {
        if (x > 0)
        {
          actionDirection = Vector2.right;
        }
        else
        {
          actionDirection = Vector2.left;
        }
      }

      return actionDirection;
    }

    private void HandleGroundEffects()
    {
      // Tilt with slopes
      windPointsParent.transform.up = Vector2.SmoothDamp(windPointsParent.transform.up, grounded ? player.GroundNormal : Vector2.up, ref tiltVelocity, tiltChangeSpeed);

      frontPoint.transform.up = Vector2.SmoothDamp(frontPoint.transform.up, grounded ? player.GroundNormal : Vector2.up, ref tiltVelocity, tiltChangeSpeed);

      backPoint.transform.up = Vector2.SmoothDamp(backPoint.transform.up, grounded ? player.GroundNormal : Vector2.up, ref tiltVelocity, tiltChangeSpeed);

      topPoint.transform.up = Vector2.SmoothDamp(topPoint.transform.up, grounded ? player.GroundNormal : Vector2.up, ref tiltVelocity, tiltChangeSpeed);
    }
  }
}
