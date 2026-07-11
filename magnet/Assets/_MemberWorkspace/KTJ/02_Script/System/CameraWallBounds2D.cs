using UnityEngine;

namespace KTJ.System
{
    [DisallowMultipleComponent]
    [AddComponentMenu("KTJ/System/Camera Wall Bounds 2D")]
    public sealed class CameraWallBounds2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private BoxCollider2D bottomCollider;
        [SerializeField] private BoxCollider2D leftCollider;
        [SerializeField] private BoxCollider2D rightCollider;

        [Header("Child Names")]
        [SerializeField] private string bottomName = "Bottom";
        [SerializeField] private string leftName = "LeftW";
        [SerializeField] private string rightName = "RightW";

        [Header("Bounds")]
        [Min(0.01f)]
        [SerializeField] private float wallThickness = 0.5f;
        [Min(0f)]
        [SerializeField] private float extraLength = 0.5f;
        [SerializeField] private bool placeInsideCameraView = true;

        private void Reset()
        {
            targetCamera = Camera.main;
            ResolveColliders();
        }

        private void Start()
        {
            FitToCamera();
        }

        [ContextMenu("Fit To Camera")]
        public void FitToCamera()
        {
            Camera cameraToUse = ResolveCamera();
            if (cameraToUse == null)
            {
                Debug.LogWarning($"{nameof(CameraWallBounds2D)}: Camera를 찾을 수 없습니다.", this);
                return;
            }

            if (!cameraToUse.orthographic)
            {
                Debug.LogWarning($"{nameof(CameraWallBounds2D)}: Orthographic 카메라 기준으로만 벽을 계산합니다.", this);
                return;
            }

            ResolveColliders();
            if (bottomCollider == null || leftCollider == null || rightCollider == null)
            {
                Debug.LogWarning($"{nameof(CameraWallBounds2D)}: Bottom, LeftW, RightW 중 BoxCollider2D가 없는 오브젝트가 있습니다.", this);
                return;
            }

            float height = cameraToUse.orthographicSize * 2f;
            float width = height * cameraToUse.aspect;
            Vector3 center = cameraToUse.transform.position;

            float left = center.x - width * 0.5f;
            float right = center.x + width * 0.5f;
            float bottom = center.y - height * 0.5f;
            float verticalCenter = center.y;

            float halfThickness = wallThickness * 0.5f;
            float horizontalInset = placeInsideCameraView ? halfThickness : -halfThickness;
            float verticalInset = placeInsideCameraView ? halfThickness : -halfThickness;

            SetCollider(
                bottomCollider,
                new Vector2(center.x, bottom + verticalInset),
                new Vector2(width + extraLength * 2f, wallThickness));

            SetCollider(
                leftCollider,
                new Vector2(left + horizontalInset, verticalCenter),
                new Vector2(wallThickness, height + extraLength * 2f));

            SetCollider(
                rightCollider,
                new Vector2(right - horizontalInset, verticalCenter),
                new Vector2(wallThickness, height + extraLength * 2f));
        }

        private Camera ResolveCamera()
        {
            if (targetCamera != null) return targetCamera;

            targetCamera = Camera.main;
            return targetCamera;
        }

        private void ResolveColliders()
        {
            bottomCollider ??= FindChildCollider(bottomName);
            leftCollider ??= FindChildCollider(leftName);
            rightCollider ??= FindChildCollider(rightName);
        }

        private BoxCollider2D FindChildCollider(string childName)
        {
            if (string.IsNullOrWhiteSpace(childName)) return null;

            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<BoxCollider2D>() : null;
        }

        private void SetCollider(BoxCollider2D boxCollider, Vector2 worldPosition, Vector2 worldSize)
        {
            Transform colliderTransform = boxCollider.transform;
            Vector3 currentPosition = colliderTransform.position;
            colliderTransform.position = new Vector3(worldPosition.x, worldPosition.y, currentPosition.z);

            Vector3 scale = colliderTransform.lossyScale;
            float scaleX = Mathf.Approximately(scale.x, 0f) ? 1f : Mathf.Abs(scale.x);
            float scaleY = Mathf.Approximately(scale.y, 0f) ? 1f : Mathf.Abs(scale.y);

            boxCollider.offset = Vector2.zero;
            boxCollider.size = new Vector2(worldSize.x / scaleX, worldSize.y / scaleY);
        }
    }
}



