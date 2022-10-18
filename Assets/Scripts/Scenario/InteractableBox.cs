using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenario
{
  public class InteractableBox : MonoBehaviour, IInteractableObject
  {
    public bool IsActive => active;

    private bool active = false;
    private Collider2D boxCollider;

    private void Awake()
    {
      boxCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
      if (other.CompareTag("Wind"))
      {
        active = true;
      }
    }
  }
}
