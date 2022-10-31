using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenario
{
  public interface IInteractableObject
  {
    public bool IsActive { get; }
    public void Toggle();
  }
}
