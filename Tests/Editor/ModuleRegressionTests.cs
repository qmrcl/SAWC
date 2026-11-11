using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SAWC.Core;
using SAWC.Core.Data;
using SAWC.Core.Input;
using SAWC.Core.Input.Readers;
using SAWC.Modifiers;
using SAWC.Modules.Audio;
using SAWC.Modules.Camera;
using SAWC.Modules.CameraUtils;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace SAWC.Tests.Editor
{
    public class ModuleRegressionTests
    {
        private readonly List<Object> _created = new List<Object>();

        private sealed class TestInput : IInputProvider
        {
            public Vector3 Direction;
            public Vector2 MoveInput => new Vector2(Direction.x, Direction.z);
            public Vector3 WorldMoveDirection => Direction;
            public Vector3 WorldLookDirection => Vector3.forward;
            public bool SprintHeld => false;
            public bool CrouchHeld => false;
            public bool JumpHeld => false;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _created.Count - 1; i >= 0; i--)
            {
                var item = _created[i];
                if (item == null) continue;
                if (item is InputActionAsset actions) actions.Disable();
                Object.DestroyImmediate(item);
            }
            _created.Clear();
            Physics.SyncTransforms();
        }

        [Test]
        public void Dash_ZeroBufferStartsOnDirectRequestWhileMoving()
        {
            var input = new TestInput { Direction = Vector3.right };
            var dash = CreateDash(input);

            dash.TriggerDash();
            Call(dash, "Update");

            Assert.That(dash.IsDashing, Is.True);
            var context = new FrameContext();
            Vector3 velocity = dash.ModifyVelocity(Vector3.zero, ref context);
            Assert.That(velocity.x, Is.EqualTo(25f).Within(0.001f));
            Assert.That(velocity.z, Is.Zero.Within(0.001f));
        }

        [Test]
        public void Dash_ZeroBufferDoesNotQueueStationaryRequest()
        {
            var input = new TestInput();
            var dash = CreateDash(input);
            dash.TriggerDash();
            Call(dash, "Update");

            input.Direction = Vector3.forward;
            Call(dash, "Update");

            Assert.That(dash.IsDashing, Is.False);
        }

        [Test]
        public void Dash_DisableCancelsMovementAndRetainsCooldown()
        {
            var dash = CreateDash(new TestInput { Direction = Vector3.forward });
            dash.TriggerDash();
            Call(dash, "Update");
            Assert.That(dash.IsDashing, Is.True);

            Call(dash, "OnDisable");
            Call(dash, "OnEnable");

            Assert.That(dash.IsDashing, Is.False);
            var context = new FrameContext();
            Vector3 original = new Vector3(1f, -2f, 3f);
            Assert.That(dash.ModifyVelocity(original, ref context), Is.EqualTo(original));
            dash.TriggerDash();
            Call(dash, "Update");
            Assert.That(dash.IsDashing, Is.False, "Toggling the component must not bypass cooldown.");
        }

        [Test]
        public void Dash_DisableDoesNotDisableSharedAction()
        {
            var asset = Keep(ScriptableObject.CreateInstance<InputActionAsset>());
            var map = new InputActionMap("Player");
            asset.AddActionMap(map);
            var action = map.AddAction("Dash", InputActionType.Button, "<Keyboard>/e");
            var reference = Keep(InputActionReference.Create(action));
            action.Enable();
            var dash = CreateDash(new TestInput(), reference);

            Call(dash, "OnDisable");

            Assert.That(action.enabled, Is.True);
        }

        [Test]
        public void Dash_UnresolvedActionReferenceAllowsPublicTrigger()
        {
            var reference = Keep(ScriptableObject.CreateInstance<InputActionReference>());
            var dash = CreateDash(new TestInput { Direction = Vector3.forward }, reference);

            Assert.DoesNotThrow(() =>
            {
                dash.TriggerDash();
                Call(dash, "Update");
                Call(dash, "OnDisable");
            });
        }

        [Test]
        public void Dash_UsesPlayerInputCopyWithoutEnablingItsMap()
        {
            var asset = Keep(ScriptableObject.CreateInstance<InputActionAsset>());
            var map = new InputActionMap("Player");
            asset.AddActionMap(map);
            var sourceAction = map.AddAction("Dash", InputActionType.Button, "<Keyboard>/e");
            var reference = Keep(InputActionReference.Create(sourceAction));
            var playerAsset = Keep(Object.Instantiate(asset));
            var controller = CreateController(new TestInput());
            var playerInput = controller.gameObject.AddComponent<PlayerInput>();
            playerInput.actions = playerAsset;
            var dash = controller.gameObject.AddComponent<Dash>();
            Set(dash, "_dashAction", reference);

            Call(dash, "Awake");
            Call(dash, "OnEnable");

            var ownedAction = playerInput.actions.FindAction(sourceAction.id);
            Assert.That(Call(dash, "ResolveDashAction"), Is.SameAs(ownedAction));
            Assert.That(ownedAction, Is.Not.SameAs(sourceAction));
            Assert.That(ownedAction.enabled, Is.False, "PlayerInput owns its action-map activation.");
        }

        [Test]
        public void DynamicFov_FailedInitializationDoesNotResetCameraToZero()
        {
            var go = InactiveObject("FovWithoutController");
            var camera = go.AddComponent<CinemachineCamera>();
            SetFov(camera, 60f);
            var fov = go.AddComponent<DynamicFOV>();
            Set(fov, "_cinemachineCam", camera);
            LogAssert.Expect(LogType.Error, "Required references are null or missing on 'FovWithoutController'!");

            Call(fov, "Awake");
            Call(fov, "OnDisable");

            Assert.That(fov.enabled, Is.False);
            Assert.That(camera.Lens.FieldOfView, Is.EqualTo(60f).Within(0.001f));
        }

        [Test]
        public void DynamicFov_ResolvesOwnCameraAndRestoresOriginalLens()
        {
            var controller = CreateController(new TestInput());
            var go = InactiveObject("FovWithOwnCamera");
            var camera = go.AddComponent<CinemachineCamera>();
            SetFov(camera, 65f);
            var fov = go.AddComponent<DynamicFOV>();
            Set(fov, "_controller", controller);

            Call(fov, "Awake");
            Assert.That(Get<CinemachineCamera>(fov, "_cinemachineCam"), Is.SameAs(camera));
            SetFov(camera, 80f);
            Call(fov, "OnDisable");

            Assert.That(camera.Lens.FieldOfView, Is.EqualTo(65f).Within(0.001f));
        }

        [Test]
        public void CameraSmoothing_ZeroDeltaUpdatesDoNotPoisonVelocities()
        {
            if (Time.deltaTime != 0f)
                Assert.Ignore("This EditMode tick does not expose a zero delta; verify the paused case in PlayMode.");

            var controller = CreateController(new TestInput());
            var go = InactiveObject("PausedCamera");
            var camera = go.AddComponent<CinemachineCamera>();
            SetFov(camera, 60f);
            var height = go.AddComponent<CameraHeightOffset>();
            Set(height, "_controller", controller);
            go.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var fov = go.AddComponent<DynamicFOV>();
            Set(fov, "_controller", controller);
            Call(fov, "Awake");
            var noise = go.AddComponent<CinemachineBasicMultiChannelPerlin>();
            var shake = go.AddComponent<CameraShakeControl>();
            Set(shake, "_controller", controller);
            Set(shake, "_noise", noise);
            Set(shake, "_defaultFrequency", 1f);
            Set(shake, "_currentFrequency", 1f);

            for (int i = 0; i < 2; i++)
            {
                Call(height, "Update");
                Call(fov, "Update");
                Call(shake, "Update");
            }

            Assert.That(Get<float>(height, "_velocity"), Is.EqualTo(0f));
            Assert.That(Get<float>(fov, "_fovVelocity"), Is.EqualTo(0f));
            Assert.That(Get<float>(shake, "_ampVelocity"), Is.EqualTo(0f));
            Assert.That(Get<float>(shake, "_freqVelocity"), Is.EqualTo(0f));
            Assert.That(go.transform.localPosition.y, Is.EqualTo(1.6f));
            Assert.That(camera.Lens.FieldOfView, Is.EqualTo(60f));
        }

        [TestCase(1f, 2f)]
        [TestCase(1f, 1f)]
        [TestCase(2f, 2f)]
        public void Audio_SurfaceProbeUsesFeetRegardlessOfHeadHeightAndScale(float scale, float height)
        {
            var audio = CreateSurfaceProbe(scale, height, out var controller, out var surfaceClip);
            Physics.SyncTransforms();

            var result = (LocomotionAudio)Call(audio, "GetCurrentLocomotionAudio");

            Assert.That(result.Walk, Is.SameAs(surfaceClip));
            Assert.That(controller.bounds.min.y, Is.EqualTo(0f).Within(0.02f));
        }

        [Test]
        public void Audio_SurfaceProbeIgnoresOwnCollidersEvenWhenHitBufferFills()
        {
            var audio = CreateSurfaceProbe(1f, 2f, out var controller, out var surfaceClip);
            for (int i = 0; i < 10; i++)
            {
                var ownCollider = InactiveObject("OwnSurfaceCollider");
                ownCollider.transform.SetParent(controller.transform);
                ownCollider.transform.position = new Vector3(10000f, 0.02f + i * 0.01f, 10000f);
                var box = ownCollider.AddComponent<BoxCollider>();
                box.size = new Vector3(0.2f, 0.005f, 0.2f);
                ownCollider.SetActive(true);
            }
            Physics.SyncTransforms();

            var result = (LocomotionAudio)Call(audio, "GetCurrentLocomotionAudio");

            Assert.That(result.Walk, Is.SameAs(surfaceClip));
        }

        [Test]
        public void Audio_UnknownSurfaceTagFallsBackWithoutErrors()
        {
            var audio = CreateSurfaceProbe(1f, 2f, out _, out var surfaceClip);
            Set(audio, "_surfaces", new[]
            {
                new SurfaceAudioConfig { SurfaceTag = "SAWC_MissingSurfaceTag", Audio = new LocomotionAudio { Walk = surfaceClip } }
            });
            Physics.SyncTransforms();

            var errors = new List<string>();
            Application.LogCallback onLog = (message, stack, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception) errors.Add(message);
            };
            LocomotionAudio result;
            Application.logMessageReceived += onLog;
            try { result = (LocomotionAudio)Call(audio, "GetCurrentLocomotionAudio"); }
            finally { Application.logMessageReceived -= onLog; }

            Assert.That(errors, Is.Empty);
            Assert.That(result.Walk, Is.SameAs(Get<LocomotionAudio>(audio, "_defaultLocomotion").Walk));
        }

        [Test]
        public void Audio_SurfaceProbeRespectsLayerMask()
        {
            var audio = CreateSurfaceProbe(1f, 2f, out _, out _);
            Set(audio, "_surfaceLayerMask", (LayerMask)(1 << 5));
            Physics.SyncTransforms();

            var result = (LocomotionAudio)Call(audio, "GetCurrentLocomotionAudio");

            Assert.That(result.Walk, Is.SameAs(Get<LocomotionAudio>(audio, "_defaultLocomotion").Walk));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Audio_SurfaceProbeUsesNearestNonTriggerSurface(bool isTrigger)
        {
            var audio = CreateSurfaceProbe(1f, 2f, out _, out var surfaceClip);
            var obstacle = InactiveObject("CloserSurface");
            obstacle.transform.position = new Vector3(10000f, 0.1f, 10000f);
            var collider = obstacle.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.2f, 0.02f, 0.2f);
            collider.isTrigger = isTrigger;
            obstacle.SetActive(true);
            Physics.SyncTransforms();

            var result = (LocomotionAudio)Call(audio, "GetCurrentLocomotionAudio");

            var expected = isTrigger ? surfaceClip : Get<LocomotionAudio>(audio, "_defaultLocomotion").Walk;
            Assert.That(result.Walk, Is.SameAs(expected));
        }

        [Test]
        public void Audio_TakeoffDoesNotPlayDueFootstepFromPreviousGroundedState()
        {
            var audio = CreateSurfaceProbe(1f, 2f, out var character, out _);
            var controller = character.GetComponent<SAWController>();
            var tracker = controller.State;
            var tick = tracker.GetType().GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic);
            var context = new FrameContext { Settings = controller.BaseSettings, IsGrounded = true, DeltaTime = 1f / 60f };
            tick.Invoke(tracker, new object[] { context, Vector3.forward * 3f, false });
            Set(audio, "_stepTimer", 0f);

            // Audio's default-order Update precedes SAWController.Update (order 20).
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            audio.GetType().GetMethod("Update", flags)?.Invoke(audio, null);
            context.IsGrounded = false;
            context.GravityVerticalVelocity = 4f;
            tick.Invoke(tracker, new object[] { context, new Vector3(0f, 4f, 3f), false });
            audio.GetType().GetMethod("LateUpdate", flags)?.Invoke(audio, null);

            Assert.That(Get<AudioSource>(audio, "_stepSource").resource, Is.Null,
                "A due footstep must not play from stale grounding on the takeoff frame.");
            Assert.That(Get<float>(audio, "_stepTimer"), Is.GreaterThanOrEqualTo(0.15f));
        }

        [Test]
        public void StarterPrefabs_HaveDashActionsAndFovReferencesToTheirOwnCameras()
        {
            foreach (string path in PlayerPrefabPaths())
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                var dashes = prefab.GetComponentsInChildren<Dash>(true);
                Assert.That(dashes, Is.Not.Empty, path);
                foreach (var dash in dashes)
                {
                    var reference = Get<InputActionReference>(dash, "_dashAction");
                    Assert.That(reference, Is.Not.Null, path);
                    Assert.That(reference.action, Is.Not.Null, path);
                    Assert.That(reference.action.name, Is.EqualTo("Dash"), path);
                }

                foreach (var fov in prefab.GetComponentsInChildren<DynamicFOV>(true))
                {
                    Assert.That(fov.GetComponent<CinemachineCamera>(), Is.Not.Null, path);
                    Assert.That(Get<CinemachineCamera>(fov, "_cinemachineCam"),
                        Is.SameAs(fov.GetComponent<CinemachineCamera>()), path);
                }
            }
        }

        [Test]
        public void DesktopPrefabs_DoNotKeepMobileReadersWithoutJoysticks()
        {
            string root = PlayerPrefabRoot();
            foreach (string name in new[] { "SAWCFirstPerson", "SAWCThirdPerson", "SAWCHybridPerson" })
            {
                string path = root + "Default/" + name + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                foreach (var reader in prefab.GetComponentsInChildren<MobileUIReader>(true))
                    Assert.That(Get<Object>(reader, "_joystick"), Is.Not.Null, path);
            }
        }

        [Test]
        public void StarterPrefabs_BodyDoesNotKeepStandingCapsuleAlongsideCharacterController()
        {
            foreach (string path in PlayerPrefabPaths())
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                var character = prefab.GetComponentInChildren<CharacterController>(true);
                Assert.That(character, Is.Not.Null, path);
                foreach (var body in character.GetComponentsInChildren<Transform>(true))
                {
                    if (body.name != "Body") continue;
                    Assert.That(body.GetComponent<CapsuleCollider>(), Is.Null,
                        path + ": the visual Body capsule must not preserve standing collision bounds during crouch.");
                }
            }
        }

        [TestCase(-1f)]
        [TestCase(1f)]
        public void ThirdPersonCamera_SustainedStrafeDoesNotSteerBackward(float side)
        {
            string path = PlayerPrefabRoot() + "Default/SAWCThirdPerson.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var source = prefab.GetComponentInChildren<CinemachineOrbitalFollow>(true);
            Assert.That(source, Is.Not.Null);
            Assert.That(Get<float>(source.GetComponent<CameraMotionDynamics>(), "_panAngle"), Is.Zero,
                "Cosmetic strafe pan must not steer the camera-relative movement basis.");
            var target = Keep(new GameObject("Strafe target"));
            var rig = Keep(new GameObject("Strafe camera"));
            var camera = rig.AddComponent<CinemachineCamera>();
            camera.Follow = target.transform;
            camera.LookAt = target.transform;
            var orbit = rig.AddComponent<CinemachineOrbitalFollow>();
            orbit.TrackerSettings = source.TrackerSettings;
            orbit.OrbitStyle = source.OrbitStyle;
            orbit.Orbits = source.Orbits;
            orbit.Radius = source.Radius;
            orbit.TargetOffset = source.TargetOffset;
            orbit.HorizontalAxis.Value = 0f;
            orbit.VerticalAxis.Value = 25f;
            rig.AddComponent<CinemachineHardLookAt>();
            camera.InternalUpdateCameraState(Vector3.up, -1f);
            Vector3 initialForward = Vector3.ProjectOnPlane(camera.State.RawOrientation * Vector3.forward, Vector3.up).normalized;

            for (int i = 0; i < 300; i++)
            {
                Vector3 forward = Vector3.ProjectOnPlane(camera.State.RawOrientation * Vector3.forward, Vector3.up).normalized;
                target.transform.position += Vector3.Cross(Vector3.up, forward) * (side * 3f / 60f);
                camera.InternalUpdateCameraState(Vector3.up, 1f / 60f);
            }

            Assert.That(Vector3.Dot(target.transform.position, initialForward), Is.Zero.Within(0.001f),
                "Horizontal follow lag plus HardLookAt feeds camera yaw back into sustained strafing.");
            Assert.That(target.transform.position.magnitude, Is.EqualTo(15f).Within(0.001f));
        }

        private Dash CreateDash(TestInput input, InputActionReference reference = null)
        {
            var controller = CreateController(input);
            var dash = controller.gameObject.AddComponent<Dash>();
            Set(dash, "_dashAction", reference);
            Set(dash, "_inputBufferTime", 0f);
            Set(dash, "_cooldown", 10f);
            Call(dash, "Awake");
            Call(dash, "OnEnable");
            return dash;
        }

        private SAWController CreateController(TestInput input)
        {
            var go = InactiveObject("RegressionController");
            var controller = go.AddComponent<SAWController>();
            var settings = Keep(ScriptableObject.CreateInstance<CharacterSettings>());
            Set(controller, "_settings", settings);
            controller.SetInputProvider(input);
            Call(controller, "Awake");
            controller.enabled = false;
            return controller;
        }

        private PlayerAudioController CreateSurfaceProbe(float scale, float height,
            out CharacterController character, out AudioClip surfaceClip)
        {
            var controller = CreateController(new TestInput());
            character = controller.GetComponent<CharacterController>();
            character.height = height;
            character.center = Vector3.zero;
            controller.transform.localScale = Vector3.one * scale;
            controller.transform.position = new Vector3(10000f, height * scale * 0.5f, 10000f);

            var head = InactiveObject("HighAudioHead");
            head.transform.SetParent(controller.transform, false);
            head.transform.localPosition = Vector3.up * 5f;
            var audio = head.AddComponent<PlayerAudioController>();
            Set(audio, "_controller", controller);
            surfaceClip = Keep(AudioClip.Create("SurfaceStep", 16, 1, 44100, false));
            var defaultClip = Keep(AudioClip.Create("DefaultStep", 16, 1, 44100, false));
            Set(audio, "_defaultLocomotion", new LocomotionAudio { Walk = defaultClip });
            Set(audio, "_surfaces", new[]
            {
                new SurfaceAudioConfig { SurfaceTag = "Respawn", Audio = new LocomotionAudio { Walk = surfaceClip } }
            });
            Call(audio, "Awake");
            head.SetActive(true);
            controller.gameObject.SetActive(true);
            character.height = height;
            character.center = Vector3.zero;

            var floor = InactiveObject("SurfaceFloor");
            floor.tag = "Respawn";
            floor.transform.position = new Vector3(10000f, -0.05f, 10000f);
            var collider = floor.AddComponent<BoxCollider>();
            collider.size = new Vector3(10f, 0.1f, 10f);
            floor.SetActive(true);
            return audio;
        }

        private GameObject InactiveObject(string name)
        {
            var go = Keep(new GameObject(name));
            go.SetActive(false);
            return go;
        }

        private T Keep<T>(T value) where T : Object
        {
            _created.Add(value);
            return value;
        }

        private static void SetFov(CinemachineCamera camera, float value)
        {
            var lens = camera.Lens;
            lens.FieldOfView = value;
            camera.Lens = lens;
        }

        private static void Set(object target, string name, object value) => Field(target.GetType(), name).SetValue(target, value);
        private static T Get<T>(object target, string name) => (T)Field(target.GetType(), name).GetValue(target);

        private static FieldInfo Field(Type type, string name)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                var field = current.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            throw new MissingFieldException(type.FullName, name);
        }

        private static object Call(object target, string name)
        {
            var method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new MissingMethodException(target.GetType().FullName, name);
            return method.Invoke(target, null);
        }

        private static IEnumerable<string> PlayerPrefabPaths()
        {
            string root = PlayerPrefabRoot();
            yield return root + "SAWCHybrid.prefab";
            yield return root + "Default/SAWCFirstPerson.prefab";
            yield return root + "Default/SAWCThirdPerson.prefab";
            yield return root + "Default/SAWCHybridPerson.prefab";
            yield return root + "Mobile/SAWCFirstPersonMobile 1.prefab";
            yield return root + "Mobile/SAWCThirdPersonMobile.prefab";
        }

        private static string PlayerPrefabRoot()
        {
            var package = PackageInfo.FindForAssembly(typeof(SAWController).Assembly);
            Assert.That(package, Is.Not.Null, "Install SAWC as a Unity package before running these tests.");
            var sample = Sample.FindByPackage(package.name, package.version)
                .Single(candidate => candidate.displayName == "Starter Assets");
            string importPath = FileUtil.GetProjectRelativePath(sample.importPath);
            if (!AssetDatabase.IsValidFolder(importPath))
                Assert.Ignore("Import the Starter Assets sample from Package Manager to run this test.");

            return importPath + "/Prefabs/Player/";
        }
    }
}
