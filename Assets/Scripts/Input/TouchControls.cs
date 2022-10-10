//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.2
//     from Assets/Scripts/Input/TouchControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @TouchControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @TouchControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""TouchControls"",
    ""maps"": [
        {
            ""name"": ""Touch"",
            ""id"": ""793eea36-6fa9-4fbe-9c2d-b05a14439f47"",
            ""actions"": [
                {
                    ""name"": ""Wind"",
                    ""type"": ""PassThrough"",
                    ""id"": ""f32893f3-d590-459d-aeb0-c0f141b2c744"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": ""NormalizeVector2"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""TouchTap"",
                    ""type"": ""PassThrough"",
                    ""id"": ""dac75d9a-c14a-4966-a4d6-5d15c52ef0e3"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""7946f68f-a8db-4169-a4f2-d415d0f78c79"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d662ee48-b540-4f64-98fd-05665aff89dc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Tap"",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Touch0"",
                    ""id"": ""ac4a609c-6fa9-45a7-aada-2dceeaaae455"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchTap"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""contact"",
                    ""id"": ""d423cadf-d8cb-4fbe-824f-f9af8edd8053"",
                    ""path"": ""<Touchscreen>/touch0/tap"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchTap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""position"",
                    ""id"": ""a7c8084a-96d2-4c84-b51b-1bdb3d509059"",
                    ""path"": ""<Touchscreen>/touch0/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchTap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch1"",
                    ""id"": ""34900cb0-0255-4378-94fb-ada0fda8f0d6"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchTap"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""contact"",
                    ""id"": ""3995c937-29be-41fd-9199-4ef1596792ad"",
                    ""path"": ""<Touchscreen>/touch1/tap"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchTap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""position"",
                    ""id"": ""82b44630-b350-4d31-abb4-3dd17a8f7e38"",
                    ""path"": ""<Touchscreen>/touch1/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchTap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""71ff2357-4c15-443a-b599-22343c5b2d08"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ce38c15a-6b89-455b-93c1-9cdbf99d752e"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Wind"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""96973496-26f7-4d3b-996a-6ef058e49f4c"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Touch
        m_Touch = asset.FindActionMap("Touch", throwIfNotFound: true);
        m_Touch_Wind = m_Touch.FindAction("Wind", throwIfNotFound: true);
        m_Touch_TouchTap = m_Touch.FindAction("TouchTap", throwIfNotFound: true);
        m_Touch_Move = m_Touch.FindAction("Move", throwIfNotFound: true);
        m_Touch_Jump = m_Touch.FindAction("Jump", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Touch
    private readonly InputActionMap m_Touch;
    private ITouchActions m_TouchActionsCallbackInterface;
    private readonly InputAction m_Touch_Wind;
    private readonly InputAction m_Touch_TouchTap;
    private readonly InputAction m_Touch_Move;
    private readonly InputAction m_Touch_Jump;
    public struct TouchActions
    {
        private @TouchControls m_Wrapper;
        public TouchActions(@TouchControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Wind => m_Wrapper.m_Touch_Wind;
        public InputAction @TouchTap => m_Wrapper.m_Touch_TouchTap;
        public InputAction @Move => m_Wrapper.m_Touch_Move;
        public InputAction @Jump => m_Wrapper.m_Touch_Jump;
        public InputActionMap Get() { return m_Wrapper.m_Touch; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TouchActions set) { return set.Get(); }
        public void SetCallbacks(ITouchActions instance)
        {
            if (m_Wrapper.m_TouchActionsCallbackInterface != null)
            {
                @Wind.started -= m_Wrapper.m_TouchActionsCallbackInterface.OnWind;
                @Wind.performed -= m_Wrapper.m_TouchActionsCallbackInterface.OnWind;
                @Wind.canceled -= m_Wrapper.m_TouchActionsCallbackInterface.OnWind;
                @TouchTap.started -= m_Wrapper.m_TouchActionsCallbackInterface.OnTouchTap;
                @TouchTap.performed -= m_Wrapper.m_TouchActionsCallbackInterface.OnTouchTap;
                @TouchTap.canceled -= m_Wrapper.m_TouchActionsCallbackInterface.OnTouchTap;
                @Move.started -= m_Wrapper.m_TouchActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_TouchActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_TouchActionsCallbackInterface.OnMove;
                @Jump.started -= m_Wrapper.m_TouchActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_TouchActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_TouchActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_TouchActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Wind.started += instance.OnWind;
                @Wind.performed += instance.OnWind;
                @Wind.canceled += instance.OnWind;
                @TouchTap.started += instance.OnTouchTap;
                @TouchTap.performed += instance.OnTouchTap;
                @TouchTap.canceled += instance.OnTouchTap;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public TouchActions @Touch => new TouchActions(this);
    public interface ITouchActions
    {
        void OnWind(InputAction.CallbackContext context);
        void OnTouchTap(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}
