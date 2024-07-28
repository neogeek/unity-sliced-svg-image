using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;

namespace ScottDoxey.UI
{

    [AddComponentMenu("UI/Sliced SVG Image")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class SlicedSVGImage : SVGImage
    {

        private static readonly Vector2[] s_VertScratch = new Vector2[4];

        private static readonly Vector2[] s_UVScratch = new Vector2[4];

        private float m_CachedReferencePixelsPerUnit = 100;

        protected float multipliedPixelsPerUnit => pixelsPerUnit * m_PixelsPerUnitMultiplier;

        [SerializeField]
        private bool m_FillCenter = true;

        [SerializeField]
        private float m_PixelsPerUnitMultiplier = 1.0f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (sprite == null)
            {
                base.OnPopulateMesh(vh);

                return;
            }

            GenerateSlicedSprite(vh);
        }

        public float pixelsPerUnit
        {
            get
            {
                float spritePixelsPerUnit = 100;

                if (sprite)
                {
                    spritePixelsPerUnit = sprite.pixelsPerUnit;
                }

                if (canvas)
                {
                    m_CachedReferencePixelsPerUnit = canvas.referencePixelsPerUnit;
                }

                return spritePixelsPerUnit / m_CachedReferencePixelsPerUnit;
            }
        }

        private void GenerateSlicedSprite(VertexHelper vh)
        {
            if (sprite == null)
            {
                return;
            }

            var border = sprite.border;
            var rect = GetPixelAdjustedRect();

            var outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            var innerUV = new Vector4(border.x / 100, border.y / 100, 1 - border.z / 100, 1 - border.w / 100);
            var padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite) / multipliedPixelsPerUnit;

            SetVertices(padding, border, rect);
            SetUVs(outerUV, innerUV);

            vh.Clear();

            for (var x = 0; x < 3; ++x)
            {
                for (var y = 0; y < 3; ++y)
                {
                    if (!m_FillCenter && x == 1 && y == 1) continue;

                    AddQuad(vh, x, y);
                }
            }
        }

        private static void SetVertices(Vector4 padding, Vector4 border, Rect rect)
        {
            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = border.x;
            s_VertScratch[1].y = border.y;

            s_VertScratch[2].x = rect.width - border.z;
            s_VertScratch[2].y = rect.height - border.w;

            for (var i = 0; i < s_VertScratch.Length; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }
        }

        private static void SetUVs(Vector4 outerUV, Vector4 innerUV)
        {
            s_UVScratch[0] = new Vector2(outerUV.x, outerUV.y);
            s_UVScratch[1] = new Vector2(innerUV.x, innerUV.y);
            s_UVScratch[2] = new Vector2(innerUV.z, innerUV.w);
            s_UVScratch[3] = new Vector2(outerUV.z, outerUV.w);
        }

        private void AddQuad(VertexHelper vh, int x, int y)
        {
            var startIndex = vh.currentVertCount;

            var posMin = new Vector2(s_VertScratch[x].x, s_VertScratch[y].y);
            var posMax = new Vector2(s_VertScratch[x + 1].x, s_VertScratch[y + 1].y);

            var uvMin = new Vector2(s_UVScratch[x].x, s_UVScratch[y].y);
            var uvMax = new Vector2(s_UVScratch[x + 1].x, s_UVScratch[y + 1].y);

            vh.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vh.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vh.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vh.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

    }

}
