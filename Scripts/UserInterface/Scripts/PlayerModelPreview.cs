using UnityEngine;
using UnityEngine.Rendering;

namespace UserInterface.Scripts
{
    /// <summary>
    ///     Creates a RenderTexture of the player prefab to be drawn within the MapView UI (live preview of the player's character).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Large portions of this code are migrations of the RT renderer logic in the Creative Characters package, modified
    ///     to preview in play-mode instead of edit-mode.
    ///     </para>
    ///     <para>
    ///     Source: https://assetstore.unity.com/packages/3d/characters/humanoids/creative-characters-free-animated-low-poly-3d-models-304841
    ///     </para>
    ///     <para>
    ///     File: CharacterCustomizationWindow.cs
    ///     </para>
    /// </remarks>
    public class PlayerModelPreview : MonoBehaviour
    {
        private const string PreviewLayerName = "Player Preview";
        private const string ShaderPath = "Custom/PlayerPreview";

        private static readonly int CircleCentreId = Shader.PropertyToID("circle_centre");
        private static readonly int CircleRadiusId = Shader.PropertyToID("circle_radius");
        private static readonly int EdgeSoftnessId = Shader.PropertyToID("edge_softness");

        private int previewLayer;
        private Camera cam;
        private Transform cameraPivot;
        private Animator previewAnimator;
        private GameObject previewInstance;
        private Material maskMaterial;
        private RenderTexture rawRt;

        [SerializeField] private Shader maskShader;

        [Header("Camera")]

        [SerializeField]
        private Vector3 cameraLocalPosition = new Vector3(0, 1.6f, -36);
        [SerializeField]
        private float cameraFov = 2.5f;
        [SerializeField]
        private float pivotRotationY = 150f;

        [Header("Player Setup")]

        [SerializeField]
        private GameObject characterPrefab;

        [SerializeField]
        private RuntimeAnimatorController previewAnimatorController;

        [Header("Mask")]

        [SerializeField]
        private Vector2 circleCentre = new Vector2(0.5f, 0.333f);

        [SerializeField]
        private float circleRadius = 0.3f;

        [SerializeField]
        private float edgeSoftness = 0.005f;

        public RenderTexture PreviewTexture { get; private set; }

        private void Awake()
        {
            previewLayer = LayerMask.NameToLayer(PreviewLayerName);
            CreateRenderTexture();
            CreateMaskMaterial();
        }

        private void OnEnable()
        {
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void Start()
        {
            SpawnPreviewModel();
            InitialiseCamera();

            previewAnimator = previewInstance.GetComponentInChildren<Animator>();
            previewAnimator.runtimeAnimatorController = previewAnimatorController;
            previewAnimator.Play("Idle");
        }

        private void OnDestroy()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

            if (previewInstance != null) Destroy(previewInstance);
            if (cameraPivot != null) Destroy(cameraPivot.gameObject);

            if (rawRt != null)
            {
                rawRt.Release();
                Destroy(rawRt);
            }

            if (PreviewTexture == null) return;
            PreviewTexture.Release();
            Destroy(PreviewTexture);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || cam == null)
            {
                return;
            }

            cam.fieldOfView = cameraFov;
            cam.transform.localPosition = cameraLocalPosition;
            cameraPivot.rotation = Quaternion.identity;
            cameraPivot.Rotate(Vector3.up, pivotRotationY, Space.Self);

            UpdateMaskMaterial();
        }

        /// <summary>
        /// Sets blit values to apply the preview mask.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="aCam"></param>
        private void OnEndCameraRendering(ScriptableRenderContext ctx, Camera aCam)
        {
            if (aCam != cam || maskMaterial == null)
            {
                return;
            }

            var cmd = new CommandBuffer
            {
                name = "PlayerPreviewBlit"
            };

            cmd.SetGlobalTexture("_BlitTexture", rawRt);
            cmd.SetGlobalVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));

            cmd.SetRenderTarget(PreviewTexture);
            cmd.DrawProcedural(Matrix4x4.identity, maskMaterial, 0, MeshTopology.Triangles, 3);
            ctx.ExecuteCommandBuffer(cmd);
            ctx.Submit();
            cmd.Release();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateRenderTexture()
        {
            if (PreviewTexture)
            {
                return;
            }

            rawRt = new RenderTexture(300, 300, 30, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1
            };

            PreviewTexture = new RenderTexture(300, 300, 30, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1
            };
        }

        private void CreateMaskMaterial()
        {
            maskMaterial = new Material(maskShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            UpdateMaskMaterial();
        }

        private void UpdateMaskMaterial()
        {
            maskMaterial.SetVector(CircleCentreId, circleCentre);
            maskMaterial.SetFloat(CircleRadiusId, circleRadius);
            maskMaterial.SetFloat(EdgeSoftnessId, edgeSoftness);
        }

        private void SpawnPreviewModel()
        {
            previewInstance = Instantiate(characterPrefab, Vector3.one * -500f, Quaternion.identity);
            previewInstance.hideFlags = HideFlags.HideAndDontSave;

            SetLayerRecursively(previewInstance, previewLayer);

            foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void InitialiseCamera()
        {
            if (cam) return;

            cameraPivot = new GameObject("CameraPivot").transform;
            cameraPivot.gameObject.hideFlags = HideFlags.HideAndDontSave;
            cameraPivot.position = previewInstance.transform.position;

            var cameraObject = new GameObject("PreviewCamera")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            cam = cameraObject.AddComponent<Camera>();
            cam.targetTexture = rawRt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.clear;
            cam.enabled = true;
            cam.useOcclusionCulling = false;
            cam.fieldOfView = cameraFov;
            cam.transform.SetParent(cameraPivot);
            cam.cullingMask = 1 << previewLayer;
            cam.depth = -100;

            cameraPivot.Rotate(Vector3.up, pivotRotationY, Space.Self);
            cam.transform.localPosition = cameraLocalPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="layer"></param>
        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}