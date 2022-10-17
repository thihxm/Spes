using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
  public class WindShooter : MonoBehaviour
  {
    [SerializeField] private Transform frontPoint;
    [SerializeField] private Transform backPoint;
    [SerializeField] private Transform topPoint;
    [SerializeField] private GameObject windPrefab;

    private PlayerController player;
    private bool grounded = false;

    private InputManager inputManager;

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

    private void OnGroundedChanged(bool grounded, float impactForce)
    {
      this.grounded = grounded;
    }

    public void ThrowWind(Vector2 swipeDelta)
    {
      if (!grounded) return;

      Debug.Log("ThrowWind");

      var windDirection = GetDirection(swipeDelta);
      if (windDirection == Direction.Stationary) return;

      if (windDirection == Direction.Left)
      {
        if (player.FacingRight)
        {
          Shoot(backPoint, (int)windDirection);
        }
        else
        {
          Shoot(frontPoint, (int)windDirection);
        }
      }
      else if (windDirection == Direction.Right)
      {
        if (player.FacingRight)
        {
          Shoot(frontPoint, (int)windDirection);
        }
        else
        {
          Shoot(backPoint, (int)windDirection);
        }
      }
      else if (windDirection == Direction.Up)
      {
        Shoot(topPoint, (int)windDirection);
      }
    }

    void Shoot(Transform point, int direction)
    {
      Wind wind = Instantiate(windPrefab, point.position, point.rotation).GetComponent<Wind>();
      wind.Shoot(direction);
    }

    private Direction GetDirection(Vector2 windDelta)
    {
      float x = windDelta.x;
      float y = windDelta.y;
      Direction actionDirection;

      if (x == 0 && y == 0)
      {
        return Direction.Stationary;
      }

      if (Mathf.Abs(y) > Mathf.Abs(x))
      {
        if (y > 0)
        {
          actionDirection = Direction.Up;
        }
        else
        {
          actionDirection = Direction.Down;
        }
      }
      else
      {
        if (x > 0)
        {
          actionDirection = Direction.Right;
        }
        else
        {
          actionDirection = Direction.Left;
        }
      }

      return actionDirection;
    }
  }
}
