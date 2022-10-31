using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenario
{
  public class InteractableBox : MonoBehaviour, IInteractableObject
  {
    public bool IsActive => isActive;

    private bool isActive = false;
    private Collider2D boxCollider;

    private void Awake()
    {
      boxCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
      if (other.CompareTag("Wind"))
      {
        isActive = true;
      }
    }

    public void Toggle()
    {
      isActive = !isActive;
    }
  }
}
