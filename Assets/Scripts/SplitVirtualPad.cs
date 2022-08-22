using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public interface IObserver
{
  void Trigger(ISubject subject);
}

public interface ISubject
{
  void Attach(IObserver observer);
  void Detach(IObserver observer);
  void Notify();
}

public class SplitVirtualPad : MonoBehaviour, ISubject
{
  public PlayerController player;

  public TextMeshProUGUI leftPadText;
  public TextMeshProUGUI rightPadText;

  private float joyStickMaxTravel = 250f;
  public float baseSpeed = 0.8f;
  private Touch leftTouch, rightTouch;
  private Vector2 joyStickTouchStartPosition, joyStickTouchEndPosition;
  private Vector2 actionTouchStartPosition, actionTouchEndPosition;
  private string debugLeftSideInfo;
  private string debugRightSideInfo;

  private float lastX;

  private bool shouldCheckAction = false;

  public Direction actionDirection = Direction.Stationary;

  #region Observer Logic
  private List<IObserver> _observers = new List<IObserver>();

  public void Attach(IObserver observer) {
    this._observers.Add(observer);
  }

  public void Detach(IObserver observer) {
    this._observers.Remove(observer);
  }

  public void Notify() {
    foreach (IObserver observer in this._observers) {
      observer.Trigger(this);
    }
  }
  #endregion
  
  void Start() {
    leftTouch.phase = TouchPhase.Canceled;
    rightTouch.phase = TouchPhase.Canceled;
  }

  // Update is called once per frame
  void Update()
  {
    if(Input.touchCount > 0) {
      for (int i = 0; i < Input.touchCount; i++) {
        Touch currentTouch = Input.GetTouch(i);
        if (currentTouch.position.x < Screen.width / 2) {
          leftTouch = currentTouch;
        } else {
          rightTouch = currentTouch;
        }
      }

      if (leftTouch.phase == TouchPhase.Began) {
        joyStickTouchStartPosition = leftTouch.position;
      } else if (leftTouch.phase == TouchPhase.Moved || leftTouch.phase == TouchPhase.Ended) {
        joyStickTouchEndPosition = leftTouch.position;
        float joyStickSensitivity = Mathf.Clamp(Mathf.Abs(joyStickTouchStartPosition.x - joyStickTouchEndPosition.x), 0, joyStickMaxTravel) / joyStickMaxTravel;

        float x = joyStickTouchEndPosition.x - joyStickTouchStartPosition.x;
        float y = joyStickTouchEndPosition.y - joyStickTouchStartPosition.y;

        if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0) {
          debugLeftSideInfo = "Tapped";
        } else {
          if (Mathf.Abs(x) >= Mathf.Abs(lastX) - 10) {
            if (x > 0) {
              debugLeftSideInfo = "Right";
              player.inputX = joyStickSensitivity;
            } else if (x < 0) {
              debugLeftSideInfo = "Left";
              player.inputX = -joyStickSensitivity;
            }
          } else {
            Vector2 auxJoyStickTouchStartPosition = joyStickTouchStartPosition;
            joyStickTouchStartPosition = joyStickTouchEndPosition;
            joyStickTouchEndPosition = auxJoyStickTouchStartPosition;
          } 
        }

        lastX = x;
      }

      if (leftTouch.phase == TouchPhase.Ended || leftTouch.phase == TouchPhase.Canceled) {
        debugLeftSideInfo = "Stopped";
        player.inputX = 0;
      }


      if (rightTouch.phase == TouchPhase.Began) {
        actionTouchStartPosition = rightTouch.position;
        shouldCheckAction = true;
      } else if (rightTouch.phase == TouchPhase.Moved || rightTouch.phase == TouchPhase.Ended) {
        if (shouldCheckAction) {

          actionTouchEndPosition = rightTouch.position;
          float joyStickSensitivity = Mathf.Clamp(Mathf.Abs(actionTouchStartPosition.y - actionTouchEndPosition.y), 0, joyStickMaxTravel) / joyStickMaxTravel;

          float x = actionTouchEndPosition.x - actionTouchStartPosition.x;
          float y = actionTouchEndPosition.y - actionTouchStartPosition.y;

          if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0) {
            debugRightSideInfo = "Tapped";
            actionDirection = Direction.Tap;

            player.actionDirection = PlayerController.Direction.Tap;
          } else {
            if (Mathf.Abs(y) > Mathf.Abs(x)) {
              if (y > 0) {
                debugRightSideInfo = "Up";
                player.actionDirection = PlayerController.Direction.Up;
                actionDirection = Direction.Up;

                player.inputY = joyStickSensitivity;
              } else {
                debugRightSideInfo = "Down";
                player.actionDirection = PlayerController.Direction.Down;
                actionDirection = Direction.Down;
              }
            } else {
              if (x > 0) {
                debugRightSideInfo = "Right";
                player.actionDirection = PlayerController.Direction.Right;
                actionDirection = Direction.Right;
              } else {
                debugRightSideInfo = "Left";
                player.actionDirection = PlayerController.Direction.Left;
                actionDirection = Direction.Left;
              }
            }
          }
        }
      }

      if (rightTouch.phase == TouchPhase.Ended || rightTouch.phase == TouchPhase.Canceled) {
        debugRightSideInfo = "Ended";
        player.inputY = 0;
        player.lastActionDirection = player.actionDirection;
        shouldCheckAction = false;
        this.Notify();
        
        if (player.isGrounded) {
          player.shouldJump = player.actionDirection == PlayerController.Direction.Tap;
        } else {
          bool isDashAction = player.actionDirection == PlayerController.Direction.Right || player.actionDirection == PlayerController.Direction.Left || player.actionDirection == PlayerController.Direction.Up || player.actionDirection == PlayerController.Direction.Down;

          player.shouldDash = isDashAction;
          switch (player.actionDirection)
          {
            case PlayerController.Direction.Right:
              player.dashDirection = Vector2.left;
              break;
            case PlayerController.Direction.Left:
              player.dashDirection = Vector2.right;
              break;
            case PlayerController.Direction.Up:
              player.dashDirection = Vector2.down;
              break;
            case PlayerController.Direction.Down:
              player.dashDirection = Vector2.up;
              break;
          }
        }


        actionTouchStartPosition = actionTouchEndPosition;
        
        player.actionDirection = PlayerController.Direction.Stationary;
        actionDirection = Direction.Stationary;
      }
    }

    rightPadText.text = debugRightSideInfo;

    leftPadText.text = debugLeftSideInfo;
  }


  public IEnumerator ResetTouchState() {
    yield return new WaitForSeconds(0.2f);
    player.lastActionDirection = PlayerController.Direction.Stationary;
  }

  public static void GetAxisRaw() {

  }


  public enum Direction {
    Left,
    Right,
    Up,
    Down,
    Tap,
    Stationary
  }
}
