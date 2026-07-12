using System.Collections;
using System.Collections.Generic;
using APIs_Helpers;
using Inventory;
using Solo.MOST_IN_ONE;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using World;

namespace Gameplay
{
    /// <summary>
    ///     Implements a swipe-to-throw mechanic that applies forces to a rigidbody
    ///     based on the user's swipe direction and speed.
    /// </summary>
    public class Throw : MonoBehaviour
    {
        [Header("References")]
        public Camera arCamera;

        public ARSpawnBox catchAnimator;
        public GameObject boxImage;
        public GameObject pointAtGroundText;
        public GameObject swipeBoxText;
        public GameObject codePanel;

        [Header("AR Placement")]
        public ARRaycastManager raycastManager;

        public ARPlaneManager planeManager;
        public float placementHeightOffset = 1f;

        [Header("Aim Assist")]
        [Range(10f, 90f)]
        public float catchAngleThreshold = 30f;

        public float minSwipeLength = 50f;
        public float modelScalar = 0.4f;

        public Vector2 startPos;
        public Vector2 endPos;

        public float swipeBoxTextVisibleDuration = 1.5f;

        private readonly List<ARRaycastHit> _arHits = new();

        private BrandType _capturedBrand = BrandType.None;
        private bool _catchResolved;
        private bool _isSwiping;

        private GameObject _spawnedToken;
        private bool _tokenPlaced;

        private void Start()
        {
            if (pointAtGroundText != null) pointAtGroundText.SetActive(true);
        }

        /// <summary>
        ///     Checks for swipe input and applies forces accordingly.
        /// </summary>
        private void Update()
        {
            if (!_tokenPlaced)
            {
                TryPlaceToken();
                return;
            }

            if (_catchResolved || _spawnedToken == null) return;

            // get swipe start and end positions
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Touch Began");
                startPos = Input.mousePosition;
                _isSwiping = true;
            }

            if (!Input.GetMouseButtonUp(0) || !_isSwiping) return;

            Debug.Log("Touch Ended");
            _isSwiping = false;
            endPos = Input.mousePosition;

            if (boxImage != null && swipeBoxText != null)
            {
                boxImage.SetActive(false);
                swipeBoxText.SetActive(false);
            }

            AudioManager.Instance?.PlaySfx(AudioManager.Instance.swipeThrow);
            CheckSwipe(startPos, endPos);
        }

        private bool SpawnToken(Vector3 position)
        {
            var registry = FindAnyObjectByType<TokenRegistry>();

            if (registry == null) return false;

            GameObject prefab = registry.GetPrefab(TokenIdentity.SelectedBrand);

            if (prefab == null) return false;

            _spawnedToken = Instantiate(prefab, position, Quaternion.identity);
            _capturedBrand = TokenIdentity.SelectedBrand;
            _spawnedToken.transform.localScale = Vector3.one * modelScalar;

            var destroyer = _spawnedToken.GetComponent<DestroyAfterTime>();

            if (destroyer != null) Destroy(destroyer);

            return true;
        }

        private void TryPlaceToken()
        {
            if (!TryGetPlacementPose(out var hitPose)) return;

            var spawnPos = hitPose.position + Vector3.up * placementHeightOffset;

            if (!SpawnToken(spawnPos)) return;

            _tokenPlaced = true;

            if (boxImage != null) boxImage.SetActive(true);

            if (pointAtGroundText != null) pointAtGroundText.SetActive(false);

            if (swipeBoxText != null) StartCoroutine(ShowSwipeText());
        }

        private bool TryGetPlacementPose(out Pose placementPose)
        {
            placementPose = default;

            if (raycastManager == null) return false;

            var screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            if (!raycastManager.Raycast(screenCentre, _arHits, TrackableType.PlaneWithinPolygon)) return false;

            var hit = _arHits[0];

            if (planeManager != null)
            {
                var plane = planeManager.GetPlane(hit.trackableId);

                if (plane == null || plane.alignment != PlaneAlignment.HorizontalUp ||
                    plane.trackingState != TrackingState.Tracking)
                    return false;
            }

            placementPose = hit.pose;
            return true;
        }

