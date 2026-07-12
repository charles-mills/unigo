using System;
using System.Collections;
using APIs_Helpers;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Layers.Base;
using Esri.GameEngine.Map;
using UnityEngine;

/*

TODO:

- Actually implement a lightmode basemap, for now it just points to the same URL as darkmode.
- Return a boolean when loaded (or not loaded if failed) for loading screen / progress.
    We could also handle network issues in the suntime API and display them on load but it probably won't be necessary.

*/

namespace Mapping
{
    /// <summary>
    ///     Switches the ArcGIS basemap between light and dark styles based on time of day or explicit calls.
    ///     Also handles initial basemap selection at startup.
    /// </summary>
    public class BasemapSwitcher : MonoBehaviour
    {
        [SerializeField]
        private ArcGISMapComponent mapComponent;

        [SerializeField]
        private string apiKey;

        [SerializeField]
        private LocationService locationService;

        /*
    A remote basemap adds a small delay on startup, but this won't be an issue once we add a loading screen anyway.
    Once it's finalised it's also possible to locally install the basemap instead of fetching it :)
    */
        [SerializeField]
        private string basemapDarkId = "aaf9e10544a64f5c9e6cbe34740839a5";

        [SerializeField]
        private string basemapLightId = "aaf9e10544a64f5c9e6cbe34740839a5";

        /// <summary>
        ///     Unity start hook. Begins asynchronous init of the basemap, based on current location and sunrise/sunset times.
        /// </summary>
        public void Start()
        {
            StartCoroutine(InitBasemap());
        }

        /// <summary>
        ///     Sets the map's basemap to dark mode.
        /// </summary>
        public void SetDarkMode()
        {
            SetBasemapById(basemapDarkId);
        }

        /// <summary>
        ///     Sets the map's basemap light mode.
        /// </summary>
        public void SetLightMode()
        {
            SetBasemapById(basemapLightId);
        }

        /// <summary>
        ///     Applies a basemap to the ArcGIS map by its ArcGIS Online item ID.
        /// </summary>
        /// <param name="id">ArcGIS Basemap ID</param>
        public void SetBasemapById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("The basemap ID is missing!");
                return;
            }

            var url =
                $"https://www.arcgis.com/sharing/rest/content/items/{id}/resources/styles/root.json";

            var basemap = new ArcGISBasemap(url, ArcGISLayerType.ArcGISVectorTileLayer, apiKey);
            var map = new ArcGISMap(basemap, ArcGISMapType.Global);
            mapComponent.Map = map;

            Debug.Log($"Basemap has been set to a URL: {url}");
        }

        /// <summary>
        ///     Determines whether to use the light or dark basemap by checking the device location and SunTime API.
        /// </summary>
        private IEnumerator InitBasemap()
        {
            yield return WaitForLocation(15f);

            var coords = locationService.GetLocation();
            var debugLoc = LocationService.DebugLocation;

            var localTesting = coords.x == 0 && coords.y == 0;
            if (localTesting) coords = debugLoc;

            var nowUtc = DateTime.UtcNow;

            yield return SunTimeAPI.GetIsNight(
                coords.x,
                coords.y,
                nowUtc,
                isNight =>
                {
                    if (isNight)
                        SetDarkMode();
                    else
                        SetLightMode();
                }
            );
        }

        /// <summary>
        ///     Waits for Unity's location services to initialise or until a timeout passes.
        /// </summary>
        /// <param name="timeoutSeconds">Maximum time to wait in seconds.</param>
        private IEnumerator WaitForLocation(float timeoutSeconds)
        {
            if (Input.location.status == LocationServiceStatus.Running) yield break;

            var elapsed = 0f;

            while (
                Input.location.status == LocationServiceStatus.Initializing && elapsed < timeoutSeconds
            )
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}