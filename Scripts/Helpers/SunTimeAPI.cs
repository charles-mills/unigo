using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace APIs_Helpers
{
    /// <summary>
    ///     Wrapper for the sunrise-sunset.org API to determine whether it is currently night
    ///     at a given latitude/longitude. Provides a method for use in Unity.
    /// </summary>
    public static class SunTimeAPI
    {
        /// <summary>
        ///     Queries the sunrise-sunset API and gets a callback if the provided UTC time
        ///     falls between sunset and sunrise for the given coordinates.
        /// </summary>
        /// <param name="lat">Latitude in decimal degrees.</param>
        /// <param name="lon">Longitude in decimal degrees.</param>
        /// <param name="utcNow">The current time in UTC.</param>
        /// <param name="onComplete">Callback invoked with true if night, false otherwise.</param>
        /// <returns>Coroutine enumerator for use with StartCoroutine.</returns>
        public static IEnumerator GetIsNight(double lat, double lon, DateTime utcNow, Action<bool> onComplete)
        {
            var url = $"https://api.sunrise-sunset.org/json?lat={lat}&lng={lon}&formatted=0";
            using var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Sun API error");
                onComplete?.Invoke(false);
                yield break;
            }

            // nothing should be null, have "invalid" or have "1970" unix time in its request.
            var payload = JsonUtility.FromJson<SunApiResponse>(request.downloadHandler.text);
            if (payload?.results == null || request.downloadHandler.text.ToLower().Contains("invalid") ||
                request.downloadHandler.text.ToLower().Contains("1970"))
            {
                Debug.LogWarning("Sun API payload invalid");
                onComplete?.Invoke(false);
                yield break;
            }

            var sunrise = DateTime.Parse(payload.results.sunrise, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal);
            var sunset = DateTime.Parse(payload.results.sunset, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal);

            var isNight = utcNow >= sunset || utcNow <= sunrise;
            Debug.Log($"SunTimeAPI called, IsNight: {isNight}");
            onComplete?.Invoke(isNight);
        }

        [Serializable]
        private class SunApiResponse
        {
            public SunApiResult results;
            public string status;
        }

        [Serializable]
        private class SunApiResult
        {
            public string sunrise;
            public string sunset;
        }
    }
}