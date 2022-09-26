using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aarthificial.Reanimation;

namespace Player {
  public class PlayerRenderer : MonoBehaviour {
    private static class Drivers {
      public const string JumpState = "jumpState";
      public const string IsGrounded = "isGrounded";
      public const string IsDashing = "isDashing";
      public const string State = "state";
      public const string IsMoving = "isMoving";
      public const string Falling = "falling";
      public const string Climbing = "climbing";
    }

    public delegate void ClimbAction(int state);
    public event ClimbAction OnChangeClimbState;

    private Reanimator reanimator;
    private PlayerController controller;

    private void Awake() {
      reanimator = GetComponent<Reanimator>();
      controller = GetComponent<PlayerController>();
    }

    private void OnEnable() {
      reanimator.AddListener(Drivers.Climbing, OnClimbing);
    }

    private void OnDisable() {
      reanimator.RemoveListener(Drivers.Climbing, OnClimbing);
    }

    private void Update() {
      var velocity = controller.Velocity;
      bool isMoving = Mathf.Abs(controller.inputX) > 0 && Mathf.Abs(velocity.x) > 0.01f;

      // reanimator.Flip = controller.facingLeft;
      reanimator.Set(Drivers.IsMoving, isMoving);
      reanimator.Set(Drivers.IsGrounded, controller.isGrounded);
      reanimator.Set(Drivers.IsDashing, controller.isDashing);
      reanimator.Set(Drivers.JumpState, (int) controller.jumpState);
      
      // Debug.Log(reanimator.State.Get(Drivers.Falling, -1));
      // if (controller.jumpState == PlayerController.JumpState.Falling && reanimator.State.Get(Drivers.Falling, -1) == -1) {
      //   reanimator.Set(Drivers.Falling, 0);
      // }

      bool didLandInThisFrame = reanimator.WillChange(Drivers.IsGrounded, true);

      if (didLandInThisFrame) {
        reanimator.ForceRerender();
      }
    }

    private void OnClimbing() {
      int climbingState = reanimator.State.Get(Drivers.Climbing, 0);
      if (climbingState > 0) {
        OnChangeClimbState?.Invoke(climbingState);
      }
    }
  }
}
