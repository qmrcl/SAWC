using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SAWC.Core;
using SAWC.Core.Data;
using SAWC.Core.Input;
using SAWC.Modifiers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAWC.Tests.Editor
{
    public class CoreRegressionTests
    {
        private static readonly Vector3 TestOrigin = new Vector3(12000f, 10000f, 8000f);
        private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly List<Object> _objects = new List<Object>();
        private CharacterSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<CharacterSettings>();
            _settings.Data.Crouch.EnvironmentMask = ~0;
            _objects.Add(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            Physics.SyncTransforms();
        }

        [Test]
        public void ZeroJumpBufferStillAcceptsTheCurrentPress()
        {
            FrameContext context = CreateContext();
            context.Settings.Jump.JumpBufferTime = 0f;
            context.Settings.Jump.EnableAutoJump = false;
            context.JumpInput = true;
            object gravity = CreateInternal("CharacterGravity");

            TickGravity(gravity, ref context);

            Assert.That(VerticalVelocity(gravity), Is.EqualTo(context.Settings.Jump.JumpForce));
            context.IsGrounded = false;
            TickGravity(gravity, ref context);
            Assert.That(VerticalVelocity(gravity), Is.LessThan(context.Settings.Jump.JumpForce),
                "Holding a manual jump must not repeat its impulse.");
        }

        [Test]
        public void ZeroJumpBufferDoesNotRememberAnEarlierAirbornePress()
        {
            FrameContext context = CreateContext();
            context.Settings.Jump.JumpBufferTime = 0f;
            context.Settings.Jump.EnableAutoJump = false;
            object gravity = CreateInternal("CharacterGravity");
            context.IsGrounded = false;
            context.DeltaTime = 1f;
            TickGravity(gravity, ref context);
            context.DeltaTime = 0.02f;
            context.JumpInput = true;
            TickGravity(gravity, ref context);
            context.JumpInput = false;
            context.IsGrounded = true;

            TickGravity(gravity, ref context);

            Assert.That(VerticalVelocity(gravity), Is.LessThanOrEqualTo(0f));
        }

        [Test]
        public void PositiveJumpBufferStillExecutesAfterLanding()
        {
            FrameContext context = CreateContext();
            context.Settings.Jump.JumpBufferTime = 0.2f;
            context.Settings.Jump.EnableAutoJump = false;
            object gravity = CreateInternal("CharacterGravity");
            context.IsGrounded = false;
            context.DeltaTime = 1f;
            TickGravity(gravity, ref context);
            context.DeltaTime = 0.02f;
            context.JumpInput = true;
            TickGravity(gravity, ref context);
            context.JumpInput = false;
            context.IsGrounded = true;

            TickGravity(gravity, ref context);

            Assert.That(VerticalVelocity(gravity), Is.EqualTo(context.Settings.Jump.JumpForce));
        }

        [Test]
        public void ZeroFallDebounceOnlyFiresOncePerActualFall()
        {
            FrameContext context = CreateContext();
            context.Settings.Thresholds.AirStateDebounceTime = 0f;
            object tracker = CreateInternal("CharacterStateTracker");
            Invoke(tracker, "Initialize", true);
            int falls = 0;
            ((ICharacterState)tracker).FallStarted += () => falls++;

            for (int i = 0; i < 5; i++) TickState(tracker, ref context, Vector3.zero);
            Assert.That(falls, Is.Zero, "Standing on the ground is not falling.");

            context.IsGrounded = false;
            context.GravityVerticalVelocity = -2f;
            for (int i = 0; i < 5; i++) TickState(tracker, ref context, Vector3.down * 2f);
            Assert.That(falls, Is.EqualTo(1));

            context.IsGrounded = true;
            TickState(tracker, ref context, Vector3.zero);
            context.IsGrounded = false;
            TickState(tracker, ref context, Vector3.down * 2f);
            Assert.That(falls, Is.EqualTo(2));
        }

        [Test]
        public void PositiveFallDebounceStillWaitsForItsDuration()
        {
            FrameContext context = CreateContext();
            context.IsGrounded = false;
            context.GravityVerticalVelocity = -2f;
            context.DeltaTime = 0.05f;
            context.Settings.Thresholds.AirStateDebounceTime = 0.2f;
            object tracker = CreateInternal("CharacterStateTracker");
            Invoke(tracker, "Initialize", true);
            int falls = 0;
            ((ICharacterState)tracker).FallStarted += () => falls++;

            for (int i = 0; i < 3; i++) TickState(tracker, ref context, Vector3.down * 2f);
            Assert.That(falls, Is.Zero);
            for (int i = 0; i < 3; i++) TickState(tracker, ref context, Vector3.down * 2f);
            Assert.That(falls, Is.EqualTo(1));
        }

        [Test]
        public void RotationUsesContextTimeAndRemainsFiniteAfterAZeroTimeFrame()
        {
            Transform player = CreateObject("Rotation").transform;
            object rotation = CreateInternal("CharacterRotation", player);
            FrameContext context = CreateContext();
            context.WorldLookDirection = Vector3.forward;
            context.DeltaTime = 0f;
            Invoke(rotation, "Tick", context);
            float velocity = (float)rotation.GetType().GetField("_rotationVelocity", InstanceMembers).GetValue(rotation);
            Assert.That(float.IsNaN(velocity), Is.False);
            Assert.That(Quaternion.Angle(player.rotation, Quaternion.identity), Is.Zero);

            context.WorldLookDirection = Vector3.right;
            context.Settings.Rotation.StrafeRotationSmoothTime = 0.4f;
            context.DeltaTime = Mathf.Abs(Time.deltaTime) + 0.123f;
            float expectedVelocity = 0f;
            float expectedAngle = Mathf.SmoothDampAngle(0f, 90f, ref expectedVelocity, 0.4f,
                Mathf.Infinity, context.DeltaTime);

            Invoke(rotation, "Tick", context);

            Assert.That(Mathf.DeltaAngle(player.eulerAngles.y, expectedAngle), Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void DashReadsFreshProviderInputBeforeTheControllerMoves()
        {
            int inputOrder = typeof(MasterInputProvider).GetCustomAttribute<DefaultExecutionOrder>().order;
            int dashOrder = typeof(Dash).GetCustomAttribute<DefaultExecutionOrder>().order;
            int movementOrder = typeof(SAWController).GetCustomAttribute<DefaultExecutionOrder>().order;

            Assert.That(dashOrder, Is.GreaterThan(inputOrder),
                "A zero-buffer dash pressed together with movement must see this frame's direction.");
            Assert.That(dashOrder, Is.LessThan(movementOrder),
                "The controller must consume the newly started dash in the same frame.");
        }

        [TestCase(1f)]
        [TestCase(2f)]
        public void StandUpQueryUsesWorldSpaceAtDifferentPlayerScales(float scale)
        {
            object posture = CreateCrouchedPosture(scale, out _);
            CreateBox("Ceiling", TestOrigin + Vector3.up * (1.5f * scale), new Vector3(4f, 0.2f, 4f));
            Physics.SyncTransforms();

            Assert.That(CanStandUp(posture), Is.False);
        }

        [Test]
        public void StandUpQueryDetectsACeilingAlreadyOverlappingTheInitialSphere()
        {
            object posture = CreateCrouchedPosture(1f, out _);
            GameObject ceiling = CreateBox("Moving ceiling", TestOrigin + Vector3.up * 3f, new Vector3(4f, 0.2f, 4f));
            Physics.SyncTransforms();
            Assert.That(CanStandUp(posture), Is.True);

            ceiling.transform.position = TestOrigin + Vector3.up * 0.95f;
            Physics.SyncTransforms();

            Assert.That(CanStandUp(posture), Is.False,
                "A moving ceiling must remain detectable after entering the cast's starting sphere.");
        }

        [Test]
        public void StandUpQueryIgnoresThePlayersOwnControllerAndChildColliders()
        {
            object posture = CreateCrouchedPosture(1f, out CharacterController controller);
            GameObject body = CreateBox("Own body", Vector3.zero, Vector3.one);
            body.transform.SetParent(controller.transform, false);
            body.transform.localPosition = Vector3.up * 1.5f;
            Physics.SyncTransforms();

            Assert.That(CanStandUp(posture), Is.True);
        }

        [Test]
        public void StandUpQueryDoesNotIgnoreAnotherPlayersController()
        {
            object posture = CreateCrouchedPosture(1f, out _);
            GameObject otherPlayer = CreateObject("Other player");
            otherPlayer.transform.position = TestOrigin + Vector3.up * 1.5f;
            otherPlayer.AddComponent<CharacterController>();
            Physics.SyncTransforms();

            Assert.That(CanStandUp(posture), Is.False);
        }

        [Test]
        public void StandUpQueryGrowsItsBufferBeforeFilteringSelfColliders()
        {
            object posture = CreateCrouchedPosture(1f, out CharacterController controller);
            for (int i = 0; i < 12; i++)
            {
                GameObject body = CreateBox("Own collider " + i, Vector3.zero, Vector3.one * 0.2f);
                body.transform.SetParent(controller.transform, false);
                body.transform.localPosition = Vector3.up * (1.1f + i * 0.02f);
            }
            Physics.SyncTransforms();
            Assert.That(CanStandUp(posture), Is.True);

            CreateBox("External ceiling", TestOrigin + Vector3.up * 1.6f, Vector3.one * 0.2f);
            Physics.SyncTransforms();
            Assert.That(CanStandUp(posture), Is.False);
        }

        [Test]
        public void SmallHeightIncreaseStillChecksItsClearance()
        {
            _settings.Data.Crouch.StandingHeight = 1.04f;
            object posture = CreateCrouchedPosture(1f, out _);
            CreateBox("Close ceiling", TestOrigin + Vector3.up * 1.05f, new Vector3(4f, 0.06f, 4f));
            Physics.SyncTransforms();

            Assert.That(CanStandUp(posture), Is.False);
        }

        [Test]
        public void DisablingCrouchDoesNotForceStandingThroughACeiling()
        {
            object posture = CreateCrouchedPosture(1f, out _);
            GameObject ceiling = CreateBox("Ceiling", TestOrigin + Vector3.up * 1.5f, new Vector3(4f, 0.2f, 4f));
            _settings.Data.Crouch.CanCrouch = false;
            Physics.SyncTransforms();

            Assert.That(CheckCrouch(posture, false, true, CanStandUp(posture)), Is.True);

            Object.DestroyImmediate(ceiling);
            Physics.SyncTransforms();
            Assert.That(CheckCrouch(posture, false, true, CanStandUp(posture)), Is.False);
        }

        [Test]
        public void ControllerUsesDefaultSettingsWithoutAnAssignedAsset()
        {
            GameObject player = CreateObject("Controller without settings");
            player.SetActive(false);
            var controller = player.AddComponent<SAWController>();
            Assert.That(typeof(SAWController).GetField("_settings", InstanceMembers).GetValue(controller), Is.Null);

            Invoke(controller, "Awake");

            CharacterSettingsData settings = controller.BaseSettings;
            Assert.That(controller.enabled, Is.True);
            Assert.That(settings.Movement.MoveSpeed, Is.EqualTo(_settings.Data.Movement.MoveSpeed));
            Assert.That(settings.Jump.JumpForce, Is.EqualTo(_settings.Data.Jump.JumpForce));
            Assert.That(settings.Crouch.StandingHeight, Is.EqualTo(_settings.Data.Crouch.StandingHeight));
            Assert.That(settings.Movement.AccelerationCurve.Evaluate(0.5f),
                Is.EqualTo(_settings.Data.Movement.AccelerationCurve.Evaluate(0.5f)));
        }

        [Test]
        public void ContextModifierCanDisableCrouchInTheCurrentFrame()
        {
            var input = new TestInput { CrouchHeld = true };
            SAWController controller = CreateController(input);
            controller.Modifiers.AddContextModifier(new ContextModifier(
                (ref FrameContext frame) => frame.Settings.Crouch.CanCrouch = false));

            Invoke(controller, "Update");

            Assert.That(controller.State.IsCrouching, Is.False);
            Assert.That(controller.GetComponent<CharacterController>().height,
                Is.EqualTo(_settings.Data.Crouch.StandingHeight));
        }

        [Test]
        public void StandUpConstraintsUseTheModifiersFinalEnvironmentMask()
        {
            var input = new TestInput { CrouchHeld = true };
            SAWController controller = CreateController(input);
            Invoke(controller, "Update");
            Assert.That(controller.State.IsCrouching, Is.True);
            CreateBox("Ceiling", TestOrigin + Vector3.up * 1.5f, new Vector3(4f, 0.2f, 4f));
            Physics.SyncTransforms();
            input.CrouchHeld = false;
            controller.Modifiers.AddContextModifier(new ContextModifier(
                (ref FrameContext frame) => frame.Settings.Crouch.EnvironmentMask = 0));

            Invoke(controller, "Update");

            Assert.That(controller.State.IsCrouching, Is.False);
        }

        [Test]
        public void RemovingTheCurrentContextModifierDoesNotSkipTheNextOne()
        {
            var modifiers = new CharacterModifiers();
            var calls = new List<string>();
            ContextModifier first = null;
            first = new ContextModifier((ref FrameContext frame) =>
            {
                calls.Add("first");
                modifiers.RemoveContextModifier(first);
            });
            modifiers.AddContextModifier(first);
            modifiers.AddContextModifier(new ContextModifier((ref FrameContext frame) => calls.Add("second")));
            FrameContext context = CreateContext();

            modifiers.ProcessContext(ref context);

            CollectionAssert.AreEqual(new[] { "first", "second" }, calls);
        }

        [Test]
        public void RemovedModifiersAreSkippedAndAddedModifiersWaitForTheNextPass()
        {
            var modifiers = new CharacterModifiers();
            var calls = new List<string>();
            var removed = new ContextModifier((ref FrameContext frame) => calls.Add("removed"));
            var added = new ContextModifier((ref FrameContext frame) => calls.Add("added"));
            modifiers.AddContextModifier(new ContextModifier((ref FrameContext frame) =>
            {
                calls.Add("first");
                modifiers.RemoveContextModifier(removed);
                modifiers.AddContextModifier(added);
            }));
            modifiers.AddContextModifier(removed);
            FrameContext context = CreateContext();

            modifiers.ProcessContext(ref context);
            CollectionAssert.AreEqual(new[] { "first" }, calls);
            calls.Clear();
            modifiers.ProcessContext(ref context);
            CollectionAssert.AreEqual(new[] { "first", "added" }, calls);
        }

        [Test]
        public void RemovingTheCurrentVelocityModifierDoesNotSkipTheNextOne()
        {
            var modifiers = new CharacterModifiers();
            VelocityModifier first = null;
            first = new VelocityModifier((Vector3 velocity, ref FrameContext frame) =>
            {
                modifiers.RemoveVelocityModifier(first);
                return velocity + Vector3.right;
            });
            modifiers.AddVelocityModifier(first);
            modifiers.AddVelocityModifier(new VelocityModifier(
                (Vector3 velocity, ref FrameContext frame) => velocity + Vector3.up));
            FrameContext context = CreateContext();

            Vector3 result = modifiers.ProcessVelocity(Vector3.zero, ref context);

            Assert.That(result, Is.EqualTo(Vector3.right + Vector3.up));
        }

        [Test]
        public void ReentrantContextProcessingPreservesTheOuterSnapshot()
        {
            var modifiers = new CharacterModifiers();
            var calls = new List<string>();
            bool nested = false;
            modifiers.AddContextModifier(new ContextModifier((ref FrameContext frame) =>
            {
                calls.Add("first");
                if (nested) return;
                nested = true;
                modifiers.ProcessContext(ref frame);
            }));
            modifiers.AddContextModifier(new ContextModifier((ref FrameContext frame) => calls.Add("second")));
            FrameContext context = CreateContext();

            modifiers.ProcessContext(ref context);

            CollectionAssert.AreEqual(new[] { "first", "first", "second", "second" }, calls);
        }

        private FrameContext CreateContext() => new FrameContext
        {
            Settings = _settings.Data,
            IsGrounded = true,
            CanStandUp = true,
            DeltaTime = 0.02f
        };

        private static object CreateInternal(string name, params object[] arguments)
        {
            Type type = typeof(SAWController).Assembly.GetType("SAWC.Core." + name, true);
            return Activator.CreateInstance(type, InstanceMembers, null, arguments, null);
        }

        private static object Invoke(object instance, string name, params object[] arguments)
            => instance.GetType().GetMethod(name, InstanceMembers).Invoke(instance, arguments);

        private static void TickGravity(object gravity, ref FrameContext context)
        {
            object[] arguments = { context };
            Invoke(gravity, "Tick", arguments);
            context = (FrameContext)arguments[0];
        }

        private static float VerticalVelocity(object gravity)
            => (float)gravity.GetType().GetProperty("VerticalVelocity", InstanceMembers).GetValue(gravity);

        private static void TickState(object tracker, ref FrameContext context, Vector3 velocity)
        {
            object[] arguments = { context, velocity, false };
            Invoke(tracker, "Tick", arguments);
            context = (FrameContext)arguments[0];
        }

        private GameObject CreateObject(string name)
        {
            var gameObject = new GameObject("SAWC regression: " + name);
            _objects.Add(gameObject);
            return gameObject;
        }

        private GameObject CreateBox(string name, Vector3 position, Vector3 size)
        {
            GameObject gameObject = CreateObject(name);
            gameObject.transform.position = position;
            gameObject.AddComponent<BoxCollider>().size = size;
            return gameObject;
        }

        private object CreateCrouchedPosture(float scale, out CharacterController controller)
        {
            GameObject player = CreateObject("Player");
            player.transform.position = TestOrigin;
            player.transform.localScale = Vector3.one * scale;
            controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = Vector3.up;
            object posture = CreateInternal("CharacterPosture", controller, player.transform, _settings.Data);
            Invoke(posture, "Tick", true, _settings.Data);
            Physics.SyncTransforms();
            return posture;
        }

        private bool CanStandUp(object posture) => (bool)Invoke(posture, "CanStandUp", _settings.Data);

        private bool CheckCrouch(object posture, bool input, bool alreadyCrouching, bool canStandUp)
            => (bool)Invoke(posture, "CheckCrouchState", input, alreadyCrouching, canStandUp, _settings.Data);

        private SAWController CreateController(TestInput input)
        {
            GameObject player = CreateObject("Controller");
            player.SetActive(false);
            player.transform.position = TestOrigin;
            CharacterController capsule = player.AddComponent<CharacterController>();
            capsule.center = Vector3.up;
            _settings.Data.Physics.UseGravity = false;
            SAWController controller = player.AddComponent<SAWController>();
            typeof(SAWController).GetField("_settings", InstanceMembers).SetValue(controller, _settings);
            controller.SetInputProvider(input);
            player.SetActive(true);
            if (typeof(SAWController).GetField("_posture", InstanceMembers).GetValue(controller) == null)
                Invoke(controller, "Awake");
            Physics.SyncTransforms();
            return controller;
        }

        private sealed class TestInput : IInputProvider
        {
            public Vector2 MoveInput => Vector2.zero;
            public Vector3 WorldMoveDirection => Vector3.zero;
            public Vector3 WorldLookDirection => Vector3.forward;
            public bool SprintHeld => false;
            public bool JumpHeld => false;
            public bool CrouchHeld { get; set; }
        }

        private delegate void ContextModification(ref FrameContext context);
        private delegate Vector3 VelocityModification(Vector3 velocity, ref FrameContext context);

        private sealed class ContextModifier : IContextModifier
        {
            private readonly ContextModification _modify;
            public int Priority => 0;
            public ContextModifier(ContextModification modify) => _modify = modify;
            public void ModifyContext(ref FrameContext context) => _modify(ref context);
        }

        private sealed class VelocityModifier : IVelocityModifier
        {
            private readonly VelocityModification _modify;
            public int Priority => 0;
            public VelocityModifier(VelocityModification modify) => _modify = modify;
            public Vector3 ModifyVelocity(Vector3 velocity, ref FrameContext context) => _modify(velocity, ref context);
        }
    }
}
