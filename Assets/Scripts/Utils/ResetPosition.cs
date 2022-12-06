using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
  public class ResetPosition : MonoBehaviour
  {
    [SerializeField] private float minYPosition;
    [SerializeField] private Transform target;

    private void Update()
    {
      if (target.position.y < minYPosition)
        target.position = transform.position;
    }
  }
}
