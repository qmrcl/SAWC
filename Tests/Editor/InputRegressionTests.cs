using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SAWC.Core.Input;
using SAWC.Core.Input.Readers;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace SAWC.Tests.Editor
{
    // InputTestFixture restores the editor's original devices, actions and settings.
    public class InputRegressionTests : InputTestFixture
    {
        private readonly List<Object> _created = new List<Object>();

        public override void TearDown()
        {
            try
            {
                for (int i = _created.Count - 1; i >= 0; i--)
                    if (_created[i] != null) Object.DestroyImmediate(_created[i]);
                _created.Clear();
            }
            finally { base.TearDown(); }
        }

        [Test]
        public void Button_AdditionalPointerCannotPressOrReleaseOwner()
        {
            var button = Button();
            int downs = 0, ups = 0;
            button.OnPointerDownEvent.AddListener(() => downs++);
            button.OnPointerUpEvent.AddListener(() => ups++);
            button.OnPointerDown(Pointer(1));
            button.OnPointerDown(Pointer(2));
            button.OnPointerUp(Pointer(2));
            Assert.That(downs, Is.EqualTo(1));
            Assert.That(ups, Is.Zero);
            button.OnPointerUp(Pointer(1));
            Assert.That(ups, Is.EqualTo(1));
        }

        [Test]
        public void Button_DuplicateReleaseDoesNotInvokeUpTwice()
        {
            var button = Button();
            int ups = 0;
            button.OnPointerUpEvent.AddListener(() => ups++);
            button.OnPointerDown(Pointer(1));
            button.OnPointerUp(Pointer(1));
            button.OnPointerUp(Pointer(1));
            Invoke(button, "OnDisable");
            Assert.That(ups, Is.EqualTo(1));
        }

        [Test]
        public void Button_HidingWhilePressedReleasesLiveReader()
        {
            var reader = MobileReader(out _);
            var button = Button();
            button.OnPointerDownEvent.AddListener(() => reader.SetJump(true));
            button.OnPointerUpEvent.AddListener(() => reader.SetJump(false));
            button.OnPointerDown(Pointer(1));
            Assert.That(reader.Jump, Is.True);
            button.gameObject.SetActive(false);
            // Ordinary MonoBehaviour callbacks are not guaranteed in EditMode.
            Invoke(button, "OnDisable");
            Assert.That(reader.isActiveAndEnabled, Is.True);
            Assert.That(reader.Jump, Is.False);
        }

        [Test]
        public void Button_FocusLossReleasesAndAllowsNewPointer()
        {
            var button = Button();
            int downs = 0, ups = 0;
            button.OnPointerDownEvent.AddListener(() => downs++);
            button.OnPointerUpEvent.AddListener(() => ups++);
            button.OnPointerDown(Pointer(1));
            Invoke(button, "OnApplicationFocus", false);
            button.OnPointerDown(Pointer(2));
            Assert.That(downs, Is.EqualTo(2));
            Assert.That(ups, Is.EqualTo(1));
        }

        [Test]
        public void Button_DisabledComponentRejectsNewPress()
        {
            var button = Button();
            int downs = 0;
            button.OnPointerDownEvent.AddListener(() => downs++);
            button.enabled = false;
            button.OnPointerDown(Pointer(1));
            Assert.That(downs, Is.Zero);
        }

        [Test]
        public void Joystick_AnotherPointerCannotChangeOrReleaseDirection()
        {
            var joystick = Joystick();
            joystick.OnPointerDown(Pointer(1, new Vector2(75, 0)));
            Vector2 original = joystick.JoystickDirection;
            Assert.That(original.x, Is.GreaterThan(0f));
            joystick.OnPointerDown(Pointer(2, new Vector2(-75, 0)));
            joystick.OnDrag(Pointer(2, new Vector2(-75, 0)));
            joystick.OnPointerUp(Pointer(2));
            Assert.That(joystick.JoystickDirection, Is.EqualTo(original));
            joystick.OnPointerUp(Pointer(1));
            Assert.That(joystick.JoystickDirection, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Joystick_DisableResetsDirectionHandleAndOwnership()
        {
            var joystick = Joystick();
            joystick.OnPointerDown(Pointer(1, new Vector2(75, 0)));
            Invoke(joystick, "OnDisable");
            Assert.That(joystick.JoystickDirection, Is.EqualTo(Vector2.zero));
            Assert.That(Get<RectTransform>(joystick, "_handle").anchoredPosition, Is.EqualTo(Vector2.zero));
            joystick.OnPointerDown(Pointer(2, new Vector2(-75, 0)));
            Assert.That(joystick.JoystickDirection.x, Is.LessThan(0f));
        }

        [TestCase(0.7f)]
        [TestCase(1f)]
        [TestCase(1.5f)]
        public void MobileReader_FullHandleTravelProducesFullMovement(float limit)
        {
            var reader = MobileReader(out var joystick);
            SetField(joystick, "_handleLimit", limit);
            SetField(joystick, "_currentDirection", Vector2.up * limit);
            Assert.That(reader.Move.y, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(reader.Sprint, Is.True);
        }

        [Test]
        public void MobileReader_HalfTravelPreservesAnalogMagnitude()
        {
            var reader = MobileReader(out var joystick);
            SetField(joystick, "_handleLimit", 0.7f);
            SetField(joystick, "_currentDirection", Vector2.up * 0.35f);
            Assert.That(reader.Move.y, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(reader.Sprint, Is.False);
        }

        [Test]
        public void MobileReader_DisableClearsHeldButtonsAndCrouchToggle()
        {
            var reader = MobileReader(out _);
            reader.SetJump(true);
            reader.SetSprint(true);
            reader.SetCrouch(true);
            Invoke(reader, "OnDisable");
            Assert.That(reader.Jump, Is.False);
            Assert.That(reader.Sprint, Is.False);
            Assert.That(reader.Crouch, Is.False);
        }

        [Test]
        public void Provider_DisableClearsCachedInputAndWorldMovement()
        {
            var reader = MobileReader(out var joystick);
            SetField(joystick, "_currentDirection", Vector2.up);
            reader.SetJump(true);
            reader.SetSprint(true);
            var provider = Provider(reader);
            Invoke(provider, "Update");
            Assert.That(provider.WorldMoveDirection.sqrMagnitude, Is.GreaterThan(0f));
            Assert.That(provider.JumpHeld, Is.True);
            provider.enabled = false;
            Invoke(provider, "OnDisable");
            Assert.That(provider.MoveInput, Is.EqualTo(Vector2.zero));
            Assert.That(provider.WorldMoveDirection, Is.EqualTo(Vector3.zero));
            Assert.That(provider.JumpHeld || provider.SprintHeld || provider.CrouchHeld, Is.False);
        }

        [Test]
        public void Provider_DisabledReaderCannotKeepCachedJumpHeld()
        {
            var reader = MobileReader(out _);
            reader.SetJump(true);
            var provider = Provider(reader);
            Invoke(provider, "Update");
            Assert.That(provider.JumpHeld, Is.True);
            reader.enabled = false;
            Invoke(provider, "Update");
            Assert.That(provider.JumpHeld, Is.False);
        }

        [Test]
        public void LookPad_OnlyOwnerContributesDelta()
        {
            var pad = Component<LookPad>();
            pad.OnPointerDown(Pointer(1));
            pad.OnDrag(Pointer(1, delta: new Vector2(3, 4)));
            pad.OnPointerDown(Pointer(2));
            pad.OnDrag(Pointer(2, delta: new Vector2(90, 90)));
            pad.OnPointerUp(Pointer(2));
            Assert.That(pad.Delta, Is.EqualTo(new Vector2(3, 4)));
        }

        [Test]
        public void LookPad_ReleasePreservesLastDragUntilConsumersReadIt()
        {
            var pad = Component<LookPad>();
            pad.OnPointerDown(Pointer(1));
            pad.OnDrag(Pointer(1, delta: Vector2.one));
            pad.OnPointerUp(Pointer(1));
            Assert.That(pad.Delta, Is.EqualTo(Vector2.one));
            Invoke(pad, "LateUpdate");
            Assert.That(pad.Delta, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void LookPad_DisableDropsOldGestureAndAcceptsNewOwner()
        {
            var pad = Component<LookPad>();
            pad.OnPointerDown(Pointer(1));
            pad.OnDrag(Pointer(1, delta: Vector2.one));
            Invoke(pad, "OnDisable");
            pad.OnPointerDown(Pointer(2));
            pad.OnDrag(Pointer(2, delta: Vector2.right));
            Assert.That(pad.Delta, Is.EqualTo(Vector2.right));
        }

        [Test]
        public void TouchReaders_SharePadWithoutConsumingEachOthersDelta()
        {
            var pad = Component<LookPad>();
            pad.OnPointerDown(Pointer(1));
            pad.OnDrag(Pointer(1, delta: new Vector2(12, 6)));
            var first = TouchReceiver(pad);
            var second = TouchReceiver(pad);
            var axis = new TouchInputReceiver.TouchReader();
            float a = axis.GetValue(first, IInputAxisOwner.AxisDescriptor.Hints.X);
            float b = axis.GetValue(second, IInputAxisOwner.AxisDescriptor.Hints.X);
            Assert.That(b, Is.EqualTo(a));
            Assert.That(pad.Delta, Is.EqualTo(new Vector2(12, 6)));
        }

        [Test]
        public void TouchReader_CancelsDriverDeltaTimeForPixelDisplacement()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) Assert.Ignore("Unity has not advanced an editor frame yet.");
            var pad = Component<LookPad>();
            pad.OnPointerDown(Pointer(1));
            pad.OnDrag(Pointer(1, delta: new Vector2(12, 0)));
            var receiver = TouchReceiver(pad);
            receiver.SetSensitivity(0.5f);
            float speed = new TouchInputReceiver.TouchReader().GetValue(receiver,
                IInputAxisOwner.AxisDescriptor.Hints.X);
            var axis = new InputAxis { Range = new Vector2(-180f, 180f), Wrap = true };
            var driver = new DefaultInputAxisDriver();
            driver.ProcessInput(ref axis, speed, dt);
            Assert.That(axis.Value, Is.EqualTo(6f).Within(0.0001f));
        }

        [Test]
        public void NewReader_StandaloneReferencesStillReadDeviceInput()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var asset = Actions();
            var move = asset.FindAction("Move");
            var reader = Component<NewInputReader>();
            SetField(reader, "_moveAction", Keep(InputActionReference.Create(move)));
            Invoke(reader, "Awake");
            asset.Enable();
            Set(gamepad.leftStick, Vector2.up);
            Assert.That(reader.Move, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void NewReader_ReadsPlayerOwnedCopyInsteadOfOriginalAssetsDevice()
        {
            var firstGamepad = InputSystem.AddDevice<Gamepad>();
            var secondGamepad = InputSystem.AddDevice<Gamepad>();
            var source = Actions();
            var copy = Keep(Object.Instantiate(source));
            source.devices = new InputDevice[] { firstGamepad };
            copy.devices = new InputDevice[] { secondGamepad };
            var go = GameObject();
            var player = go.AddComponent<PlayerInput>();
            player.enabled = false;
            player.actions = copy;
            var reader = go.AddComponent<NewInputReader>();
            SetField(reader, "_moveAction", Keep(InputActionReference.Create(source.FindAction("Move"))));
            SetField(reader, "_jumpAction", Keep(InputActionReference.Create(source.FindAction("Jump"))));
            Invoke(reader, "Awake");
            source.Enable();
            copy.Enable();
            Set(firstGamepad.leftStick, Vector2.up);
            Set(secondGamepad.leftStick, Vector2.right);
            Press(secondGamepad.buttonSouth);
            Assert.That(reader.Move, Is.EqualTo(Vector2.right));
            Assert.That(reader.Jump, Is.True);
            Release(secondGamepad.buttonSouth);
            Press(firstGamepad.buttonSouth);
            Assert.That(reader.Jump, Is.False);
        }

        [Test]
        public void ProjectLook_GamepadValueIsAngularSpeedRatherThanPerFrameAngle()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var asset = ProjectActions();
            var look = asset.FindAction("Player/Look");
            look.Enable();
            Set(gamepad.rightStick, Vector2.right);
            Assert.That(look.ReadValue<Vector2>().x, Is.EqualTo(66f).Within(0.001f));
            foreach (float dt in new[] { 1f / 30f, 1f / 60f, 1f / 144f })
            {
                var axis = new InputAxis { Range = new Vector2(-180f, 180f), Wrap = true };
                var driver = new DefaultInputAxisDriver();
                int frames = Mathf.RoundToInt(1f / dt);
                for (int i = 0; i < frames; i++) driver.ProcessInput(ref axis, look.ReadValue<Vector2>().x, dt);
                Assert.That(axis.Value, Is.EqualTo(66f).Within(0.01f));
            }
        }

        [Test]
        public void ProjectLook_MousePixelDeltaRetainsExistingAngularScale()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) Assert.Ignore("Unity has not advanced an editor frame yet.");
            var mouse = InputSystem.AddDevice<Mouse>();
            var asset = ProjectActions();
            var look = asset.FindAction("Player/Look");
            look.Enable();
            Set(mouse.delta, new Vector2(12, 0));
            Assert.That(look.ReadValue<Vector2>().x * dt, Is.EqualTo(1.2f).Within(0.0001f));
        }

        [Test]
        public void ProjectLook_OnlyMouseDeltaHasTimeNormalizationProcessor()
        {
            var asset = ProjectActions();
            var look = asset.FindAction("Player/Look");
            Assert.That(look.processors, Does.Not.Contain("DeltaTimeScale"));
            bool mouseFound = false, gamepadFound = false;
            foreach (var binding in look.bindings)
            {
                if (binding.path == "<Mouse>/delta")
                {
                    mouseFound = true;
                    Assert.That(binding.processors, Does.Contain("DeltaTimeScale"));
                }
                if (binding.path == "<Gamepad>/rightStick")
                {
                    gamepadFound = true;
                    Assert.That(binding.processors, Does.Not.Contain("DeltaTimeScale"));
                }
            }
            Assert.That(mouseFound && gamepadFound, Is.True);
        }

        [TestCase(25f, 2f, -1f)]
        [TestCase(25f, -2f, 1f)]
        [TestCase(-60f, 15f, -1f)]
        [TestCase(80f, -30f, 1f)]
        public void Provider_StrafeIsPerpendicularToViewDespitePitchAndRoll(float pitch, float roll, float side)
        {
            var reader = MobileReader(out var joystick);
            SetField(joystick, "_handleLimit", 1f);
            SetField(joystick, "_currentDirection", Vector2.right * side);
            var provider = Provider(reader);
            var camera = Get<Transform>(provider, "_cameraTransform");
            camera.rotation = Quaternion.Euler(pitch, 37f, roll);

            Invoke(provider, "Update");

            Vector3 forward = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
            Vector3 expected = Vector3.Cross(Vector3.up, forward) * side;
            Assert.That(Vector3.Distance(provider.WorldMoveDirection, expected), Is.LessThan(0.00001f));
            Assert.That(Vector3.Dot(provider.WorldMoveDirection, camera.forward), Is.Zero.Within(0.00001f));
            Assert.That(Vector3.Distance(provider.WorldLookDirection, forward), Is.LessThan(0.00001f));
        }

        [TestCase(90f)]
        [TestCase(-90f)]
        public void Provider_VerticalCameraRetainsFiniteUnitStrafe(float pitch)
        {
            var reader = MobileReader(out var joystick);
            SetField(joystick, "_handleLimit", 1f);
            SetField(joystick, "_currentDirection", Vector2.right);
            var provider = Provider(reader);
            Get<Transform>(provider, "_cameraTransform").rotation = Quaternion.Euler(pitch, 37f, 0f);

            Invoke(provider, "Update");

            Assert.That(provider.WorldMoveDirection.magnitude, Is.EqualTo(1f).Within(0.00001f));
            Assert.That(provider.WorldMoveDirection.y, Is.Zero);
            Assert.That(Vector3.Dot(provider.WorldMoveDirection, provider.WorldLookDirection), Is.Zero.Within(0.00001f));
        }

        [TestCase(0.3f, 0.4f, 0.5f)]
        [TestCase(1f, 1f, 1f)]
        public void Provider_PitchedRolledCameraPreservesAnalogAndCapsDiagonals(float x, float y, float magnitude)
        {
            var reader = MobileReader(out var joystick);
            SetField(joystick, "_handleLimit", 1f);
            SetField(joystick, "_currentDirection", new Vector2(x, y));
            var provider = Provider(reader);
            Get<Transform>(provider, "_cameraTransform").rotation = Quaternion.Euler(60f, 37f, 15f);

            Invoke(provider, "Update");

            Assert.That(provider.WorldMoveDirection.magnitude, Is.EqualTo(magnitude).Within(0.00001f));
            Assert.That(provider.WorldMoveDirection.y, Is.Zero);
        }

        private T Keep<T>(T value) where T : Object { _created.Add(value); return value; }

        private GameObject GameObject() => Keep(new GameObject("SAWC input regression"));

        private T Component<T>() where T : Component => GameObject().AddComponent<T>();

        private UniversalUIButton Button()
        {
            var button = Component<UniversalUIButton>();
            button.OnPointerDownEvent = new UnityEvent();
            button.OnPointerUpEvent = new UnityEvent();
            return button;
        }

        private SAWC.Core.Input.Joystick Joystick()
        {
            var go = Keep(new GameObject("SAWC test joystick", typeof(RectTransform)));
            go.SetActive(false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(200, 200);
            var handle = Keep(new GameObject("Handle", typeof(RectTransform)));
            handle.transform.SetParent(go.transform, false);
            var joystick = go.AddComponent<SAWC.Core.Input.Joystick>();
            SetField(joystick, "_handle", (RectTransform)handle.transform);
            go.SetActive(true);
            Invoke(joystick, "Awake");
            return joystick;
        }

        private MobileUIReader MobileReader(out SAWC.Core.Input.Joystick joystick)
        {
            joystick = Joystick();
            var go = GameObject();
            go.SetActive(false);
            var reader = go.AddComponent<MobileUIReader>();
            SetField(reader, "_joystick", joystick);
            go.SetActive(true);
            return reader;
        }

        private MasterInputProvider Provider(BaseInputReader reader)
        {
            var provider = Component<MasterInputProvider>();
            SetField(provider, "_readers", new[] { reader });
            SetField(provider, "_cameraTransform", GameObject().transform);
            return provider;
        }

        private TouchInputReceiver TouchReceiver(LookPad pad)
        {
            var receiver = Component<TouchInputReceiver>();
            receiver.IgnoreTimeScale = true;
            SetField(receiver, "_lookPad", pad);
            return receiver;
        }

        private InputActionAsset Actions()
        {
            var asset = Keep(ScriptableObject.CreateInstance<InputActionAsset>());
            var map = new InputActionMap("Player");
            map.AddAction("Move", InputActionType.Value, "<Gamepad>/leftStick", expectedControlLayout: "Vector2");
            map.AddAction("Jump", InputActionType.Button, "<Gamepad>/buttonSouth");
            asset.AddActionMap(map);
            return asset;
        }

        private InputActionAsset ProjectActions()
        {
            // Fixture resets third-party processor registration with the input state.
            var processor = typeof(CinemachineBrain).Assembly.GetType("DeltaTimeScaleProcessor");
            Assert.That(processor, Is.Not.Null);
            InputSystem.RegisterProcessor(processor, "DeltaTimeScale");
            var source = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Packages/com.qmrcl.sawc/Runtime/Core/Input/SAWCInputAction.inputactions");
            Assert.That(source, Is.Not.Null);
            return Keep(Object.Instantiate(source));
        }

        private static PointerEventData Pointer(int id, Vector2 position = default, Vector2 delta = default)
            => new PointerEventData(null) { pointerId = id, position = position, delta = delta };

        private static FieldInfo Field(object target, string name)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            throw new MissingFieldException(target.GetType().Name, name);
        }

        private static void SetField(object target, string name, object value) => Field(target, name).SetValue(target, value);
        private static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);

        private static void Invoke(object target, string name, params object[] arguments)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method == null) continue;
                method.Invoke(target, arguments);
                return;
            }
            throw new MissingMethodException(target.GetType().Name, name);
        }
    }
}
