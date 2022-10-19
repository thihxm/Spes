using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenario
{
  public class InteractableDoor : MonoBehaviour
  {
    [SerializeField] private GameObject buttonObject;
    private IInteractableObject button;

    [SerializeField] private bool IsOpen = false;
    [SerializeField] private Transform openPoint;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
      button = buttonObject.GetComponent<IInteractableObject>();
      closedPosition = transform.position;
      openPosition = openPoint.position;
    }

    void FixedUpdate()
    {

      if (button.IsActive && !IsOpen)
      {
        IsOpen = true;
      }
      else if (!button.IsActive && IsOpen)
      {
        Close();
      }

      if (IsOpen)
      {
        transform.position = Vector3.SmoothDamp(transform.position, openPosition, ref velocity, 0.5f);
      }
    }

    void Close()
    {
      IsOpen = false;
      transform.position = closedPosition;
    }
  }
}
