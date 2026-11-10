using System;
using System.Reflection;
using NUnit.Framework;
using SAWC.Core;
using SAWC.Core.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAWC.Tests.Editor
{
    public class PostureScaleRegressionTests
    {
        [TestCase(0f)]
        [TestCase(90f)]
        public void WideUprightController_CanStandOnFloorWithoutCeiling(float yaw)
        {
            GameObject player = null, floor = null;
            CharacterSettings settings = null;
            try
            {
                settings = ScriptableObject.CreateInstance<CharacterSettings>();
                player = new GameObject("SAWC wide posture regression");
                player.transform.position = new Vector3(20000f, 100f, 20000f);
                player.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                player.transform.localScale = new Vector3(3f, 1f, 1f);
                var controller = player.AddComponent<CharacterController>();
                controller.radius = 0.5f;
                controller.height = 2f;
                controller.center = Vector3.up;

                Type type = typeof(SAWController).Assembly.GetType("SAWC.Core.CharacterPosture", true);
                object posture = Activator.CreateInstance(type,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new object[] { controller, player.transform, settings.Data }, null);
                Invoke(posture, "Tick", true, settings.Data);
                Physics.SyncTransforms();
                Assert.That((bool)Invoke(posture, "CanStandUp", settings.Data), Is.True,
                    "No unrelated scene obstacle should affect the isolated test location.");

                // Put the floor immediately below the actual native capsule bounds,
                // not below an assumed local-height representation of the capsule.
                Bounds bounds = controller.bounds;
                floor = new GameObject("SAWC posture regression floor");
                floor.transform.position = new Vector3(bounds.center.x,
                    bounds.min.y - 0.51f, bounds.center.z);
                var box = floor.AddComponent<BoxCollider>();
                box.size = new Vector3(20f, 1f, 20f);
                Physics.SyncTransforms();
                Assert.That(controller.bounds.min.y, Is.GreaterThan(box.bounds.max.y));

                Assert.That((bool)Invoke(posture, "CanStandUp", settings.Data), Is.True,
                    "A supporting floor must not be reported as an overhead obstacle when width exceeds crouched height.");
            }
            finally
            {
                if (floor != null) Object.DestroyImmediate(floor);
                if (player != null) Object.DestroyImmediate(player);
                if (settings != null) Object.DestroyImmediate(settings);
                Physics.SyncTransforms();
            }
        }

        private static object Invoke(object target, string name, params object[] arguments)
            => target.GetType().GetMethod(name,
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
    }
}
