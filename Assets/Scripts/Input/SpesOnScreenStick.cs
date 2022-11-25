using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using Player;

namespace Input
{
  [AddComponentMenu("Input/Spes On-Screen Stick")]
  public class SpesOnScreenStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
  {
    [SerializeField] private ScriptableStats stats;
    private Vector2 lastJoystickPosition = Vector2.zero;
    public void OnPointerDown(PointerEventData eventData)
    {
      if (eventData == null)
        throw new System.ArgumentNullException(nameof(eventData));

      RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out m_PointerDownPos);

      lastJoystickPosition = m_PointerDownPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
      if (eventData == null)
        throw new System.ArgumentNullException(nameof(eventData));

      RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
      var delta = position - m_PointerDownPos;

      delta = Vector2.ClampMagnitude(delta, movementRange);
      ((RectTransform)transform).anchoredPosition = m_StartPos + (Vector3)delta;

      var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
      float allowedRange = Mathf.Abs(Mathf.Abs(newPos.x) - Mathf.Abs(lastJoystickPosition.x));
      if (allowedRange >= stats.ChangeDirectionThreshold)
      {
        SendValueToControl(Vector2.zero);
      }
      else
      {
        SendValueToControl(newPos);
      }
      lastJoystickPosition = newPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      ((RectTransform)transform).anchoredPosition = m_StartPos;
      SendValueToControl(Vector2.zero);
    }

    private void Start()
    {
      m_StartPos = ((RectTransform)transform).anchoredPosition;
    }

    public float movementRange
    {
      get => m_MovementRange;
      set => m_MovementRange = value;
    }

    [FormerlySerializedAs("movementRange")]
    [SerializeField]
    private float m_MovementRange = 50;

    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;

    private Vector3 m_StartPos;
    private Vector2 m_PointerDownPos;

    protected override string controlPathInternal
    {
      get => m_ControlPath;
      set => m_ControlPath = value;
    }
  }

  public struct JoystickTouchInput
  {
    public Vector2 TouchPosition;
    public bool TouchStarted;
    public bool TouchEnded;
  }
}
