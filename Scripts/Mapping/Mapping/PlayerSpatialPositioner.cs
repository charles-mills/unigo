using System;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;

namespace Mapping
{
    /// <summary>
    ///     Handles player rotation based on device compass direction and has a camera that follow
    ///     behind the player with smoothing applied to both movement and rotation.
    /// </summary>
    [RequireComponent(typeof(ArcGISLocationComponent))]
    public class PlayerSpatialPositioner : MonoBehaviour
    {
        private const double EarthRadius = 6378137.0;

        [Header("Location Components")]
        [SerializeField]
        private ArcGISLocationComponent arcLocation;

        [SerializeField]
        private ArcGISLocationComponent cameraLocation;

        [Header("Camera Positioning")]
        [SerializeField]
        private float distanceBehind = 63f;

        [SerializeField]
        private float altitudeOffset = 38f;

        [SerializeField]
        private float cameraPitch = 55f;

        [Header("Camera Control")]
        [SerializeField]
        private bool isCameraRotationEnabled;

        [Header("Compass Settings")]
        [SerializeField]
        private float playerSmoothSpeed = 8f;

        [Header("Camera Smoothing")]
        [SerializeField]
        private float cameraPositionSmoothSpeed = 3f;

        [SerializeField]
        private float cameraRotationSmoothSpeed = 5f;

        [Header("Debug Variables")]
        [SerializeField]
        [Range(0f, 360f)]
        private float debugHeading;

        private bool isInitialised;
        private float lockedCameraHeading;
        private float smoothedHeading;
        private double targetCameraAlt;

        private float targetCameraHeading;
        private double targetCameraLat;
        private double targetCameraLon;

        /// <summary>
        ///     Unity awake hook. Ensures required references exist and resets state.
        /// </summary>
        private void Awake()
        {
            if (arcLocation == null) arcLocation = GetComponent<ArcGISLocationComponent>();

            isInitialised = false;
        }

        /// <summary>
        ///     Unity update loop. Updates player compass rotation and camera rotation/look.
        /// </summary>
        private void Update()
        {
            UpdateCompassRotation();
            UpdateCameraLook();
        }

        /// <summary>
        ///     Unity enable hook. Enables the device compass and initialises headings.
        /// </summary>
        private void OnEnable()
        {
            Input.compass.enabled = true;
            if (arcLocation != null) smoothedHeading = (float)arcLocation.Rotation.Heading;

            if (cameraLocation != null) lockedCameraHeading = (float)cameraLocation.Rotation.Heading;
            targetCameraHeading = lockedCameraHeading;
        }

        /// <summary>
        ///     Toggles whether the camera should rotate to follow the player's heading.
        /// </summary>
        public void ToggleCameraRotation()
        {
            if (isCameraRotationEnabled) lockedCameraHeading = targetCameraHeading;
            isCameraRotationEnabled = !isCameraRotationEnabled;
        }

        /// <summary>
        ///     Syncs the player's facing direction with the compass heading.
        /// </summary>
        private void UpdateCompassRotation()
        {
            if (arcLocation == null) return;

            var heading = GetCompassHeading();

            if (heading < 0f) return; // GetCompassHeading will return -1 if unavailable

            smoothedHeading = Mathf.LerpAngle(
                smoothedHeading,
                heading,
                Time.deltaTime * playerSmoothSpeed
            );

            var rotation = arcLocation.Rotation;
            arcLocation.Rotation = new ArcGISRotation(smoothedHeading, rotation.Pitch, rotation.Roll);
        }

        /// <summary>
        ///     Gets the compass heading in degrees, or -1 if unavailable.
        /// </summary>
        /// <returns>The compass heading in degrees, or -1 if not available.</returns>
        public float GetCompassHeading()
        {
            // If compass heading is unavailable then the debugHeading is used,
            // it is likely preferable for this to be removed (perhaps the boolean flag to use
            // debug heading reintroduced) once it is less useful to quickly switch from on-device compass testing
            // to in-editor compass testing.
            if (!Input.compass.enabled || Input.compass.timestamp == 0) return debugHeading;
            ;

            return Input.compass.trueHeading;
        }

        /// <summary>
        ///     Updates the target camera position and rotation based on the player's position and heading,
        ///     then applies smoothing and applies the transform to the ArcGIS camera location.
        /// </summary>
        private void UpdateCameraLook()
        {
            if (arcLocation == null || cameraLocation == null)
            {
                Debug.LogWarning("Missing location references");
                return;
            }

            var playerPos = arcLocation.Position;

            if (playerPos == null)
            {
                Debug.LogWarning("Player position is null");
                return;
            }

            var effectiveHeading = GetEffectiveCameraHeading();
            var targetPosition = CalculateCameraTargetPosition(playerPos, effectiveHeading);

            InitialiseCameraIfNeeded(targetPosition, effectiveHeading);
            UpdateCameraPosition(targetPosition);
            UpdateCameraRotation(smoothedHeading);

            ApplyCameraTransform(playerPos.SpatialReference);
        }

