using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SplitVirtualPad : MonoBehaviour
{
  public PlayerController player;

  public TextMeshProUGUI leftPadText;
  public TextMeshProUGUI rightPadText;

  public float joyStickMaxTravel = 250f;
  public float baseSpeed = 0.3f;
  private Touch leftTouch, rightTouch;
  private Vector2 joyStickTouchStartPosition, joyStickTouchEndPosition;
  private Vector2 actionTouchStartPosition, actionTouchEndPosition;
  private string direction;
  private string multiTouchActionInfo;
  private int maxTapCount;

  private bool shouldJump = false;

  void Start() {
    leftTouch.phase = TouchPhase.Canceled;
    rightTouch.phase = TouchPhase.Canceled;
  }

  // Update is called once per frame
  void Update()
  {
    if(Input.touchCount > 0) {
      for(int i = 0; i < Input.touchCount; i++) {
        Touch currentTouch = Input.GetTouch(i);
        if (currentTouch.position.x < Screen.width / 2) {
          leftTouch = currentTouch;
        } else {
          rightTouch = currentTouch;
        }
      }

      if(leftTouch.phase == TouchPhase.Began) {
        joyStickTouchStartPosition = leftTouch.position;
      } else if(leftTouch.phase == TouchPhase.Moved || leftTouch.phase == TouchPhase.Ended) {
        float currentTouchX = leftTouch.position.x;
        float joyStickSensitivity = Mathf.Clamp(Mathf.Abs(joyStickTouchStartPosition.x - currentTouchX), 0, joyStickMaxTravel)/joyStickMaxTravel;
        joyStickTouchEndPosition = leftTouch.position;

        float x = joyStickTouchEndPosition.x - joyStickTouchStartPosition.x;

        if (x > 0) {
          direction = "Right";
        //   player.move.x = baseSpeed * joyStickSensitivity;
        } else if (x < 0) {
          direction = "Left";
        //   player.move.x = -baseSpeed * joyStickSensitivity;
        } else {
          direction = "Tapped";
        }
      }

      if (rightTouch.phase == TouchPhase.Began) {
        actionTouchStartPosition = rightTouch.position;
        if (player.IsGrounded()) {
          shouldJump = true;
        }
      } else if (rightTouch.phase == TouchPhase.Moved || rightTouch.phase == TouchPhase.Ended) {
        actionTouchEndPosition = rightTouch.position;

        float x = actionTouchEndPosition.x - actionTouchStartPosition.x;
        float y = actionTouchEndPosition.y - actionTouchStartPosition.y;

        if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0) {
          multiTouchActionInfo = "Tapped";
        }
        if (Mathf.Abs(y) > Mathf.Abs(x)) {
          if (y > 0) {
            multiTouchActionInfo = "Up";

            if (shouldJump && player.IsGrounded()) {
              player.Jump();
              // player.jumpState = PlayerController.JumpState.PrepareToJump;
              shouldJump = false;
            }
            // player.direction = 2;
          } else {
            multiTouchActionInfo = "Down";
            // player.direction = -2;
          }
        } else {
          if (x > 0) {
            multiTouchActionInfo = "Right";
            // player.direction = 1;
            
          } else {
            multiTouchActionInfo = "Left";
            // player.direction = -1;
          }
        }

        
      }

      if (leftTouch.phase == TouchPhase.Ended || leftTouch.phase == TouchPhase.Canceled) {
        direction = "Stopped";
        // player.move.x = 0;
      }
      if (rightTouch.phase == TouchPhase.Ended) {
        multiTouchActionInfo = "Ended";
      }
    }

    // multiTouchActionInfo = string.Format("Max tap count: {0}\n", maxTapCount);

    // if (Input.touchCount > 0) {
    //   for (int i = 0; i < Input.touchCount; i++) {
    //     theTouch = Input.GetTouch(i);

    //     multiTouchActionInfo += string.Format("Touch {0} - Position {1} - Tap Count: {2} - Finger ID: {3}\nRadius {4} ({5}%)\n", i, theTouch.position, theTouch.tapCount, theTouch.fingerId, theTouch.radius, ((theTouch.radius/(theTouch.radius + theTouch.radiusVariance)) * 100f).ToString("F1"));

    //     if (theTouch.tapCount > maxTapCount) {
    //       maxTapCount = theTouch.tapCount;
    //     }
    //   }
    // }
    rightPadText.text = multiTouchActionInfo;

    leftPadText.text = direction;
  }
}
