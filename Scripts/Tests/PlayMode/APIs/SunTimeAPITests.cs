using System;
using System.Collections;
using APIs_Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.APIs
{
    /// <summary>
    ///     Unit tests for SunTimeAPI behaviours.
    /// </summary>
    /// <remarks>
    ///     Tests if invalid coordinates trigger error.
    ///     Tests if no Wi-Fi connection triggers error.
    ///     Tests if valid requests return a valid invocation.
    ///     Tests if valid requests return an actual boolean (regardless of time)
    /// </remarks>
    public class SunTimeAPITests
    {
        /// <summary>
        ///     For invalid coords or when there is no Wi-Fi the API invokes the callback (r => result = r) with false.
        /// </summary>
        [UnityTest]
        public IEnumerator InvalidCoordsOrNoWiFi()
        {
            bool? result = null;
            // Invalid coordinates (outside valid range) to trigger error.
            // If there is no internet, this should also just return false.
            yield return SunTimeAPI.GetIsNight(9999, -9999, DateTime.UtcNow, r => result = r);

            Assert.IsNotNull(result, "Callback should be invoked");
            Assert.IsFalse(result.Value, "Invalid coords or failed request should return false");
        }

        /// <summary>
        ///     With internet available, valid coordinates request should complete within a reasonable time.
        ///     Only assert completion because the value would depend on the time.
        ///     When offline, the test is skipped as that is already tested for.
        /// </summary>
        [UnityTest]
        public IEnumerator GetIsNight_ValidCoords_Completes_WhenOnline()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                Assert.Ignore("No internet connectivity; skipping.");

            var invoked = false;
            bool? value = null;

            var lon = -1.185; // X
            var lat = 52.953; // Y

            yield return SunTimeAPI.GetIsNight(lat, lon, DateTime.UtcNow, r =>
            {
                invoked = true;
                value = r;
            });

            Assert.IsTrue(invoked, "Callback should be invoked when online");
            // value is a boolean, and we don't care what it is, only that it is a bool and callback was actually fired.
            Assert.IsInstanceOf<bool>(value, "Value is a valid boolean.");
        }
    }
}