        /// <summary>
        ///     Calculates the camera position at a fixed position behind the player for a given heading.
        /// </summary>
        /// <param name="playerPos">Player position (ArcGIS coordinates).</param>
        /// <param name="heading">Heading in degrees.</param>
        /// <returns>Desired camera position (lat, lon, alt).</returns>
        private CameraTargetPosition CalculateCameraTargetPosition(ArcGISPoint playerPos, float heading)
        {
            var behindHeadingRad = GetBehindHeadingRad(heading);

            double dn = distanceBehind * Mathf.Cos(behindHeadingRad);
            double de = distanceBehind * Mathf.Sin(behindHeadingRad);

            var latRadians = playerPos.Y * Mathf.Deg2Rad;

            var dLat = dn / EarthRadius * Mathf.Rad2Deg;
            var dLon = de / (EarthRadius * Math.Cos(latRadians)) * Mathf.Rad2Deg;

            return new CameraTargetPosition
            {
                Latitude = playerPos.Y + dLat,
                Longitude = playerPos.X + dLon,
                Altitude = playerPos.Z + altitudeOffset
            };
        }

        /// <summary>
        ///     Performs one-time camera initialisation when valid target data becomes available.
        /// </summary>
        /// <param name="target">Target camera position.</param>
        /// <param name="heading">Initial heading in degrees.</param>
        private void InitialiseCameraIfNeeded(CameraTargetPosition target, float heading)
        {
            if (isInitialised) return;

            if (target.Latitude == 0 && target.Longitude == 0) return;

            targetCameraLat = target.Latitude;
            targetCameraLon = target.Longitude;
            targetCameraAlt = target.Altitude;

            targetCameraHeading = heading;
            lockedCameraHeading = heading;

            isInitialised = true;
        }

        /// <summary>
        ///     Smoothly updates the stored target camera position towards the desired target.
        /// </summary>
        /// <param name="target">Desired camera target position.</param>
        private void UpdateCameraPosition(CameraTargetPosition target)
        {
            var lerpFactor = Time.deltaTime * cameraPositionSmoothSpeed;

            targetCameraLat += (target.Latitude - targetCameraLat) * lerpFactor;
            targetCameraLon += (target.Longitude - targetCameraLon) * lerpFactor;
            targetCameraAlt += (target.Altitude - targetCameraAlt) * lerpFactor;
        }

        /// <summary>
        ///     Smoothly updates the stored target camera heading when camera rotation is enabled.
        /// </summary>
        /// <param name="desiredHeading">Desired heading in degrees.</param>
        private void UpdateCameraRotation(float desiredHeading)
        {
            if (!isCameraRotationEnabled) return;

            var lerpFactor = Time.deltaTime * cameraRotationSmoothSpeed;
            targetCameraHeading = Mathf.LerpAngle(targetCameraHeading, desiredHeading, lerpFactor);
        }

        /// <summary>
        ///     Applies the calculated camera position and rotation to the ArcGIS camera location.
        /// </summary>
        /// <param name="spatialReference">The spatial reference for the ArcGIS point.</param>
        private void ApplyCameraTransform(ArcGISSpatialReference spatialReference)
        {
            cameraLocation.Position = new ArcGISPoint(
                targetCameraLon,
                targetCameraLat,
                targetCameraAlt,
                spatialReference
            );
            cameraLocation.Rotation = new ArcGISRotation(targetCameraHeading, cameraPitch, 0);
        }

        /// <summary>
        ///     Converts a heading to radians and calculates the behind direction.
        /// </summary>
        /// <param name="headingForPositioning">Heading in degrees.</param>
        /// <returns>Angle in radians pointing behind the heading.</returns>
        private float GetBehindHeadingRad(float headingForPositioning)
        {
            return (headingForPositioning + 180f) * Mathf.Deg2Rad;
        }

        /// <summary>
        ///     Gets the heading to use for the camera (either smoothed player heading or locked heading).
        /// </summary>
        /// <returns>Effective camera heading in degrees.</returns>
        private float GetEffectiveCameraHeading()
        {
            return isCameraRotationEnabled ? smoothedHeading : lockedCameraHeading;
        }

        private struct CameraTargetPosition
        {
            public double Latitude;
            public double Longitude;
            public double Altitude;
        }
    }
}