using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SplitVirtualPad : MonoBehaviour
{
  public PlayerController player;

  public TextMeshProUGUI leftPadText;
  public TextMeshProUGUI rightPadText;

  private float joyStickMaxTravel = 250f;
  public float baseSpeed = 0.8f;
  private Touch leftTouch, rightTouch;
  private Vector2 joyStickTouchStartPosition, joyStickTouchEndPosition;
  private Vector2 actionTouchStartPosition, actionTouchEndPosition;
  private string direction;
  private string multiTouchActionInfo;
  private int maxTapCount;

  private float lastX;
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
          direction = "Tapped";
        } else {
          if (Mathf.Abs(x) >= Mathf.Abs(lastX) - 10) {
            if (x > 0) {
              direction = "Right";
              player.inputX = joyStickSensitivity;
            } else if (x < 0) {
              direction = "Left";
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
        direction = "Stopped";
        player.inputX = 0;
      }


      if (rightTouch.phase == TouchPhase.Began) {
        actionTouchStartPosition = rightTouch.position;
      } else if (rightTouch.phase == TouchPhase.Moved || rightTouch.phase == TouchPhase.Ended) {
        actionTouchEndPosition = rightTouch.position;
        float joyStickSensitivity = Mathf.Clamp(Mathf.Abs(actionTouchStartPosition.y - actionTouchEndPosition.y), 0, joyStickMaxTravel) / joyStickMaxTravel;

        float x = actionTouchEndPosition.x - actionTouchStartPosition.x;
        float y = actionTouchEndPosition.y - actionTouchStartPosition.y;

        if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0) {
          multiTouchActionInfo = "Tapped";

          player.actionDirection = PlayerController.Direction.Up;
        } else {
          if (Mathf.Abs(y) > Mathf.Abs(x)) {
            if (y > 0) {
              multiTouchActionInfo = "Up";
              player.actionDirection = PlayerController.Direction.Tap;

              player.inputY = joyStickSensitivity;
            } else {
              multiTouchActionInfo = "Down";
              player.actionDirection = PlayerController.Direction.Down;
            }
          } else {
            if (x > 0) {
              multiTouchActionInfo = "Right";
              player.actionDirection = PlayerController.Direction.Right;
            } else {
              multiTouchActionInfo = "Left";
              player.actionDirection = PlayerController.Direction.Left;
            }
          }
        }
      }

      if (rightTouch.phase == TouchPhase.Ended || rightTouch.phase == TouchPhase.Canceled) {
        // multiTouchActionInfo = "Ended";
        player.inputY = 0;
        player.lastActionDirection = player.actionDirection;
        player.shouldJump = player.actionDirection == PlayerController.Direction.Up;
        actionTouchStartPosition = Vector2.zero;
        actionTouchEndPosition = Vector2.zero;
        
        player.actionDirection = PlayerController.Direction.Stationary;
      }
    }

    rightPadText.text = multiTouchActionInfo;

    leftPadText.text = direction;
  }


  public IEnumerator ResetTouchState() {
    yield return new WaitForSeconds(0.2f);
    player.lastActionDirection = PlayerController.Direction.Stationary;
  }

  public static void GetAxisRaw() {

  }
}
