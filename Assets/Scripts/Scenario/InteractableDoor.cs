using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenario
{
  public class InteractableDoor : MonoBehaviour, IInteractableObject
  {
    [SerializeField] private bool isOpen = false;
    [SerializeField] private Transform openPoint;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 velocity = Vector3.zero;

    public bool IsActive => isOpen;

    [SerializeField] private float closingSmoothTime = 0.5f;

    private void Awake()
    {
      closedPosition = transform.position;
      openPosition = openPoint.position;
    }

    void Update()
    {
      if (isOpen)
      {
        transform.position = Vector3.SmoothDamp(transform.position, openPosition, ref velocity, closingSmoothTime);
      }
      else
      {
        transform.position = Vector3.SmoothDamp(transform.position, closedPosition, ref velocity, closingSmoothTime);
      }
    }

    public void Toggle()
    {
      isOpen = !isOpen;
    }
  }
}
