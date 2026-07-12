using System;
using System.Collections;
using System.Reflection;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Mapping;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests.PlayMode.Mapping
{
    public class PlayerSpatialPositionerTests
    {
        private readonly ArcGISSpatialReference sr = ArcGISSpatialReference.WGS84();
        private GameObject camera;
        private ArcGISLocationComponent cameraLoc;
        private PlayerSpatialPositioner compass;
        private GameObject map;
        private GameObject player;
        private ArcGISLocationComponent playerLoc;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            map = new GameObject("MapRoot");
            map.AddComponent<ArcGISMapComponent>();

            player = new GameObject("player");
            player.transform.SetParent(map.transform, false);
            playerLoc = player.AddComponent<ArcGISLocationComponent>();
            camera = new GameObject("camera");
            camera.transform.SetParent(map.transform, false);
            cameraLoc = camera.AddComponent<ArcGISLocationComponent>();

            compass = player.AddComponent<PlayerSpatialPositioner>();
            TestHelpers.SetPrivate(compass, "arcLocation", playerLoc);
            TestHelpers.SetPrivate(compass, "cameraLocation", cameraLoc);

            playerLoc.Position = new ArcGISPoint(1, 1, 10, sr);
            playerLoc.Rotation = new ArcGISRotation(0, 0, 0);
            cameraLoc.Rotation = new ArcGISRotation(45, 0, 0);

            compass.enabled = true;
            Time.captureDeltaTime = 0.02f;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(player);
            Object.Destroy(camera);
            Object.Destroy(map);
            yield return null;
        }

        /*[UnityTest]
        public IEnumerator UpdateCompassRotation_UsesDebugHeadingIfEnabled()
        {
            TestHelpers.SetPrivate(compass, "useDebugHeading", true);
            TestHelpers.SetPrivate(compass, "isCameraRotationEnabled", true);
            TestHelpers.SetPrivate(compass, "debugHeading", 180f);
            TestHelpers.SetPrivate(compass, "playerSmoothSpeed", 100f);

            for (var i = 0; i < 5; i++) yield return null;

            var heading = (float)playerLoc.Rotation.Heading;
            Assert.AreEqual(
                180f,
                heading,
                1f,
                "Heading should follow debug heading if debug heading is enabled"
            );
        }*/

        /*[UnityTest]
        public IEnumerator UpdateCompassRotation_DoesNotUseDebugHeadingIfDisabled()
        {
            TestHelpers.SetPrivate(compass, "useDebugHeading", false);
            TestHelpers.SetPrivate(compass, "isCameraRotationEnabled", true);
            TestHelpers.SetPrivate(compass, "debugHeading", 180f);
            TestHelpers.SetPrivate(compass, "playerSmoothSpeed", 100f);

            for (var i = 0; i < 5; i++) yield return null;

            var heading = (float)playerLoc.Rotation.Heading;
            Assert.AreEqual(
                0f,
                heading,
                1f,
                "Heading should not follow debug heading if debug heading is disabled"
            );
        }*/

        [UnityTest]
        public IEnumerator ToggleCameraRotation_LocksCurrentHeading()
        {
            TestHelpers.SetPrivate(compass, "isCameraRotationEnabled", true);
            TestHelpers.SetPrivate(compass, "targetCameraHeading", 180f);
            TestHelpers.SetPrivate(compass, "lockedCameraHeading", 360f);

            compass.ToggleCameraRotation();
            yield return null;

            Assert.IsFalse(
                TestHelpers.GetPrivate<bool>(compass, "isCameraRotationEnabled"),
                "Rotation should be disabled after toggle off"
            );

            var locked = TestHelpers.GetPrivate<float>(compass, "lockedCameraHeading");

            Assert.AreEqual(
                180f,
                locked,
                1f,
                "Heading should lock once or if isCameraRotationEnabled is true"
            );
        }

        [UnityTest]
        public IEnumerator ToggleCameraRotation_UnlocksCurrentHeading()
        {
            TestHelpers.SetPrivate(compass, "isCameraRotationEnabled", false);
            TestHelpers.SetPrivate(compass, "targetCameraHeading", 360f);
            TestHelpers.SetPrivate(compass, "lockedCameraHeading", 360f);

            TestHelpers.SetPrivate(compass, "smoothedHeading", 180f);
            TestHelpers.SetPrivate(compass, "cameraRotationSmoothSpeed", 50f);

            compass.ToggleCameraRotation();
            yield return null;

            Assert.IsTrue(
                TestHelpers.GetPrivate<bool>(compass, "isCameraRotationEnabled"),
                "Rotation should be enabled after toggle on"
            );

            var updateCameraRotation = typeof(PlayerSpatialPositioner).GetMethod(
                "UpdateCameraRotation",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            for (var i = 0; i < 5; i++)
            {
                updateCameraRotation.Invoke(compass, new object[] { 180f });
                yield return null;
            }

            var target = TestHelpers.GetPrivate<float>(compass, "targetCameraHeading");
            var locked = TestHelpers.GetPrivate<float>(compass, "lockedCameraHeading");

            Assert.AreEqual(360f, locked, 0.1f, "Locked heading should remain unchanged when enabling");

            Assert.Greater(
                target,
                200f,
                "Target heading should start moving toward the smoothed heading"
            );
        }

        /*[UnityTest]
        public IEnumerator GetCompassHeading_ReturnsNegativeIfUnavailable()
        {
            Input.compass.enabled = false;

            var GetCompassHeading = typeof(CompassRotation).GetMethod(
                "GetCompassHeading",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var value = (float)GetCompassHeading.Invoke(compass, null);

            Assert.Less(
                value,
                0f,
                "GetCompassHeading should return a negative value when compass is unavailable"
            );

            yield break;
        }*/

        // Verify the maths, which is adapted from the following Stack Overflow query:
        // https://gis.stackexchange.com/questions/2951/
        [UnityTest]
        public IEnumerator CalculateCameraTargetPosition_PlacesCameraBehindPlayer()
        {
            TestHelpers.SetPrivate(compass, "distanceBehind", 50f);
            TestHelpers.SetPrivate(compass, "altitudeOffset", 12f);

            var CalculateCameraTargetPosition = typeof(PlayerSpatialPositioner).GetMethod(
                "CalculateCameraTargetPosition",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var playerPos = new ArcGISPoint(1, 1, 10, sr);
            var heading = 30f;

            var result = CalculateCameraTargetPosition.Invoke(
                compass,
                new object[] { playerPos, heading }
            );

            var lat = (double)result.GetType().GetField("Latitude").GetValue(result);
            var lon = (double)result.GetType().GetField("Longitude").GetValue(result);
            var alt = (double)result.GetType().GetField("Altitude").GetValue(result);

            const double EarthRadius = 6378137.0;

            var distanceBehind = 50f;
            var altitudeOffset = 12f;

            double behindHeadingRad = (heading + 180f) * Mathf.Deg2Rad;
            var dn = distanceBehind * Math.Cos(behindHeadingRad);
            var de = distanceBehind * Math.Sin(behindHeadingRad);
            var latRad = playerPos.Y * Mathf.Deg2Rad;

            var dLat = dn / EarthRadius * Mathf.Rad2Deg;
            var dLon = de / (EarthRadius * Math.Cos(latRad)) * Mathf.Rad2Deg;

            var expectedLat = playerPos.Y + dLat;
            var expectedLon = playerPos.X + dLon;
            var expectedAlt = playerPos.Z + altitudeOffset;

            Assert.AreEqual(expectedLat, lat, 1e-5);
            Assert.AreEqual(expectedLon, lon, 1e-5);
            Assert.AreEqual(expectedAlt, alt, 1e-3);
            yield break;
        }
    }
}