// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/GameInput/ActionInputs.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @ActionInputs : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @ActionInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""ActionInputs"",
    ""maps"": [
        {
            ""name"": ""Movement"",
            ""id"": ""fd194e84-ed8a-44ea-9ce6-2a5abe3f5972"",
            ""actions"": [
                {
                    ""name"": ""Horizontal"",
                    ""type"": ""PassThrough"",
                    ""id"": ""df18e738-fa3d-4773-8906-2ae6e139d745"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": ""AxisDeadzone,Normalize(max=1)"",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Vertical"",
                    ""type"": ""PassThrough"",
                    ""id"": ""978109b5-a488-46f9-a50f-e42938991f9e"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": ""AxisDeadzone,Normalize(max=1)"",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Horizontal"",
                    ""id"": ""a2031189-e0cb-456d-b4d4-0678db35194f"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""56314dc2-086b-49e5-b512-706979b721d6"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""5ac22407-8266-4655-aa5d-0e8d8a3b2f0a"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""37182348-a0d4-4664-a62f-a99b6f9cdfb0"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""e400a7a4-95ef-49d3-b3ef-fa3841392ca6"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Vertical"",
                    ""id"": ""9d9bfc40-20bd-49c4-b1ee-d174b7102a96"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""4bc2fab1-bbd5-4a4e-87cd-4da072c414d6"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""7f460449-4c3a-4b40-99be-ce040cbd15a0"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""98c06bb3-65f7-4e29-a4f9-066a8efdf10d"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""eaa05e45-3a4f-4f2c-82a5-1722dfe5cd7f"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Combat"",
            ""id"": ""ab2a0e5f-5fad-4394-8e4e-a38d50b2d201"",
            ""actions"": [
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""92079911-86a1-45d2-a947-f45d4c9e6398"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4ee046f5-8fd9-4443-bf2a-17b1c33a1631"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""PC"",
            ""bindingGroup"": ""PC"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Movement
        m_Movement = asset.FindActionMap("Movement", throwIfNotFound: true);
        m_Movement_Horizontal = m_Movement.FindAction("Horizontal", throwIfNotFound: true);
        m_Movement_Vertical = m_Movement.FindAction("Vertical", throwIfNotFound: true);
        // Combat
        m_Combat = asset.FindActionMap("Combat", throwIfNotFound: true);
        m_Combat_Newaction = m_Combat.FindAction("New action", throwIfNotFound: true);
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
    private readonly InputAction m_Movement_Horizontal;
    private readonly InputAction m_Movement_Vertical;
    public struct MovementActions
    {
        private @ActionInputs m_Wrapper;
        public MovementActions(@ActionInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Horizontal => m_Wrapper.m_Movement_Horizontal;
        public InputAction @Vertical => m_Wrapper.m_Movement_Vertical;
        public InputActionMap Get() { return m_Wrapper.m_Movement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MovementActions set) { return set.Get(); }
        public void SetCallbacks(IMovementActions instance)
        {
            if (m_Wrapper.m_MovementActionsCallbackInterface != null)
            {
                @Horizontal.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnHorizontal;
                @Horizontal.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnHorizontal;
                @Horizontal.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnHorizontal;
                @Vertical.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnVertical;
                @Vertical.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnVertical;
                @Vertical.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnVertical;
            }
            m_Wrapper.m_MovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Horizontal.started += instance.OnHorizontal;
                @Horizontal.performed += instance.OnHorizontal;
                @Horizontal.canceled += instance.OnHorizontal;
                @Vertical.started += instance.OnVertical;
                @Vertical.performed += instance.OnVertical;
                @Vertical.canceled += instance.OnVertical;
            }
        }
    }
    public MovementActions @Movement => new MovementActions(this);

    // Combat
    private readonly InputActionMap m_Combat;
    private ICombatActions m_CombatActionsCallbackInterface;
    private readonly InputAction m_Combat_Newaction;
    public struct CombatActions
    {
        private @ActionInputs m_Wrapper;
        public CombatActions(@ActionInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Newaction => m_Wrapper.m_Combat_Newaction;
        public InputActionMap Get() { return m_Wrapper.m_Combat; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CombatActions set) { return set.Get(); }
        public void SetCallbacks(ICombatActions instance)
        {
            if (m_Wrapper.m_CombatActionsCallbackInterface != null)
            {
                @Newaction.started -= m_Wrapper.m_CombatActionsCallbackInterface.OnNewaction;
                @Newaction.performed -= m_Wrapper.m_CombatActionsCallbackInterface.OnNewaction;
                @Newaction.canceled -= m_Wrapper.m_CombatActionsCallbackInterface.OnNewaction;
            }
            m_Wrapper.m_CombatActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Newaction.started += instance.OnNewaction;
                @Newaction.performed += instance.OnNewaction;
                @Newaction.canceled += instance.OnNewaction;
            }
        }
    }
    public CombatActions @Combat => new CombatActions(this);
    private int m_PCSchemeIndex = -1;
    public InputControlScheme PCScheme
    {
        get
        {
            if (m_PCSchemeIndex == -1) m_PCSchemeIndex = asset.FindControlSchemeIndex("PC");
            return asset.controlSchemes[m_PCSchemeIndex];
        }
    }
    public interface IMovementActions
    {
        void OnHorizontal(InputAction.CallbackContext context);
        void OnVertical(InputAction.CallbackContext context);
    }
    public interface ICombatActions
    {
        void OnNewaction(InputAction.CallbackContext context);
    }
}
