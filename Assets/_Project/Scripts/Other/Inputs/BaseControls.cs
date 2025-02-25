// GENERATED AUTOMATICALLY FROM 'Assets/_Project/Scripts/Other/Inputs/BaseControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @BaseControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @BaseControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""BaseControls"",
    ""maps"": [
        {
            ""name"": ""Movement"",
            ""id"": ""33e788b7-c55e-4a19-a465-9895b3fe9759"",
            ""actions"": [
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""16498fbe-8c15-496c-aeeb-0b39abaa8339"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Control"",
                    ""type"": ""Button"",
                    ""id"": ""2dc3956d-ae77-41c6-a3a0-06774b1e2c99"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""d5978f0c-66f1-4f58-9b2a-d54db1af4ae8"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Camera"",
                    ""type"": ""Value"",
                    ""id"": ""3b44f76d-c8f3-4dad-8751-b3003c30fc2a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Shoot"",
                    ""type"": ""Button"",
                    ""id"": ""6fe0080f-83db-4565-99f0-1461db460f5a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a4302ce0-c8c8-4405-ab7c-cb98a0e6e303"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4f6b06ea-db73-4fa4-9ee6-8e3386ad9031"",
                    ""path"": ""<SwitchProControllerHID>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""71309d02-43a5-4379-ba68-561f5a8eaec6"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""044cc5a6-ddd0-4364-9a9e-3bdd176c102f"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector (Keyboard)"",
                    ""id"": ""15422645-f7a7-4c1e-93a2-c39b6409def8"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8f373eb5-8fd4-4fac-af46-a30aa7734353"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""e16c53a4-28e5-4970-89f1-6a658c5a26c9"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""1f09b82a-1a78-4a9d-a9c5-69e27196d423"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""71079341-a264-4b80-9624-d00ff1392b9e"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""256b3f8b-91c4-4f91-a981-7a3591728e52"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d6e17c12-a45d-4c98-a462-a9954d558fff"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a7f92bcc-f3fc-4a97-ab25-7c58f929b5a1"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Control"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""903d3872-c333-4b75-a519-ae07a53e61df"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Control"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b3bc1259-af6d-4160-9cf4-09157675dbbc"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""58ee27af-e883-4ce3-92b8-4e942cda0ae8"",
                    ""path"": ""<SwitchProControllerHID>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""DefaultControl"",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""DefaultControl"",
            ""bindingGroup"": ""DefaultControl"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": true,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<SwitchProControllerHID>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Movement
        m_Movement = asset.FindActionMap("Movement", throwIfNotFound: true);
        m_Movement_Jump = m_Movement.FindAction("Jump", throwIfNotFound: true);
        m_Movement_Control = m_Movement.FindAction("Control", throwIfNotFound: true);
        m_Movement_Move = m_Movement.FindAction("Move", throwIfNotFound: true);
        m_Movement_Camera = m_Movement.FindAction("Camera", throwIfNotFound: true);
        m_Movement_Shoot = m_Movement.FindAction("Shoot", throwIfNotFound: true);
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

    // Movement
    private readonly InputActionMap m_Movement;
    private IMovementActions m_MovementActionsCallbackInterface;
    private readonly InputAction m_Movement_Jump;
    private readonly InputAction m_Movement_Control;
    private readonly InputAction m_Movement_Move;
    private readonly InputAction m_Movement_Camera;
    private readonly InputAction m_Movement_Shoot;
    public struct MovementActions
    {
        private @BaseControls m_Wrapper;
        public MovementActions(@BaseControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Jump => m_Wrapper.m_Movement_Jump;
        public InputAction @Control => m_Wrapper.m_Movement_Control;
        public InputAction @Move => m_Wrapper.m_Movement_Move;
        public InputAction @Camera => m_Wrapper.m_Movement_Camera;
        public InputAction @Shoot => m_Wrapper.m_Movement_Shoot;
        public InputActionMap Get() { return m_Wrapper.m_Movement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MovementActions set) { return set.Get(); }
        public void SetCallbacks(IMovementActions instance)
        {
            if (m_Wrapper.m_MovementActionsCallbackInterface != null)
            {
                @Jump.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnJump;
                @Control.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnControl;
                @Control.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnControl;
                @Control.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnControl;
                @Move.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Camera.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnCamera;
                @Camera.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnCamera;
                @Camera.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnCamera;
                @Shoot.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnShoot;
                @Shoot.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnShoot;
                @Shoot.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnShoot;
            }
            m_Wrapper.m_MovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Control.started += instance.OnControl;
                @Control.performed += instance.OnControl;
                @Control.canceled += instance.OnControl;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Camera.started += instance.OnCamera;
                @Camera.performed += instance.OnCamera;
                @Camera.canceled += instance.OnCamera;
                @Shoot.started += instance.OnShoot;
                @Shoot.performed += instance.OnShoot;
                @Shoot.canceled += instance.OnShoot;
            }
        }
    }
    public MovementActions @Movement => new MovementActions(this);
    private int m_DefaultControlSchemeIndex = -1;
    public InputControlScheme DefaultControlScheme
    {
        get
        {
            if (m_DefaultControlSchemeIndex == -1) m_DefaultControlSchemeIndex = asset.FindControlSchemeIndex("DefaultControl");
            return asset.controlSchemes[m_DefaultControlSchemeIndex];
        }
    }
    public interface IMovementActions
    {
        void OnJump(InputAction.CallbackContext context);
        void OnControl(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnCamera(InputAction.CallbackContext context);
        void OnShoot(InputAction.CallbackContext context);
    }
}
