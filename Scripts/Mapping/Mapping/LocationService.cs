using System;
using System.Collections;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Mapping
{
    /// <summary>
    ///     Wraps Unity's LocationService to provide GPS tracking and exposes helper methods
    ///     for pushing location updates into ArcGIS components and other systems.
    /// </summary>
    [ExecuteAlways]
    public class LocationService : MonoBehaviour
    {
        /// <summary>
        ///     WGS84 spatial reference used for ArcGIS points.
        /// </summary>
        private static readonly ArcGISSpatialReference WGS84 = new(4326);

        [Header("Debug Options")]
        [SerializeField]
        private Vector3[] debugLocations =
        {
            new(-1.185f, 52.953f, 20f), // jubilee :-)
            new(-1.190f, 52.955f, 20f), // probably also Jubilee :->
            new(-1.182f, 52.951f, 20f) // also Jubilee
        };

        [Range(0, 2)]
        [SerializeField]
        private int debugLocationIndex;

        [SerializeField]
        private bool useDebugLocation = true;

        [SerializeField]
        private ArcGISLocationComponent playerLocation;

        [Header("Tracking Settings")]
        [SerializeField]
        private float accuracyMeters = 10f;

        [SerializeField]
        private float updateMeters = 10f;

        [SerializeField]
        [Min(0.1f)]
        private float pollingRateSeconds = 5f;

        [SerializeField]
        [Min(0.1f)]
        private float initialWaitSeconds = 20f;

        /// <summary>
        ///     The current debug location value when <see cref="useDebugLocation" /> is enabled.
        /// </summary>
        public static Vector3 DebugLocation { get; private set; }

        /// <summary>
        ///     Unity awake hook. Initialises the debug location if enabled.
        /// </summary>
        private void Awake()
        {
            if (useDebugLocation && debugLocations.Length > 0)
            {
                var index = Mathf.Clamp(debugLocationIndex, 0, debugLocations.Length - 1);
                DebugLocation = debugLocations[index];
            }
        }

        /// <summary>
        ///     Unity start hook. Begins GPS tracking coroutine when playing.
        /// </summary>
        private void Start()
        {
            StartCoroutine(StartTracking());
        }

        /// <summary>
        ///     Editor validation hook to update the debug position when parameters change.
        /// </summary>
        private void OnValidate()
        {
            if (useDebugLocation && debugLocations.Length > 0)
            {
                var index = Mathf.Clamp(debugLocationIndex, 0, debugLocations.Length - 1);
                var location = debugLocations[index];
                DebugLocation = location;
                SetPosition(location.x, location.y, location.z);
            }
        }

        /// <summary>
        ///     Starts the device location service, waits for initialisation and periodically
        ///     applies GPS updates to the player.
        /// </summary>
        private IEnumerator StartTracking()
        {
            if (!Application.isPlaying)
                yield break;

#if UNITY_ANDROID && !UNITY_EDITOR // force get android location permission.
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);

            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                yield return null;
            }
        }
#endif
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("Location services are not enabled by the user.");
                yield break;
            }

            StartLocationTracking();

            if (!PerformChecks())
                yield break;

            // wait initialWaitSeconds seconds for init
            var waitCounter = initialWaitSeconds;
            while (Input.location.status == LocationServiceStatus.Initializing && waitCounter > 0)
            {
                yield return new WaitForSeconds(1);
                waitCounter--;
            }

            if (waitCounter <= 0)
            {
                Debug.LogWarning("Location service init timed out.");
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("Location service failed to get device location.");
                yield break;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning("Location service is not running after init.");
                yield break;
            }

            UpdateFromGps();

            while (true)
            {
                var lastTimestamp = GetLocationTimestamp();
                yield return new WaitForSeconds(pollingRateSeconds);

                if ((int)Math.Round(lastTimestamp) == (int)Math.Round(GetLocationTimestamp()))
                    continue; // add rounding for floating point errors.
                UpdateFromGps();
            }
        }

        /// <summary>
        ///     Reads the latest GPS values from Unity and forwards them to the player position.
        /// </summary>
        /// <remarks>
        ///     I have renamed (and reworked) this as camera is now
        ///     handled in CompassRotation (which is now a bad name for the script...)
        ///     - Charles Mills
        /// </remarks>
        private void UpdateFromGps()
        {
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning("Location service is not running.");
                return;
            }

            double lon = Input.location.lastData.longitude;
            double lat = Input.location.lastData.latitude;
            double alt = Input.location.lastData.altitude;

            SetPosition(lon, lat, alt);
        }

        /// <summary>
        ///     Sets the player's ArcGIS position to the given coordinates.
        /// </summary>
        /// <param name="lon">Longitude in decimal degrees.</param>
        /// <param name="lat">Latitude in decimal degrees.</param>
        /// <param name="alt">Altitude in meters.</param>
        private void SetPosition(double lon, double lat, double alt)
        {
            if (playerLocation == null) return;

            // Since we're now grounding the player we can just grab their current altitude to avoid
            // un-grounding them :)
            var currentAlt = playerLocation.Position?.Z ?? alt;
            var point = new ArcGISPoint(lon, lat, currentAlt, WGS84);
            playerLocation.Position = point;

            Debug.LogFormat("Pushed GPS -> ArcGISPoint lon:{0} lat:{1} alt:{2}", lon, lat, currentAlt);
        }

        /// <summary>
        ///     Gets the current device location from Unity's location service.
        /// </summary>
        /// <returns>Vector3 where x=lat, y=lon, z=alt. Returns Vector3.zero if unavailable.</returns>
        public Vector3 GetLocation()
        {
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning("Location service is not running.");
                return Vector3.zero;
            }

            var lat = Input.location.lastData.latitude;
            var lon = Input.location.lastData.longitude;
            var alt = Input.location.lastData.altitude;

            Debug.LogFormat("Location: lat={0}, lon={1}, alt={2}", lat, lon, alt);
            return new Vector3(lat, lon, alt);
        }

        /// <summary>
        ///     Gets the timestamp of the last GPS update.
        /// </summary>
        /// <returns>Timestamp in seconds since last GPS update, or 0 if unavailable.</returns>
        public double GetLocationTimestamp()
        {
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning("Location service is not running.");
                return 0d;
            }

            return Input.location.lastData.timestamp;
        }

        /// <summary>
        ///     Stops Unity's location service.
        /// </summary>
        public void StopLocationTracking()
        {
            Input.location.Stop();
        }

        /// <summary>
        ///     Starts Unity's location service with the configured accuracy and distance parameters.
        /// </summary>
        public void StartLocationTracking()
        {
            Input.location.Start(accuracyMeters, updateMeters);
        }

        /// <summary>
        ///     Performs sanity checks for required references and platform constraints.
        /// </summary>
        /// <returns>True if checks pass; otherwise false.</returns>
        private bool PerformChecks()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("Location is not enabled by the user.");
                return false;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogWarning("Failed to get device location.");
                return false;
            }

            return true;
        }
    }
}