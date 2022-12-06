using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
  public struct FrameInput
  {
    public Vector2 Move;
    public bool JumpTapped;
    public Vector2 Wind;
  }

  public interface IPlayerController
  {
    /// <summary>
    /// true = Landed. false = Left the Ground. float is Impact Speed
    /// </summary>
    public event Action<bool, float> GroundedChanged;
    public event Action<bool, Vector2> DashingChanged; // Dashing - Dir
    public event Action<bool> WallGrabChanged;
    public event Action<bool> LedgeClimbChanged;
    public event Action<bool> Jumped; // Is wall jump
    public event Action DoubleJumped;

    public ScriptableStats PlayerStats { get; }
    public Vector2 Input { get; }
    public Vector2 Speed { get; }
    public Vector2 GroundNormal { get; }
    public int WallDirection { get; }
    public bool Crouching { get; }
    public bool GrabbingLedge { get; }
    public bool FacingRight { get; }
    public bool ClimbingLedge { get; }
    public void ApplyVelocity(Vector2 vel, PlayerForce forceType);
    public bool ShouldFlip { get; }
    public bool Running { get; set; }
    public void Flip();

    public Collider2D BodyCollider { get; }
  }

  public enum PlayerForce
  {
    /// <summary>
    /// Added directly to the players movement speed, to be controlled by the standard deceleration
    /// </summary>
    Burst,

    /// <summary>
    /// An additive force handled by the decay system
    /// </summary>
    Decay
  }

  public enum JumpState
  {
    Grounded = 0,
    Jumping = 1,
    LedgeClimbing = 2,
    Falling = 3,
  }
}