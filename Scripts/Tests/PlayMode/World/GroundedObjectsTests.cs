using System;
using System.Collections;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using World;
using Object = UnityEngine.Object;

namespace Tests.PlayMode.World
{
    /// <summary>
    ///     Unit tests for GroundedObject behaviours.
    /// </summary>
    /// <remarks>
    ///     Tests if object position is not null with no renderer
    ///     Tests if X and Y coordinates are unchanged with no renderer
    ///     Tests if grounding is correct with no renderer
    ///     Expects an error due to missing ArcGIS map (not required here)
    ///     Tests if object position is not null with a renderer
    ///     Tests if X and Y coordinates are unchanged with a renderer
    ///     Tests if grounding is correct with a renderer
    ///     Tests if null positioned object stays null positioned after grounding.
    /// </remarks>
    public class GroundedObjectsTests
    {
        /// <summary>
        ///     when there are no renderers on this object, or its children, the base offset is zero.
        ///     Altitude should be grounded to 0 no matter what the starting altitude is.
        /// </summary>
        [UnityTest]
        public IEnumerator NoRendererIsGroundedZero()
        {
            var gameObject = new GameObject("Grounded_NoRenderers");
            var locationComponent = gameObject.AddComponent<ArcGISLocationComponent>();
            var grounded = gameObject.AddComponent<GroundedObject>();

            // set position with a non-zero altitude to make sure it changes to 0 when snapped.
            locationComponent.Position =
                new ArcGISPoint(-1.185, 52.953, 25.0, new ArcGISSpatialReference(4326)); // WGS84
            grounded.SnapToGround();
            yield return null;

            // null check incase it decides to explode
            Assert.IsNotNull(locationComponent.Position, "Position should not be null");
            // check if everything is the same, but the altitude is grounded.
            Assert.AreEqual(R3(-1.185), R3(locationComponent.Position.X));
            Assert.AreEqual(R3(52.953), R3(locationComponent.Position.Y));
            Assert.AreEqual(R3(0.0), R3(locationComponent.Position.Z),
                "Altitude should be grounded to 0 when no renderers are present");
            Object.Destroy(gameObject);
            yield return null;
        }

        /// <summary>
        ///     Make sure child is grounded correct with its size factored in.
        /// </summary>
        /// <remarks>
        ///     "Unable to find a parent ArcGISMapComponent." is the expected output for this.
        ///     This only deals with the altitude so, ArcGIS gets confused but the tests are correct and pass.
        ///     This happens because I don't need to have the ArcGIS map on the GameObject to test this.
        /// </remarks>
        [UnityTest]
        public IEnumerator RendererBaseOffset()
        {
            LogAssert.Expect(LogType.Warning, "Unable to find a parent ArcGISMapComponent.");
            var parent = new GameObject("GroundedWithRenderer");
            parent.transform.position = new Vector3(0, 10, 0);

            var arcGisComp = parent.AddComponent<ArcGISLocationComponent>();
            var grounded = parent.AddComponent<GroundedObject>();

            // child cube with renderer and ensure it matches parent position.
            var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.name = "ChildCube";
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            // initial position altitude, arbitrary for the test.
            arcGisComp.Position = new ArcGISPoint(1.0, 2.0, 5.0, new ArcGISSpatialReference(4326));

            grounded.SnapToGround();
            yield return null;

            Assert.IsNotNull(arcGisComp.Position);
            Assert.AreEqual(R3(1.0), R3(arcGisComp.Position.X));
            Assert.AreEqual(R3(2.0), R3(arcGisComp.Position.Y));
            Assert.AreEqual(R3(0.5), R3(arcGisComp.Position.Z), "Altitude should be grounded with base offset of 0.5");

            Object.Destroy(child);
            Object.Destroy(parent);

            yield return null;
        }

        /// <summary>
        ///     If ArcGISLocationComponent pos is null, SnapToGround should finish cleanly.
        ///     It should do this without throwing an error and without giving it a new position, keeping it all null.
        /// </summary>
        [UnityTest]
        public IEnumerator NullPositionCleanFinish()
        {
            var gameObject = new GameObject("GroundedNullPosition");
            var arcGisLoc = gameObject.AddComponent<ArcGISLocationComponent>();
            var grounded = gameObject.AddComponent<GroundedObject>();

            arcGisLoc.Position = null;

            grounded.SnapToGround();
            yield return null;

            Assert.IsNull(arcGisLoc.Position, "Position should remain null if not set");
            Object.Destroy(gameObject);

            yield return null;
        }

        // self-explanatory helper method
        private static double R3(double v)
        {
            return Math.Round(v, 3);
        }
    }
}