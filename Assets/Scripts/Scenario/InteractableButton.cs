using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenario
{
  public class InteractableButton : MonoBehaviour
  {
    [SerializeField] private GameObject keyObject;
    [SerializeField] private GameObject doorObject;
    private IInteractableObject door;

    private Collider2D buttonCollider;
    [SerializeField] public Bounds KeyDetector = new(Vector3.zero, Vector2.one);
    private readonly Collider2D[] buttonHits = new Collider2D[5];
    private int buttonHitCount;

    [SerializeField] private bool isActive = false;

    private void Awake()
    {
      door = doorObject.GetComponent<IInteractableObject>();
      Debug.Log(door);
    }

    void Update()
    {
      HandleCollision();
      if ((isActive && !door.IsActive) || (!isActive && door.IsActive))
      {
        door.Toggle();
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
