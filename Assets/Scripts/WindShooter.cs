using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindShooter : MonoBehaviour, IObserver
{
  [SerializeField] private Transform rightPoint;
  [SerializeField] private Transform leftPoint;
  [SerializeField] private Transform topPoint;
  [SerializeField] private Transform bottomPoint;
  [SerializeField] private GameObject windPrefab;

  private PlayerController player;

  void Start() {
    player = GetComponent<PlayerController>();
  }

  public void Trigger(ISubject subject)
  {
    SplitVirtualPad splitVirtualPad = (SplitVirtualPad) subject;

    if (player.isGrounded) {  
      if (splitVirtualPad.actionDirection == Direction.Left)
      {
        Shoot(leftPoint, (int) splitVirtualPad.actionDirection);
      } else if (splitVirtualPad.actionDirection == Direction.Right)
      {
        Shoot(rightPoint, (int) splitVirtualPad.actionDirection);
      } else if (splitVirtualPad.actionDirection == Direction.Up)
      {
        Shoot(topPoint, (int) splitVirtualPad.actionDirection);
      } else if (splitVirtualPad.actionDirection == Direction.Down)
      {
        Shoot(bottomPoint, (int) splitVirtualPad.actionDirection);
      }
    }
  }

  // Update is called once per frame
  void Update() {
    
  }

  void Shoot(Transform point, int direction) {
    Wind wind = Instantiate(windPrefab, point.position, point.rotation).GetComponent<Wind>();
    StartCoroutine(wind.Shoot(direction));
  }
}
