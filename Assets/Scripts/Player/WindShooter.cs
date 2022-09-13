using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindShooter : MonoBehaviour
{
  [SerializeField] private Transform rightPoint;
  [SerializeField] private Transform leftPoint;
  [SerializeField] private Transform topPoint;
  [SerializeField] private Transform bottomPoint;
  [SerializeField] private GameObject windPrefab;

  private PlayerController player;
  
  private InputManager inputManager;

  void Awake() {
    inputManager = InputManager.Instance;
  }

  void OnEnable() {
    inputManager.OnThrowWind += ThrowWind;
  }

  void Start() {
    player = GetComponent<PlayerController>();
  }

  public void ThrowWind(Direction windDirection) {
    if (windDirection == Direction.Stationary) return;

    if (player.isGrounded) {  
      if (windDirection == Direction.Left)
      {
        if (player.IsFacingLeft()) {
          Shoot(leftPoint, (int) windDirection);
        } else {
          Shoot(rightPoint, (int) windDirection);
        }
      } else if (windDirection == Direction.Right)
      {
        if (!player.IsFacingLeft()) {
          Shoot(rightPoint, (int) windDirection);
        } else {
          Shoot(leftPoint, (int) windDirection);
        }
      } else if (windDirection == Direction.Up)
      {
        Shoot(topPoint, (int) windDirection);
      } else if (windDirection == Direction.Down)
      {
        Shoot(bottomPoint, (int) windDirection);
      }
    }
  }

  // Update is called once per frame
  void Update() {
    
  }

  void Shoot(Transform point, int direction) {
    Wind wind = Instantiate(windPrefab, point.position, point.rotation).GetComponent<Wind>();
    wind.Shoot(direction);
  }
}
