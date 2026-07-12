using UnityEngine;

namespace Mapping
{
    /// <summary>
    ///     Continuously rotates the GameObject around a target and keeps it facing the target.
    ///     Useful for object showcase effects.
    /// </summary>
    public class ObjectRotator : MonoBehaviour
    {
        /// <summary>
        ///     Target object to rotate around.
        /// </summary>
        public GameObject target;

        /// <summary>
        ///     Degrees per second for the rotation around the Y axis.
        /// </summary>
        public float x;

        /// <summary>
        ///     Unity start hook. Applies an initial tilt.
        /// </summary>
        private void Start()
        {
            transform.RotateAround(target.transform.position, Vector3.right, 40);
        }

        /// <summary>
        ///     Unity late update hook. Performs the rotation and keeps the object looking at the target.
        /// </summary>
        private void LateUpdate()
        {
            transform.RotateAround(target.transform.position, Vector3.up, x * Time.deltaTime);
            transform.LookAt(target.transform);
        }
    }
}