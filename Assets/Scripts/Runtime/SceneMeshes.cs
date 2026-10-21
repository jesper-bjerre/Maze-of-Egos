using System.Collections.Generic;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    public static class SceneMeshes
    {
        private static Material litTemplate, emissiveTemplate;
        public static Mesh BananaPeel()
        {
            var vertices = new List<Vector3>(); var indices = new List<int>();
            for (int peel = 0; peel < 3; peel++)
            {
                float angle = peel * Mathf.PI * 2 / 3;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 side = new Vector3(-direction.z, 0, direction.x);
                int start = vertices.Count;
                for (int i = 0; i <= 12; i++)
                {
                    float t = i / 12f, width = Mathf.Sin(Mathf.PI * t) * 0.09f + 0.012f;
                    Vector3 point = direction * (t * 0.43f) + Vector3.up * (0.16f * (1 - t) + Mathf.Sin(t * Mathf.PI * 1.5f) * 0.09f + 0.055f);
                    vertices.Add(point - side * width); vertices.Add(point + side * width);
                    if (i == 0) continue;
                    int a = start + (i - 1) * 2;
                    indices.AddRange(new[] { a, a + 1, a + 2, a + 1, a + 3, a + 2 });
                }
            }
            var mesh = new Mesh { name = "Three curled banana peels" };
            mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        public static Mesh Heart()
        {
            const int segments = 40;
            var vertices = new List<Vector3>(); var indices = new List<int>();
            // Five concentric outline rings give a genuinely rounded, volumetric heart.
            float[] scales = { 0.05f, 0.83f, 1, 0.83f, 0.05f };
            float[] depths = { 0.20f, 0.14f, 0, -0.14f, -0.20f };
            for (int ring = 0; ring < scales.Length; ring++)
                for (int i = 0; i < segments; i++)
                {
                    float t = i * Mathf.PI * 2 / segments;
                    float x = 16 * Mathf.Pow(Mathf.Sin(t), 3) / 32;
                    float y = (13 * Mathf.Cos(t) - 5 * Mathf.Cos(2 * t) - 2 * Mathf.Cos(3 * t) - Mathf.Cos(4 * t) + 2) / 32;
                    vertices.Add(new Vector3(x * scales[ring], y * scales[ring], depths[ring]));
                    if (ring == 0) continue;
                    int a = (ring - 1) * segments + i, b = (ring - 1) * segments + (i + 1) % segments;
                    int c = ring * segments + i, d = ring * segments + (i + 1) % segments;
                    indices.Add(a); indices.Add(c); indices.Add(b); indices.Add(b); indices.Add(c); indices.Add(d);
                }
            int front = vertices.Count; vertices.Add(new Vector3(0, 0, 0.205f));
            int back = vertices.Count; vertices.Add(new Vector3(0, 0, -0.205f));
            for (int i = 0; i < segments; i++)
            {
                indices.Add(front); indices.Add(i); indices.Add((i + 1) % segments);
                indices.Add(back); indices.Add(4 * segments + (i + 1) % segments); indices.Add(4 * segments + i);
            }
            var mesh = new Mesh { name = "Rounded heart" }; mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        public static Mesh RoundedBox(Vector3 size, float bevel = 0.08f)
        {
            var vertices = new List<Vector3>(); var indices = new List<int>();
            Vector3 half = size * 0.5f;
            Vector3 inner = half - Vector3.one * bevel;
            Vector3[] normals = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (Vector3 normal in normals)
            {
                Vector3 u = normal.y != 0 ? Vector3.right : Vector3.up;
                Vector3 v = Vector3.Cross(normal, u);
                int offset = vertices.Count;
                for (int y = 0; y < 5; y++) for (int x = 0; x < 5; x++)
                {
                    float a = Coordinate(x, Vector3.Dot(Abs(u), half), bevel);
                    float b = Coordinate(y, Vector3.Dot(Abs(v), half), bevel);
                    Vector3 p = Vector3.Scale(normal, half) + u * a + v * b;
                    Vector3 center = new Vector3(Mathf.Clamp(p.x, -inner.x, inner.x), Mathf.Clamp(p.y, -inner.y, inner.y), Mathf.Clamp(p.z, -inner.z, inner.z));
                    vertices.Add(center + (p - center).normalized * bevel);
                }
                for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++)
                {
                    int a = offset + y * 5 + x, b = a + 1, c = a + 5, d = c + 1;
                    indices.Add(a); indices.Add(b); indices.Add(c); indices.Add(b); indices.Add(d); indices.Add(c);
                }
            }
            var mesh = new Mesh { name = "Bevelled wall" }; mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        private static Vector3 Abs(Vector3 value) => new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        private static float Coordinate(int index, float half, float bevel) => index == 0 ? -half : index == 1 ? -half + bevel : index == 2 ? 0 : index == 3 ? half - bevel : half;
        public static Material Material(string name, Color color, float metallic = 0, float smoothness = 0.35f, bool emissive = false)
        {
            if (!litTemplate) litTemplate = Resources.Load<Material>("Materials/GardenLit");
            if (!emissiveTemplate) emissiveTemplate = Resources.Load<Material>("Materials/GardenEmissive");
            Material template = emissive ? emissiveTemplate : litTemplate;
            if (!template) throw new System.InvalidOperationException("Missing garden material resource.");
            var material = new Material(template) { name = name, color = color, enableInstancing = true };
            material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smoothness);
            if (emissive) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * 0.65f); }
            return material;
        }
        public static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material material, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
        public static Transform Primitive(string name, Transform parent, PrimitiveType type, Material material, Vector3 position, Vector3 scale)
        {
            GameObject go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Object.Destroy(go.GetComponent<Collider>()); return go.transform;
        }
    }
}
