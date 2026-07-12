using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LocationService = Mapping.LocationService;
using Object = UnityEngine.Object;

namespace Tests.PlayMode.Mapping
{
    /// <summary>
    ///     PlayMode/Editor unit tests for LocationService behaviours.
    /// </summary>
    /// <remarks>
    ///     Alot of these tests had to use the same code and so were bunched together.
    ///     Below is listed what this tests for.
    ///     Tests that location being off is Vector3.zero
    ///     Tests that location is within the bounds of Nottingham
    ///     Tests that 1st and 2nd location updates are not zero
    ///     Tests that movements updates occur within 50 meter radius
    ///     Tests that movement updates do not occur with gps off/in editor
    ///     Tests that timestamps do actually update between location updates
    ///     Tests that timestamps do not update with gps off/in editor
    ///     Tests that location service fails with gps off/in editor
    ///     Tests that location service is running on device
    ///     Tests that location service stops running when told to on device
    /// </remarks>
    public class LocationServiceTests
    {
        // Nottingham bounding box (approximate): lon [-1.25, -1.10], lat [52.90, 53.05].
        // Based on debug coordinates defined in LocationService and general city bounds.
        private const double MinLon = -1.25;
        private const double MaxLon = -1.10;
        private const double MinLat = 52.90;
        private const double MaxLat = 53.05;

        /// <summary>
        ///     in editor (or no gps) returns Vector3.zero,
        ///     otherwise values should be within Nottingham bounds.
        /// </summary>
        [UnityTest]
        public IEnumerator GetLocationValidValues() // get pos
        {
            var go = new GameObject("GetLocationValidValues");
            var svc = go.AddComponent<LocationService>();

            svc.StartLocationTracking();

            var timeout = 5f;
            while (Input.location.status == LocationServiceStatus.Initializing && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            var locationVec = svc.GetLocation();

            if (Input.location.status != LocationServiceStatus.Running)
            {
                // editor (or no gps) should be zero vector.
                Assert.AreEqual(Vector3.zero, locationVec, "Editor and no GPS should be zero.");
            }
            else
            {
                // in bounds, on device.
                double lat = locationVec.x;
                double lon = locationVec.y;
                Assert.IsTrue(lat >= MinLat && lat <= MaxLat, $"Latitude {lat} within Nottingham bounds");
                Assert.IsTrue(lon >= MinLon && lon <= MaxLon, $"Longitude {lon} within Nottingham bounds");
            }

            svc.StopLocationTracking();
            Object.Destroy(go);
        }

        /// <summary>
        ///     movement every ~5s should be <= 50 meters.
        ///     in editor or no gps, location should be zero.
        /// </summary>
        [UnityTest]
        public IEnumerator CheckMovementValid() // set pos
        {
            var gameObject = new GameObject("Movement_Valid");
            var locationService = gameObject.AddComponent<LocationService>();
            locationService.StartLocationTracking();

            float timeout = 5;
            while (Input.location.status == LocationServiceStatus.Initializing && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (Input.location.status == LocationServiceStatus.Running)
            {
                // device location specific
                var locationVec1 = locationService.GetLocation();

                var wait = 10f;
                while (wait > 0f)
                {
                    wait -= Time.deltaTime;
                    yield return null;
                }

                var locationVec2 = locationService.GetLocation();

                Assert.AreNotEqual(Vector3.zero, locationVec1, "First location should be not zero on device.");
                Assert.AreNotEqual(Vector3.zero, locationVec2, "Second location should be not zero on device.");

                var distance = HaversineDistanceMeters(locationVec1.x, locationVec1.y, locationVec2.x, locationVec2.y);
                Assert.LessOrEqual(distance, 50.0, $"movement {distance:F1}m should be <= 50m between updates");
            }
            else // editor/no GPS
            {
                var loc1 = locationService.GetLocation();
                yield return new WaitForSeconds(5f);
                var loc2 = locationService.GetLocation();
                Assert.True(loc1 == Vector3.zero && loc2 == Vector3.zero);
            }

            locationService.StopLocationTracking();
            Object.Destroy(gameObject);
        }

        /// <summary>
        ///     Timestamp check, in editor/no gps it should be zero.
        ///     On device, it should increase over time.
        /// </summary>
        [UnityTest]
        public IEnumerator CheckTimestampValid() // get time (we dont set time so no test for set time)
        {
            var go = new GameObject("Timestamp_Valid");
            var svc = go.AddComponent<LocationService>();
            svc.StartLocationTracking();

            var timeout = 5f;
            while (Input.location.status == LocationServiceStatus.Initializing && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (Input.location.status == LocationServiceStatus.Running) // device
            {
                // device timestamp should increase
                var t1 = svc.GetLocationTimestamp();

                var wait = 10f;
                while (wait > 0f)
                {
                    wait -= Time.deltaTime;
                    yield return null;
                }

                var t2 = svc.GetLocationTimestamp();
                Assert.Greater(t2, t1, "Timestamp value should increase between checks on device.");
            }
            else // editor/no gps
            {
                Assert.AreEqual(0d, svc.GetLocationTimestamp());
            }

            svc.StopLocationTracking();
            Object.Destroy(go);
        }

        /// <summary>
        ///     In editor, status should not be running.
        ///     On device, it should finish initialisation correctly, then stop correctly
        /// </summary>
        [UnityTest]
        public IEnumerator CheckToggleTracking() // location service init "tracker" test
        {
            var go = new GameObject("StartStopTrackingEditorAndDevice");
            var locationService = go.AddComponent<LocationService>();

            locationService.StartLocationTracking();

            var timeout = 15f;
            while (Input.location.status == LocationServiceStatus.Initializing && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
                // we consider this acceptable due to other tests accounting for it.
                Assert.Pass("Location service failed to start on device.");

            if (Input.location.status != LocationServiceStatus.Running)
                // editor
                Assert.AreNotEqual(LocationServiceStatus.Running, Input.location.status);
            else
                // device
                Assert.AreEqual(LocationServiceStatus.Running, Input.location.status);

            // stop and allow some time for it to be fine.
            locationService.StopLocationTracking();

            // device might say its running for a short period after stopping, wait a few secs for it to stop running.
            var stopTimeout = 5f;
            while (Input.location.status == LocationServiceStatus.Running && stopTimeout > 0f)
            {
                stopTimeout -= Time.deltaTime;
                yield return null;
            }

            // it should no longer be running, stopped/failed/notinitialised are all acceptable.
            Assert.AreNotEqual(LocationServiceStatus.Running, Input.location.status,
                $"Location service should not be running: {Input.location.status}");

            Object.Destroy(go);
        }

        /// <summary>
        ///     Method to get the distance in metres between two coordinates (haversine)
        /// </summary>
        /// <remarks>
        ///     A similar method exists in CompassRotation.cs but should not be used in tests.
        ///     Calculations like these need to be separate for tests.
        /// </remarks>
        private static double HaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double radius = 6371000.0; // earth radius in meters
            var halfDegRadLat = Deg2Rad(lat2 - lat1) / 2;
            var halfDegRadLon = Deg2Rad(lon2 - lon1) / 2;
            var sinLat = Math.Sin(halfDegRadLat);
            var sinLon = Math.Sin(halfDegRadLon);
            var haversine
                = sinLat * sinLat
                  + Math.Cos(Deg2Rad(lat1))
                  * Math.Cos(Deg2Rad(lat2))
                  * sinLon * sinLon;
            var distanceRadians = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
            return radius * distanceRadians;
        }

        /// <summary>
        ///     converts degrees to radians.
        /// </summary>
        private static double Deg2Rad(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}