        private void CheckSwipe(Vector2 start, Vector2 end)
        {
            var swipeDir = end - start;

            if (swipeDir.magnitude < minSwipeLength) return;

            var tokenScreenPos = arCamera.WorldToScreenPoint(_spawnedToken.transform.position);
            
            if (tokenScreenPos.z < 0)
            {
                ResolveSwipe(false, end);
                return;
            }
            
            var toToken = (Vector2)tokenScreenPos - start;
            var angle = Vector2.Angle(swipeDir.normalized, toToken.normalized);
            
            ResolveSwipe(angle <= catchAngleThreshold, end);
        }

        private void ResolveSwipe(bool isSuccess, Vector2 swipeEnd)
        {
            _catchResolved = true;

            var sfx = AudioManager.Instance;
            sfx.PlaySfx(isSuccess ? sfx.catchSuccess : sfx.catchFail);

            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.HeavyImpact);
            var swipeWorldTarget = GetSwipeWorldTarget(swipeEnd);
            
            if (isSuccess)
            {
                FinaliseSuccessfulCatch();
                catchAnimator.PlayCatchAnimation(_spawnedToken, swipeWorldTarget);
            }
            else
            {
                SavePlayerIncrement(false);
                catchAnimator.PlayFailAnimation(_spawnedToken, swipeWorldTarget);
            }
        }
        
        private void FinaliseSuccessfulCatch()
        {
            var inventoryManager = ResolveInventoryManager();
            var brandName = GetCapturedBrandName(_capturedBrand);
            var code = GetCollectedCode();

            if (inventoryManager != null && !string.IsNullOrWhiteSpace(code)) inventoryManager.AddItem(brandName, code);

            SavePlayerIncrement(true);
        }

        private Vector3 GetSwipeWorldTarget(Vector2 swipeEnd)
        {
            var tokenScreenPos = arCamera.WorldToScreenPoint(_spawnedToken.transform.position);
            var depth = tokenScreenPos.z > 0 ? tokenScreenPos.z : 3f;

            return arCamera.ScreenToWorldPoint(new Vector3(swipeEnd.x, swipeEnd.y, depth));
        }

        private IEnumerator ShowSwipeText()
        {
            swipeBoxText.SetActive(true);
            yield return new WaitForSeconds(swipeBoxTextVisibleDuration);
            swipeBoxText.SetActive(false);
        }

        private static void SavePlayerIncrement(bool isSuccess)
        {
            var data = PlayerDataSaveManager.LoadPlayerData();

            if (isSuccess)
                data.IncrementCatchSuccess();
            else
                data.IncrementCatchFail();

            PlayerDataSaveManager.SavePlayerData(data);
        }

        private static InventoryManager ResolveInventoryManager()
        {
            if (InventoryManager.Instance != null) return InventoryManager.Instance;

            var inventoryManager = FindAnyObjectByType<InventoryManager>();
            if (inventoryManager != null) return inventoryManager;
            
            var managerObject = new GameObject("InventoryManager");
            return managerObject.AddComponent<InventoryManager>();
        }

        private static string GetCapturedBrandName(BrandType brand)
        {
            return brand switch
            {
                BrandType.Nike => "Bike",
                BrandType.Adidas => "Abibas",
                BrandType.Microsoft => "Smallsoft",
                BrandType.Dominos => "Bominos",
                _ => "Unknown"
            };
        }

        private string GetCollectedCode()
        {
            var panel = codePanel;

            if (panel == null && catchAnimator != null) panel = catchAnimator.codePanel;

            if (panel != null)
            {
                var setCode = panel.GetComponentInChildren<SetCode>(true);
                if (setCode != null) return setCode.GetOrGenerateCode();

                var text = panel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null) return text.text;
            }

            var fallbackCodeSource = FindAnyObjectByType<SetCode>();
            return fallbackCodeSource != null ? fallbackCodeSource.GetOrGenerateCode() : string.Empty;
        }
    }
}