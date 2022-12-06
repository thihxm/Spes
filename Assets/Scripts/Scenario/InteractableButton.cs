using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenario
{
  public class InteractableButton : MonoBehaviour
  {
    [SerializeField] private GameObject keyObject;
    [SerializeField] private GameObject[] doorObjects;
    private List<IInteractableObject> doors = new List<IInteractableObject>();

    private Collider2D buttonCollider;
    [SerializeField] public Bounds KeyDetector = new(Vector3.zero, Vector2.one);
    private readonly Collider2D[] buttonHits = new Collider2D[5];
    private int buttonHitCount;

    [SerializeField] private bool isActive = false;

    private void Awake()
    {
      foreach (var doorObject in doorObjects)
      {
        doors.Add(doorObject.GetComponent<IInteractableObject>());
      }
    }

    void Update()
    {
      HandleCollision();
      if (doors.Count > 0)
      {
        if ((isActive && !doors[0].IsActive) || (!isActive && doors[0].IsActive))
        {
          foreach (var door in doors)
          {
            door.Toggle();
          }
        }
      }
    }

    private void HandleCollision()
    {
      var bounds = GetKeyDetectionBounds();
      buttonHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, buttonHits);

      if (buttonHitCount > 0)
      {
        bool hasKey = false;
        foreach (var hit in buttonHits)
        {
          if (hit != null && hit.gameObject.Equals(keyObject))
          {
            hasKey = true;
            break;
          }
        }

        isActive = hasKey;
      }
      else
      {
        isActive = false;
      }
    }

    private Bounds GetKeyDetectionBounds()
    {
      var colliderOrigin = transform.position + KeyDetector.center;
      return new Bounds(colliderOrigin, KeyDetector.size);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
      Gizmos.color = Color.white;
      var bounds = GetKeyDetectionBounds();
      Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
#endif
  }
}
