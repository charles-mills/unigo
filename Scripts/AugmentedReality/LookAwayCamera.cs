using UnityEngine;

namespace World
{
    public class LookAwayCamera : MonoBehaviour
    {
        private Camera arCamera;

        private void Start()
        {
            arCamera = Camera.main;
        }

        private void Update()
        {
            if (arCamera == null)
                return;


            transform.LookAt(arCamera.transform);

            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);
        }
    }
}