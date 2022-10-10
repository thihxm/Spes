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

    private InputManager inputManager;

    void Awake()
    {
      inputManager = InputManager.Instance;
    }

    // void OnEnable()
    // {
    //   inputManager.OnThrowWind += ThrowWind;
    // }

    void Start()
    {
      player = PlayerController.Instance;
    }

    // public void ThrowWind(Direction windDirection, Vector2 swipeDelta)
    // {
    //   if (windDirection == Direction.Stationary) return;

    //   if (player.Grounded)
    //   {
    //     if (windDirection == Direction.Left)
    //     {
    //       if (player.FacingRight)
    //       {
    //         Shoot(backPoint, (int)windDirection);
    //       }
    //       else
    //       {
    //         Shoot(frontPoint, (int)windDirection);
    //       }
    //     }
    //     else if (windDirection == Direction.Right)
    //     {
    //       if (player.FacingRight)
    //       {
    //         Shoot(frontPoint, (int)windDirection);
    //       }
    //       else
    //       {
    //         Shoot(backPoint, (int)windDirection);
    //       }
    //     }
    //     else if (windDirection == Direction.Up)
    //     {
    //       Shoot(topPoint, (int)windDirection);
    //     }
    //   }
    // }

    void Shoot(Transform point, int direction)
    {
      Wind wind = Instantiate(windPrefab, point.position, point.rotation).GetComponent<Wind>();
      wind.Shoot(direction);
    }
  }
}
