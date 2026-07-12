using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;

namespace World
{
    /// <summary>
    ///     Ensures the GameObject (with an ArcGISLocation component) is grounded to the map by adjusting its altitude
    ///     based on renderer bounds and an optional offset.
    /// </summary>
    [RequireComponent(typeof(ArcGISLocationComponent))]
    public class GroundedObject : MonoBehaviour
    {
        // We probably want the brand objects to hover a bit
        [SerializeField]
        /// <summary>
        /// Additional altitude applied on top of the grounded position.
        /// </summary>
        private float additionalOffset;

        /*

    This could be useful to disable on some objects at some point.

    For now though, the recommended use is:
        1. Parent object which holds ArcGISLocation and GroundedObject components.
        2. Child object without an ArcGISLocation component (the thing you want to ground).

    For example, to ground an object spawning around the player (a brand):
        1. Spawn the object (type WorldObject) and position around the player's circle.
        2. Set the spawned object as a child to the player empty.
        3. The spawned object will be grounded dynamically on play.

        (A full implementation would probably want a seperate empty which tracks the player
        so that an altitude offset can be applied).

    */
        /// <summary>
        ///     If true, include all child renderers when calculating bounds for grounding.
        /// </summary>
        [SerializeField]
        private bool includeChildren = true;

        private readonly float groundAltitude = 0f;

        private ArcGISLocationComponent arcLocation;

        /// <summary>
        ///     Unity awake hook. Caches the ArcGISLocationComponent and snaps to ground.
        /// </summary>
        private void Awake()
        {
            arcLocation = GetComponent<ArcGISLocationComponent>();
            SnapToGround();
        }

        /// <summary>
        ///     Snap the object to the ground, accounting for its height.
        ///     We can call this elsewhere, e.g. if we ever change an object's size in runtime
        /// </summary>
        public void SnapToGround()
        {
            if (arcLocation == null || arcLocation.Position == null) return;

            var baseOffset = CalculateBaseOffset();

            var currentPos = arcLocation.Position;

            arcLocation.Position = new ArcGISPoint(
                currentPos.X,
                currentPos.Y,
                groundAltitude + baseOffset + additionalOffset,
                currentPos.SpatialReference
            );
        }

        /// <summary>
        ///     Calculates the vertical offset needed to place the object's lowest point on the ground.
        /// </summary>
        /// <returns>The offset from the current position to the bottom of the bounds.</returns>
        private float CalculateBaseOffset()
        {
            var bounds = GetCombinedBounds();

            if (bounds.size == Vector3.zero) return 0f;

            return transform.position.y - bounds.min.y;
        }

        /// <summary>
        ///     Computes the combined renderer bounds for this object (and optionally its children).
        /// </summary>
        /// <returns>The combined bounds in world space.</returns>
        private Bounds GetCombinedBounds()
        {
            var renderers = includeChildren
                ? GetComponentsInChildren<Renderer>()
                : GetComponents<Renderer>();

            if (renderers.Length == 0) return new Bounds(transform.position, Vector3.zero);

            var combined = renderers[0].bounds;

            for (var i = 1; i < renderers.Length; i++) combined.Encapsulate(renderers[i].bounds);

            return combined;
        }
    }
}