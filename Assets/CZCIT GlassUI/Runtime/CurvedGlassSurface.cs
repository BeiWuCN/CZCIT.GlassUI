// 把玻璃表面网格弯曲为面板形状，供曲面菜单视觉使用。
using UnityEngine;
using UnityEngine.UI;

namespace CZCIT.GlassUI
{
    // Decorative background only; interactive cards remain real, flat RectTransforms.
    [ExecuteAlways]
    public class CurvedGlassSurface : BaseMeshEffect
    {
        public float radius = 1800;

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || mesh.currentVertCount == 0)
                return;
            var vertex = new UIVertex();
            mesh.PopulateUIVertex(ref vertex, 0);
            var rect = graphic.rectTransform.rect;
            mesh.Clear();
            const int segments = 48;
            for (int x = 0; x <= segments; x++)
            {
                float u = (float)x / segments;
                float px = Mathf.Lerp(rect.xMin, rect.xMax, u), angle = px / radius;
                for (int y = 0; y < 2; y++)
                {
                    var v = vertex;
                    v.position = new Vector3(Mathf.Sin(angle) * radius, Mathf.Lerp(rect.yMin, rect.yMax, y), -radius * (1 - Mathf.Cos(angle)));
                    v.uv0 = new Vector4(u, y, 0, 0);
                    mesh.AddVert(v);
                }
                if (x == 0)
                    continue;
                int i = x * 2;
                mesh.AddTriangle(i - 2, i - 1, i);
                mesh.AddTriangle(i, i - 1, i + 1);
            }
        }
    }
}
