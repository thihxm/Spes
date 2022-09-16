using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aarthificial.Reanimation;

namespace Player {
  public class PlayerRenderer : MonoBehaviour {
    private static class Drivers {
      public const string JumpState = "jumpState";
      public const string IsGrounded = "isGrounded";
      public const string State = "state";
      public const string IsMoving = "isMoving";
      public const string Falling = "falling";
    }

    private Reanimator reanimator;
    private PlayerController controller;

    private void Awake() {
      reanimator = GetComponent<Reanimator>();
      controller = GetComponent<PlayerController>();
    }

    private void OnEnable() {}

    private void OnDisable() {}

    private void Update() {
      bool isMoving = Mathf.Abs(controller.inputX) > 0;

      // reanimator.Flip = controller.facingLeft;
      reanimator.Set(Drivers.IsMoving, isMoving);
      reanimator.Set(Drivers.IsGrounded, controller.isGrounded);
      reanimator.Set(Drivers.JumpState, (int) controller.jumpState);
      

      if (controller.jumpState == PlayerController.JumpState.Falling && reanimator.State.Get(Drivers.Falling, -1) == -1) {
        reanimator.Set(Drivers.Falling, 0);
      }

      bool didLandInThisFrame = reanimator.WillChange(Drivers.IsGrounded, true);

      if (didLandInThisFrame) {
        reanimator.ForceRerender();
      }
    }
  }
}
