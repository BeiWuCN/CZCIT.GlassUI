// 先细分 UI 三角形再映射到曲面，避免大三角形直接弯曲产生明显折面。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CZCIT.GlassUI
{
    [ExecuteAlways]
    public class CurvedGraphicMesh : BaseMeshEffect
    {
        public Transform root;
        public float radius = 1600;
        readonly List<UIVertex> input = new List<UIVertex>();

        public static Vector3 Bend(Vector3 p, float r)
        {
            float a = p.x / r;
            return new Vector3((r + p.z) * Mathf.Sin(a), p.y, (r + p.z) * Mathf.Cos(a) - r);
        }

        public static Vector3 Flatten(Vector3 p, float r)
        {
            return new Vector3(Mathf.Atan2(p.x, p.z + r) * r, p.y, new Vector2(p.x, p.z + r).magnitude - r);
        }

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || !root)
                return;
            input.Clear();
            mesh.GetUIVertexStream(input);
            mesh.Clear();
            // 所有子控件先转换到同一个面板坐标系弯曲，避免各自围绕局部原点变形。
            var toRoot = root.worldToLocalMatrix * transform.localToWorldMatrix;
            var fromRoot = transform.worldToLocalMatrix * root.localToWorldMatrix;
            for (int t = 0; t < input.Count; t += 3)
            {
                var a = input[t];
                var b = input[t + 1];
                var c = input[t + 2];
                float length = Mathf.Max(
                    Vector3.Distance(toRoot.MultiplyPoint3x4(a.position), toRoot.MultiplyPoint3x4(b.position)),
                    Vector3.Distance(toRoot.MultiplyPoint3x4(a.position), toRoot.MultiplyPoint3x4(c.position)));
                int n = Mathf.Clamp(Mathf.CeilToInt(length / 28), 1, 40);
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n - i; j++)
                    {
                        Add(a, b, c, (float)i / n, (float)j / n, toRoot, fromRoot, mesh);
                        Add(a, b, c, (float)(i + 1) / n, (float)j / n, toRoot, fromRoot, mesh);
                        Add(a, b, c, (float)i / n, (float)(j + 1) / n, toRoot, fromRoot, mesh);
                        if (j < n - i - 1)
                        {
                            Add(a, b, c, (float)(i + 1) / n, (float)j / n, toRoot, fromRoot, mesh);
                            Add(a, b, c, (float)(i + 1) / n, (float)(j + 1) / n, toRoot, fromRoot, mesh);
                            Add(a, b, c, (float)i / n, (float)(j + 1) / n, toRoot, fromRoot, mesh);
                        }
                    }
            }
        }

        void Add(UIVertex a, UIVertex b, UIVertex c, float u, float v, Matrix4x4 toRoot, Matrix4x4 fromRoot, VertexHelper mesh)
        {
            var vertex = a;
            float w = 1 - u - v;
            vertex.position = fromRoot.MultiplyPoint3x4(Bend(toRoot.MultiplyPoint3x4(a.position * w + b.position * u + c.position * v), radius));
            vertex.uv0 = a.uv0 * w + b.uv0 * u + c.uv0 * v;
            vertex.color = (Color)a.color * w + (Color)b.color * u + (Color)c.color * v;
            int index = mesh.currentVertCount;
            mesh.AddVert(vertex);
            if (index % 3 == 2)
                mesh.AddTriangle(index - 2, index - 1, index);
        }
    }
}
