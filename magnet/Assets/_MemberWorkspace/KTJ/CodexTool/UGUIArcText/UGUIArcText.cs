using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KTJ.CodexTool
{
    /// <summary>
    /// Bends a legacy UGUI Text mesh into an arc.
    /// Positive angles arch upward and negative angles arch downward.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("UI/Effects/UGUI Arc Text")]
    public sealed class UGUIArcText : BaseMeshEffect
    {
        private const int VerticesPerGlyph = 6;
        private const float MinimumAngle = 0.01f;

        [SerializeField, Range(-180f, 180f)]
        [Tooltip("Total arc angle. Positive values bend upward; negative values bend downward.")]
        private float arcAngle = 30f;

        [SerializeField]
        [Tooltip("Rotates each character so it follows the tangent of the arc.")]
        private bool rotateCharacters = true;

        private readonly List<UIVertex> vertices = new List<UIVertex>();
        private readonly List<GlyphInfo> glyphs = new List<GlyphInfo>();

        public float ArcAngle
        {
            get => arcAngle;
            set
            {
                float clamped = Mathf.Clamp(value, -180f, 180f);
                if (Mathf.Approximately(arcAngle, clamped))
                    return;

                arcAngle = clamped;
                SetVerticesDirty();
            }
        }

        public bool RotateCharacters
        {
            get => rotateCharacters;
            set
            {
                if (rotateCharacters == value)
                    return;

                rotateCharacters = value;
                SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!IsActive() || vertexHelper.currentVertCount == 0 || Mathf.Abs(arcAngle) < MinimumAngle)
                return;

            vertices.Clear();
            vertexHelper.GetUIVertexStream(vertices);

            // Legacy UGUI Text emits two triangles (six stream vertices) per visible glyph.
            if (vertices.Count < VerticesPerGlyph || vertices.Count % VerticesPerGlyph != 0)
                return;

            BuildGlyphInfo();
            BendLines();

            vertexHelper.Clear();
            vertexHelper.AddUIVertexTriangleStream(vertices);
        }

        private void BuildGlyphInfo()
        {
            glyphs.Clear();

            for (int start = 0; start < vertices.Count; start += VerticesPerGlyph)
            {
                Vector3 min = vertices[start].position;
                Vector3 max = min;

                for (int i = 1; i < VerticesPerGlyph; i++)
                {
                    Vector3 position = vertices[start + i].position;
                    min = Vector3.Min(min, position);
                    max = Vector3.Max(max, position);
                }

                glyphs.Add(new GlyphInfo(start, (min + max) * 0.5f, max.y - min.y));
            }
        }

        private void BendLines()
        {
            int lineStart = 0;

            while (lineStart < glyphs.Count)
            {
                int lineEnd = lineStart + 1;
                float referenceY = glyphs[lineStart].Center.y;
                float tolerance = Mathf.Max(1f, glyphs[lineStart].Height * 0.5f);

                while (lineEnd < glyphs.Count)
                {
                    GlyphInfo candidate = glyphs[lineEnd];
                    float candidateTolerance = Mathf.Max(tolerance, candidate.Height * 0.5f);
                    if (Mathf.Abs(candidate.Center.y - referenceY) > candidateTolerance)
                        break;

                    lineEnd++;
                }

                BendLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }
        }

        private void BendLine(int start, int end)
        {
            float minX = glyphs[start].Center.x;
            float maxX = minX;

            for (int i = start + 1; i < end; i++)
            {
                minX = Mathf.Min(minX, glyphs[i].Center.x);
                maxX = Mathf.Max(maxX, glyphs[i].Center.x);
            }

            float width = maxX - minX;
            if (width <= Mathf.Epsilon)
                return;

            float middleX = (minX + maxX) * 0.5f;
            float radians = Mathf.Abs(arcAngle) * Mathf.Deg2Rad;
            float radius = width / radians;
            float direction = Mathf.Sign(arcAngle);
            float edgeCosine = Mathf.Cos(radians * 0.5f);

            for (int glyphIndex = start; glyphIndex < end; glyphIndex++)
            {
                GlyphInfo glyph = glyphs[glyphIndex];
                float normalizedX = (glyph.Center.x - middleX) / width;
                float theta = radians * normalizedX;

                Vector2 curvedCenter = new Vector2(
                    middleX + radius * Mathf.Sin(theta),
                    glyph.Center.y + direction * radius * (Mathf.Cos(theta) - edgeCosine));

                float rotation = rotateCharacters ? -direction * theta : 0f;
                float sine = Mathf.Sin(rotation);
                float cosine = Mathf.Cos(rotation);

                for (int vertexIndex = 0; vertexIndex < VerticesPerGlyph; vertexIndex++)
                {
                    int index = glyph.VertexStart + vertexIndex;
                    UIVertex vertex = vertices[index];
                    Vector3 local = vertex.position - glyph.Center;

                    float rotatedX = local.x * cosine - local.y * sine;
                    float rotatedY = local.x * sine + local.y * cosine;
                    vertex.position = new Vector3(
                        curvedCenter.x + rotatedX,
                        curvedCenter.y + rotatedY,
                        vertex.position.z);
                    vertices[index] = vertex;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetVerticesDirty();
        }

        protected override void OnDisable()
        {
            SetVerticesDirty();
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            arcAngle = Mathf.Clamp(arcAngle, -180f, 180f);
            SetVerticesDirty();
        }
#endif

        private void SetVerticesDirty()
        {
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

        private readonly struct GlyphInfo
        {
            public readonly int VertexStart;
            public readonly Vector3 Center;
            public readonly float Height;

            public GlyphInfo(int vertexStart, Vector3 center, float height)
            {
                VertexStart = vertexStart;
                Center = center;
                Height = height;
            }
        }
    }
}
