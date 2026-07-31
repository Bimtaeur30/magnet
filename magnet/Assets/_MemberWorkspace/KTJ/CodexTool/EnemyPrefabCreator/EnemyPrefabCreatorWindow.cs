using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Magent.KTJ.Editor
{
    public sealed class EnemyPrefabCreatorWindow : EditorWindow
    {
        private const string DefaultOutputFolder = "Assets/_MemberWorkspace/KTJ/06_Prefab/Enemy";
        private const string DefaultTemplatePath = "Assets/_MemberWorkspace/KTJ/06_Prefab/Enemy/Enemy_01.prefab";
        private const float PreviewHeight = 420f;

        [Serializable]
        private sealed class Part
        {
            public string label;
            public string objectName;
            public Sprite sprite;
            public Vector2 position;
            public float angle;
            public Vector2 scale = Vector2.one;
            public int sortingOrder;
            public Matrix4x4 parentMatrix = Matrix4x4.identity;

            public Part(string label, string objectName, int sortingOrder)
            {
                this.label = label;
                this.objectName = objectName;
                this.sortingOrder = sortingOrder;
            }
        }

        private readonly Part[] parts =
        {
            new Part("몸통", "Body", 0),
            new Part("왼팔", "LHand", 1),
            new Part("오른팔", "RHand", 1)
        };

        private string prefabName = "Enemy_New";
        private string outputFolder = DefaultOutputFolder;
        private GameObject templatePrefab;
        private Vector2 scroll;
        private int selectedPart;
        private bool draggingPosition;
        private bool draggingRotation;
        private Vector2 dragStartMouse;
        private Vector2 dragStartPosition;
        private float dragStartAngle;
        private float previewScale = 100f;
        private Vector2 previewPan;

        [MenuItem("Tools/KTJ/에너미 프리팹 제작기")]
        private static void Open()
        {
            var window = GetWindow<EnemyPrefabCreatorWindow>();
            window.titleContent = new GUIContent("에너미 제작기");
            window.minSize = new Vector2(620f, 760f);
            window.Show();
        }

        private void OnEnable()
        {
            if (templatePrefab == null)
                templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultTemplatePath);
            LoadTemplateDefaults();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawHeader();
            DrawIdentity();
            DrawPartSettings();
            DrawPreview();
            DrawCreateButton();
            EditorGUILayout.EndScrollView();

            if (draggingPosition || draggingRotation)
                Repaint();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("에너미 프리팹 제작기", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "몸통/왼팔/오른팔 스프라이트를 넣고 위치와 각도를 조절하세요. " +
                "미리보기에서 파츠를 클릭한 뒤 중앙 기즈모는 이동, 원형 핸들은 회전입니다.",
                MessageType.Info);
        }

        private void DrawIdentity()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("프리팹 설정", EditorStyles.boldLabel);
            prefabName = EditorGUILayout.TextField("이름", prefabName);
            EditorGUI.BeginChangeCheck();
            templatePrefab = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("기준 프리팹", "컴포넌트와 전체 구조를 복제할 원본입니다."),
                templatePrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                LoadTemplateDefaults();
                Repaint();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("저장 폴더");
                EditorGUILayout.SelectableLabel(outputFolder, EditorStyles.textField, GUILayout.Height(19));
                if (GUILayout.Button("선택", GUILayout.Width(54)))
                    SelectOutputFolder();
            }
        }

        private void DrawPartSettings()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("파츠 설정", EditorStyles.boldLabel);

            for (int i = 0; i < parts.Length; i++)
            {
                Part part = parts[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool selected = selectedPart == i;
                        if (GUILayout.Toggle(selected, part.label, EditorStyles.toolbarButton, GUILayout.Width(70)) && !selected)
                            selectedPart = i;

                        EditorGUI.BeginChangeCheck();
                        part.sprite = (Sprite)EditorGUILayout.ObjectField(part.sprite, typeof(Sprite), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            selectedPart = i;
                            Repaint();
                        }
                    }

                    part.position = EditorGUILayout.Vector2Field("Visual 위치", part.position);
                    part.angle = EditorGUILayout.FloatField("Visual 각도", part.angle);
                    part.scale = EditorGUILayout.Vector2Field("Visual 크기", part.scale);
                    part.sortingOrder = EditorGUILayout.IntField("정렬 순서", part.sortingOrder);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("선택 파츠 초기화"))
                {
                    parts[selectedPart].position = Vector2.zero;
                    parts[selectedPart].angle = 0f;
                    parts[selectedPart].scale = Vector2.one;
                }
                if (GUILayout.Button("템플릿 값 다시 불러오기"))
                {
                    LoadTemplateDefaults();
                }
            }
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("미리보기", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"확대 {previewScale:0} px/unit", EditorStyles.miniLabel);
                if (GUILayout.Button("화면 맞춤", EditorStyles.miniButton, GUILayout.Width(70)))
                    FrameAll();
            }

            Rect rect = GUILayoutUtility.GetRect(100f, PreviewHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.105f, 0.115f, 0.13f));
            DrawGrid(rect);

            for (int i = 0; i < parts.Length; i++)
                DrawSprite(rect, parts[i], i == selectedPart);

            DrawGizmo(rect, parts[selectedPart]);
            HandlePreviewInput(rect);

            GUI.Label(new Rect(rect.x + 8, rect.y + 7, rect.width - 16, 20),
                "클릭: 파츠 선택  |  중앙 사각형 드래그: 이동  |  원형 핸들 드래그: 회전  |  휠: 확대",
                EditorStyles.miniLabel);
        }

        private void DrawGrid(Rect rect)
        {
            Vector2 origin = WorldToPreview(rect, Vector2.zero);
            Color gridColor = new Color(1f, 1f, 1f, 0.07f);
            Color axisColor = new Color(1f, 1f, 1f, 0.22f);
            Handles.BeginGUI();
            Handles.color = gridColor;
            float spacing = Mathf.Max(20f, previewScale);
            for (float x = origin.x % spacing; x < rect.xMax; x += spacing)
                Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.yMax));
            for (float y = origin.y % spacing; y < rect.yMax; y += spacing)
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));
            Handles.color = axisColor;
            Handles.DrawLine(new Vector3(rect.x, origin.y), new Vector3(rect.xMax, origin.y));
            Handles.DrawLine(new Vector3(origin.x, rect.y), new Vector3(origin.x, rect.yMax));
            Handles.EndGUI();
        }

        private void DrawSprite(Rect rect, Part part, bool selected)
        {
            if (part.sprite == null)
                return;

            Rect spriteRect = part.sprite.rect;
            float pixelsPerUnit = Mathf.Max(1f, part.sprite.pixelsPerUnit);
            GetComposedTransform(part, out Vector2 worldPosition, out float worldAngle, out Vector2 worldScale);
            Vector2 size = Vector2.Scale(
                spriteRect.size / pixelsPerUnit * previewScale,
                new Vector2(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.y)));
            Vector2 center = WorldToPreview(rect, worldPosition);
            Rect drawRect = new Rect(center - size * 0.5f, size);
            Rect uv = new Rect(
                spriteRect.x / part.sprite.texture.width,
                spriteRect.y / part.sprite.texture.height,
                spriteRect.width / part.sprite.texture.width,
                spriteRect.height / part.sprite.texture.height);

            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(-worldAngle, center);
            GUI.DrawTextureWithTexCoords(drawRect, part.sprite.texture, uv, true);
            if (selected)
            {
                Handles.BeginGUI();
                Handles.color = new Color(0.25f, 0.75f, 1f, 0.85f);
                Handles.DrawAAPolyLine(2f,
                    new Vector3(drawRect.xMin, drawRect.yMin), new Vector3(drawRect.xMax, drawRect.yMin),
                    new Vector3(drawRect.xMax, drawRect.yMax), new Vector3(drawRect.xMin, drawRect.yMax),
                    new Vector3(drawRect.xMin, drawRect.yMin));
                Handles.EndGUI();
            }
            GUI.matrix = oldMatrix;
        }

        private void DrawGizmo(Rect rect, Part part)
        {
            GetComposedTransform(part, out Vector2 worldPosition, out float worldAngle, out _);
            Vector2 center = WorldToPreview(rect, worldPosition);
            const float radius = 43f;
            Vector2 rotationPoint = center + RotateVector(Vector2.up * radius, -worldAngle);

            Handles.BeginGUI();
            Handles.color = new Color(0.2f, 0.85f, 1f, 0.95f);
            Handles.DrawWireDisc(center, Vector3.forward, radius);
            Handles.DrawLine(center, rotationPoint);
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(rotationPoint, Vector3.forward, 6f);
            Handles.color = new Color(0.2f, 0.85f, 1f, 1f);
            Handles.DrawSolidRectangleWithOutline(
                new Rect(center.x - 6f, center.y - 6f, 12f, 12f),
                new Color(0.2f, 0.85f, 1f, 0.8f), Color.white);
            Handles.color = Color.red;
            Handles.DrawLine(center, center + Vector2.right * 32f);
            Handles.color = Color.green;
            Handles.DrawLine(center, center + Vector2.up * 32f);
            Handles.EndGUI();
        }

        private void HandlePreviewInput(Rect rect)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition) && !draggingPosition && !draggingRotation)
                return;

            Part part = parts[selectedPart];
            GetComposedTransform(part, out Vector2 worldPosition, out float worldAngle, out _);
            Vector2 center = WorldToPreview(rect, worldPosition);
            Vector2 rotationPoint = center + RotateVector(Vector2.up * 43f, -worldAngle);

            if (evt.type == EventType.ScrollWheel && rect.Contains(evt.mousePosition))
            {
                previewScale = Mathf.Clamp(previewScale * (1f - evt.delta.y * 0.08f), 20f, 500f);
                evt.Use();
                Repaint();
                return;
            }

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (Vector2.Distance(evt.mousePosition, rotationPoint) <= 12f)
                {
                    draggingRotation = true;
                    dragStartAngle = part.angle;
                    dragStartMouse = evt.mousePosition;
                    evt.Use();
                    return;
                }

                if (Vector2.Distance(evt.mousePosition, center) <= 14f)
                {
                    draggingPosition = true;
                    dragStartPosition = part.position;
                    dragStartMouse = evt.mousePosition;
                    evt.Use();
                    return;
                }

                int hit = HitTestPart(rect, evt.mousePosition);
                if (hit >= 0)
                {
                    selectedPart = hit;
                    evt.Use();
                    Repaint();
                }
            }

            if (evt.type == EventType.MouseDrag && evt.button == 0)
            {
                if (draggingPosition)
                {
                    Vector2 delta = evt.mousePosition - dragStartMouse;
                    Vector3 worldDelta = new Vector3(delta.x, -delta.y, 0f) / previewScale;
                    Vector3 localDelta = part.parentMatrix.inverse.MultiplyVector(worldDelta);
                    part.position = dragStartPosition + new Vector2(localDelta.x, localDelta.y);
                    evt.Use();
                }
                else if (draggingRotation)
                {
                    Vector2 start = dragStartMouse - center;
                    Vector2 current = evt.mousePosition - center;
                    part.angle = dragStartAngle - Vector2.SignedAngle(start, current);
                    evt.Use();
                }
            }

            if (evt.type == EventType.MouseUp && evt.button == 0)
            {
                draggingPosition = false;
                draggingRotation = false;
            }
        }

        private int HitTestPart(Rect rect, Vector2 mouse)
        {
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                Part part = parts[i];
                if (part.sprite == null)
                    continue;
                GetComposedTransform(part, out Vector2 worldPosition, out float worldAngle, out Vector2 worldScale);
                Vector2 size = Vector2.Scale(
                    part.sprite.rect.size / Mathf.Max(1f, part.sprite.pixelsPerUnit) * previewScale,
                    new Vector2(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.y)));
                Vector2 local = RotateVector(mouse - WorldToPreview(rect, worldPosition), worldAngle);
                if (Mathf.Abs(local.x) <= size.x * 0.5f && Mathf.Abs(local.y) <= size.y * 0.5f)
                    return i;
            }
            return -1;
        }

        private void FrameAll()
        {
            bool hasSprite = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Part part in parts)
            {
                if (part.sprite == null)
                    continue;
                GetComposedTransform(part, out Vector2 worldPosition, out _, out Vector2 worldScale);
                Vector2 size = Vector2.Scale(
                    (Vector2)part.sprite.bounds.size,
                    new Vector2(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.y)));
                Bounds partBounds = new Bounds(worldPosition, size);
                if (!hasSprite)
                {
                    bounds = partBounds;
                    hasSprite = true;
                }
                else
                {
                    bounds.Encapsulate(partBounds);
                }
            }

            if (!hasSprite)
            {
                previewScale = 100f;
                previewPan = Vector2.zero;
                return;
            }

            previewScale = Mathf.Clamp(Mathf.Min(500f / Mathf.Max(0.1f, bounds.size.x),
                330f / Mathf.Max(0.1f, bounds.size.y)), 20f, 500f);
            previewPan = -(Vector2)bounds.center;
            Repaint();
        }

        private void DrawCreateButton()
        {
            EditorGUILayout.Space(12);
            bool valid = IsValid(out string reason);
            if (!valid)
                EditorGUILayout.HelpBox(reason, MessageType.Warning);

            using (new EditorGUI.DisabledScope(!valid))
            {
                GUIStyle style = new GUIStyle(GUI.skin.button)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 14,
                    fixedHeight = 42
                };
                if (GUILayout.Button("에너미 프리팹 제작", style))
                    CreatePrefab();
            }
            EditorGUILayout.Space(12);
        }

        private bool IsValid(out string reason)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                reason = "프리팹 이름을 입력하세요.";
                return false;
            }
            if (prefabName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                reason = "프리팹 이름에 파일명으로 사용할 수 없는 문자가 있습니다.";
                return false;
            }
            foreach (Part part in parts)
            {
                if (part.sprite == null)
                {
                    reason = $"{part.label} 스프라이트를 지정하세요.";
                    return false;
                }
            }
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                reason = "유효한 프로젝트 내부 저장 폴더를 선택하세요.";
                return false;
            }
            if (templatePrefab == null || PrefabUtility.GetPrefabAssetType(templatePrefab) == PrefabAssetType.NotAPrefab)
            {
                reason = "복제할 기준 프리팹을 지정하세요.";
                return false;
            }
            if (!HasRequiredVisualHierarchy(templatePrefab.transform))
            {
                reason = "기준 프리팹에 Visual/Body·LHand·RHand/Visual 구조와 SpriteRenderer가 없습니다.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private void SelectOutputFolder()
        {
            string absolute = EditorUtility.OpenFolderPanel("프리팹 저장 폴더", Application.dataPath, string.Empty);
            if (string.IsNullOrEmpty(absolute))
                return;

            absolute = absolute.Replace('\\', '/');
            string assets = Application.dataPath.Replace('\\', '/');
            if (!absolute.StartsWith(assets, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("저장 폴더 오류", "Assets 폴더 내부를 선택해야 합니다.", "확인");
                return;
            }
            outputFolder = "Assets" + absolute.Substring(assets.Length);
        }

        private void CreatePrefab()
        {
            string safeName = prefabName.Trim();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{safeName}.prefab");
            string templatePath = AssetDatabase.GetAssetPath(templatePrefab);
            GameObject root = null;
            bool copied = false;
            try
            {
                copied = AssetDatabase.CopyAsset(templatePath, path);
                if (!copied)
                    throw new InvalidOperationException($"기준 프리팹을 복제하지 못했습니다: {templatePath}");

                root = PrefabUtility.LoadPrefabContents(path);
                root.name = safeName;
                foreach (Part part in parts)
                {
                    Transform visual = FindPartVisual(root.transform, part.objectName);
                    if (visual == null)
                        throw new InvalidOperationException($"{part.objectName}/Visual을 찾지 못했습니다.");

                    visual.localPosition = new Vector3(part.position.x, part.position.y, visual.localPosition.z);
                    visual.localRotation = Quaternion.Euler(0f, 0f, part.angle);
                    visual.localScale = new Vector3(part.scale.x, part.scale.y, visual.localScale.z);
                    SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
                    renderer.sprite = part.sprite;
                    renderer.sortingOrder = part.sortingOrder;
                }

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                EditorUtility.DisplayDialog("제작 완료", $"프리팹을 생성했습니다.\n{path}", "확인");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (copied)
                    AssetDatabase.DeleteAsset(path);
                EditorUtility.DisplayDialog("제작 실패", exception.Message, "확인");
            }
            finally
            {
                if (root != null)
                    PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool HasRequiredVisualHierarchy(Transform root)
        {
            return FindPartVisual(root, "Body") != null
                && FindPartVisual(root, "LHand") != null
                && FindPartVisual(root, "RHand") != null;
        }

        private void LoadTemplateDefaults()
        {
            if (templatePrefab == null)
                return;

            foreach (Part part in parts)
            {
                Transform visual = FindPartVisual(templatePrefab.transform, part.objectName);
                if (visual == null)
                    return;

                part.position = new Vector2(visual.localPosition.x, visual.localPosition.y);
                part.angle = NormalizeAngle(visual.localEulerAngles.z);
                part.scale = new Vector2(visual.localScale.x, visual.localScale.y);
                Transform visualRoot = visual.parent != null ? visual.parent.parent : null;
                part.parentMatrix = visualRoot != null
                    ? visualRoot.worldToLocalMatrix * visual.parent.localToWorldMatrix
                    : Matrix4x4.identity;
                SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    part.sortingOrder = renderer.sortingOrder;
            }
            FrameAll();
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private static void GetComposedTransform(
            Part part, out Vector2 position, out float angle, out Vector2 scale)
        {
            Matrix4x4 local = Matrix4x4.TRS(
                new Vector3(part.position.x, part.position.y, 0f),
                Quaternion.Euler(0f, 0f, part.angle),
                new Vector3(part.scale.x, part.scale.y, 1f));
            Matrix4x4 composed = part.parentMatrix * local;
            Vector3 worldPosition = composed.MultiplyPoint3x4(Vector3.zero);
            Vector3 xAxis = composed.MultiplyVector(Vector3.right);
            Vector3 yAxis = composed.MultiplyVector(Vector3.up);
            position = new Vector2(worldPosition.x, worldPosition.y);
            angle = Mathf.Atan2(xAxis.y, xAxis.x) * Mathf.Rad2Deg;
            scale = new Vector2(xAxis.magnitude, yAxis.magnitude);
        }

        private static Transform FindPartVisual(Transform root, string partName)
        {
            foreach (Transform visualRoot in root.GetComponentsInChildren<Transform>(true))
            {
                if (visualRoot.name != "Visual")
                    continue;

                Transform part = null;
                foreach (Transform child in visualRoot)
                {
                    if (child.name == partName)
                    {
                        part = child;
                        break;
                    }
                }
                if (part == null)
                    continue;

                foreach (Transform child in part)
                {
                    if (child.name == "Visual" && child.GetComponent<SpriteRenderer>() != null)
                        return child;
                }
            }
            return null;
        }

        private Vector2 WorldToPreview(Rect rect, Vector2 world)
        {
            Vector2 center = rect.center;
            Vector2 value = world + previewPan;
            return center + new Vector2(value.x, -value.y) * previewScale;
        }

        private static Vector2 RotateVector(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }
    }
}
