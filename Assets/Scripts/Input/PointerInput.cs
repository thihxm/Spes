using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Simple object to contain information for drag inputs.
/// </summary>
public struct PointerInput
{
    public bool Contact;

    /// <summary>
    /// Position of draw input.
    /// </summary>
    public Vector2 Position;
}

// What we do in PointerInputManager is to simply create a separate action for each input we need for PointerInput.
// This here shows a possible alternative that sources all inputs as a single value using a composite. Has pros
// and cons. Biggest pro is that all the controls actuate together and deliver one input value.
//
// NOTE: In PointerControls, we are binding mouse and pen separately from touch. If we didn't care about multitouch,
//       we wouldn't have to to that but could rather just bind `<Pointer>/position` etc. However, to source each touch
//       as its own separate PointerInput source, we need to have multiple PointerInputComposites.
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class PointerInputComposite : InputBindingComposite<PointerInput>
{
  [InputControl(layout = "Button")]
  public int contact;

  [InputControl(layout = "Vector2")]
  public int position;

  public override PointerInput ReadValue(ref InputBindingCompositeContext context)
  {
    var contact = context.ReadValueAsButton(this.contact);
    var position = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.position);

    return new PointerInput
    {
      Contact = contact,
      Position = position,
    };
  }

  #if UNITY_EDITOR
  static PointerInputComposite()
  {
    Register();
  }

  #endif

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Register()
  {
    InputSystem.RegisterBindingComposite<PointerInputComposite>();
  }
}

