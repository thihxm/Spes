using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Input
{
  public class MoveJoystickController : MonoBehaviour
  {
    [SerializeField] private RectTransform joystickTransform;
    private Vector2 joystickStartPosition;
    private InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
      inputManager = InputManager.Instance;
      joystickStartPosition = joystickTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
      Camera.main.WorldToScreenPoint(transform.position);
      Vector2 touchPosition = inputManager.JoystickTouchInput.TouchPosition;
      var test = Rect.PointToNormalized(joystickTransform.rect, touchPosition);
      Debug.Log("touchPos: " + touchPosition + "; test: " + test);
      Debug.Log("contains? " + joystickTransform.rect.Contains(touchPosition));

      if (!inputManager.JoystickTouchInput.TouchEnded)
      {
        joystickTransform.position = touchPosition;
      }
      else
      {
        joystickTransform.position = joystickStartPosition;
      }
    }
  }

  public struct JoystickTouchInput
  {
    public Vector2 TouchPosition;
    public bool TouchStarted;
    public bool TouchEnded;
  }
}
