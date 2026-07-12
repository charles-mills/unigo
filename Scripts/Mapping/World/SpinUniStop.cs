using UnityEngine;

namespace Mapping.World
{
    /// <summary>
    /// Rotates a GameObject. Intended to be used with the diamond above UniStops.
    /// </summary>
    public class SpinUniStop : MonoBehaviour
    {
        [SerializeField]
        private GameObject cube;

        [SerializeField]
        private Vector3 rotationSpeed = new(15f, 5f, 5f);

        private void Update()
        {
            cube.transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}