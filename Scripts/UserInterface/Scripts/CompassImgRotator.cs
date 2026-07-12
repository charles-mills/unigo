using Mapping;
using UnityEngine;
using UnityEngine.UIElements;

namespace Display
{
    /**
     * Rotates a UI image (the compass) in line with the compass heading of the user's device.
     */
    public class CompassImgRotator : MonoBehaviour
    {
        [SerializeField]
        private float smoothSpeed = 2f;

        private VisualElement _compass;
        private PlayerSpatialPositioner _playerSpatialPositioner;
        private float _smoothedHeading;

        private void Update()
        {
            var heading = _playerSpatialPositioner.GetCompassHeading();
            if (heading < 0f) return;

            _smoothedHeading = Mathf.LerpAngle(_smoothedHeading, heading, Time.deltaTime * smoothSpeed);
            _compass.style.rotate = new Rotate(Angle.Degrees(-_smoothedHeading));
        }

        private void OnEnable()
        {
            _playerSpatialPositioner = FindAnyObjectByType<PlayerSpatialPositioner>();
            var uiDocument = GetComponent<UIDocument>();
            _compass = uiDocument.rootVisualElement.Q<VisualElement>("Compass");

            _compass.RegisterCallback<ClickEvent>(OnCompassClicked);
        }

        private void OnCompassClicked(ClickEvent evt)
        {
            if (_playerSpatialPositioner != null) _playerSpatialPositioner.ToggleCameraRotation();
        }
    }
}