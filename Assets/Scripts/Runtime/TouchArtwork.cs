using UnityEngine;
using UnityEngine.UI;

namespace MazeOfEgos.Runtime
{
    // Vector artwork stays sharp at every device scale and needs no texture download.
    public sealed class TouchArtwork : MaskableGraphic
    {
        public enum Shape { Base, Thumb, Button, Empty, Banana, Oil, Boost, Heart, Balloon, Pie, Medal, Trophy }
        private Shape shape;
        public Shape Artwork { get => shape; set { if (shape == value) return; shape = value; SetVerticesDirty(); } }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); float unit = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 100f;
            if (shape == Shape.Base || shape == Shape.Button)
            {
                Disc(mesh, Vector2.zero, 47 * unit, new Color(0.025f, 0.08f, 0.10f, 0.5f) * color);
                Ring(mesh, 46 * unit, 1.6f * unit, color);
                if (shape == Shape.Base)
                {
                    Ring(mesh, 31 * unit, 0.5f * unit, color * new Color(1, 1, 1, 0.25f));
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = i * Mathf.PI / 2; Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        Line(mesh, direction * 39 * unit, direction * 42 * unit, 1.4f * unit, color);
                    }
                }
            }
            else if (shape == Shape.Thumb)
            {
                Disc(mesh, Vector2.zero, 45 * unit, color * new Color(0.85f, 0.78f, 0.53f, 0.75f));
                Ring(mesh, 45 * unit, 1.8f * unit, color);
                Disc(mesh, Vector2.zero, 7 * unit, color * new Color(1, 1, 1, 0.5f));
            }
            else if (shape == Shape.Medal)
            {
                Quad(mesh,new Vector2(-30,43)*unit,new Vector2(-7,43)*unit,new Vector2(13,-5)*unit,new Vector2(-9,-5)*unit,new Color(.22f,.55f,.85f));
                Quad(mesh,new Vector2(7,43)*unit,new Vector2(30,43)*unit,new Vector2(9,-5)*unit,new Vector2(-13,-5)*unit,new Color(.32f,.67f,.95f));
                Disc(mesh,new Vector2(0,-13)*unit,30*unit,color);
                Disc(mesh,new Vector2(0,-13)*unit,22*unit,color*new Color(.82f,.82f,.82f));
                Disc(mesh,new Vector2(0,-13)*unit,8*unit,color);
            }
            else if (shape == Shape.Trophy)
            {
                Quad(mesh,new Vector2(-27,34)*unit,new Vector2(27,34)*unit,new Vector2(17,-5)*unit,new Vector2(-17,-5)*unit,color);
                Line(mesh,new Vector2(-27,24)*unit,new Vector2(-42,24)*unit,7*unit,color);
                Line(mesh,new Vector2(-42,24)*unit,new Vector2(-31,-4)*unit,7*unit,color);
                Line(mesh,new Vector2(-31,-4)*unit,new Vector2(-14,-8)*unit,7*unit,color);
                Line(mesh,new Vector2(27,24)*unit,new Vector2(42,24)*unit,7*unit,color);
                Line(mesh,new Vector2(42,24)*unit,new Vector2(31,-4)*unit,7*unit,color);
                Line(mesh,new Vector2(31,-4)*unit,new Vector2(14,-8)*unit,7*unit,color);
                Line(mesh,new Vector2(0,-5)*unit,new Vector2(0,-30)*unit,9*unit,color);
                Line(mesh,new Vector2(-25,-34)*unit,new Vector2(25,-34)*unit,10*unit,color);
            }
            else if (shape == Shape.Balloon)
            {
                Disc(mesh, new Vector2(0, 5) * unit, 29 * unit, color);
                Triangle(mesh, new Vector2(0,-24)*unit, new Vector2(-8,-37)*unit, new Vector2(8,-37)*unit,color);
                Line(mesh,new Vector2(-12,14)*unit,new Vector2(-8,23)*unit,4*unit,Color.white);
            }
            else if (shape == Shape.Pie)
            {
                for (int i = -1; i <= 1; i++) Disc(mesh,new Vector2(i*18,5)*unit,18*unit,color);
                Line(mesh,new Vector2(-35,-16)*unit,new Vector2(35,-16)*unit,8*unit,new Color(1,.65f,.25f)*color);
                Disc(mesh,new Vector2(0,29)*unit,7*unit,new Color(1,.25f,.3f)*color);
            }
            else if (shape == Shape.Heart)
            {
                Vector2 previous = HeartPoint(0) * unit;
                for (int i = 1; i <= 48; i++)
                { Vector2 next = HeartPoint(i * Mathf.PI * 2 / 48) * unit; Triangle(mesh, Vector2.zero, previous, next, color); previous = next; }
            }
            else if (shape == Shape.Banana)
            {
                Vector2 previous = new Vector2(-30, 17) * unit;
                for (int i = 1; i <= 20; i++)
                {
                    float t = i / 20f; Vector2 next = new Vector2(-30 + 60 * t, 17 - Mathf.Sin(t * Mathf.PI) * 44) * unit;
                    Line(mesh, previous, next, (4 + Mathf.Sin(t * Mathf.PI) * 8) * unit, color); previous = next;
                }
                Line(mesh, new Vector2(30, 17) * unit, new Vector2(34, 27) * unit, 5 * unit, color);
            }
            else if (shape == Shape.Oil)
            {
                Vector2 previous = new Vector2(0, 37) * unit;
                for (int i = 1; i <= 40; i++)
                {
                    float a = i * Mathf.PI * 2 / 40;
                    Vector2 next = new Vector2(Mathf.Sin(a) * (21 - 10 * Mathf.Cos(a)), 8 + Mathf.Cos(a) * 29) * unit;
                    Triangle(mesh, Vector2.zero, previous, next, color); previous = next;
                }
            }
            else if (shape == Shape.Boost)
            {
                for (int i = 0; i < 2; i++)
                {
                    float x = -16 + i * 29;
                    Line(mesh, new Vector2(x - 12, 27) * unit, new Vector2(x + 10, 0) * unit, 8 * unit, color);
                    Line(mesh, new Vector2(x + 10, 0) * unit, new Vector2(x - 12, -27) * unit, 8 * unit, color);
                }
            }
            else
            {
                Ring(mesh, 25 * unit, 2 * unit, color);
                Line(mesh, new Vector2(-17, -17) * unit, new Vector2(17, 17) * unit, 2 * unit, color);
            }
        }
        private static Vector2 HeartPoint(float t) => new Vector2(16 * Mathf.Pow(Mathf.Sin(t), 3),
            13 * Mathf.Cos(t) - 5 * Mathf.Cos(2 * t) - 2 * Mathf.Cos(3 * t) - Mathf.Cos(4 * t) + 2) * 2.7f;
        public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
        {
            if (!base.Raycast(screenPoint, eventCamera)) return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 point);
            return point.sqrMagnitude <= Mathf.Pow(Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f, 2);
        }
        private static void Disc(VertexHelper mesh, Vector2 center, float radius, Color tint)
        {
            for (int i = 0; i < 64; i++)
            {
                float a = i * Mathf.PI / 32, b = (i + 1) * Mathf.PI / 32;
                Triangle(mesh, center, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, tint);
            }
        }
        private static void Ring(VertexHelper mesh, float radius, float thickness, Color tint)
        {
            for (int i = 0; i < 64; i++)
            {
                float a = i * Mathf.PI / 32, b = (i + 1) * Mathf.PI / 32;
                Vector2 u = new Vector2(Mathf.Cos(a), Mathf.Sin(a)), v = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                Quad(mesh, u * radius, v * radius, v * (radius - thickness), u * (radius - thickness), tint);
            }
        }
        private static void Line(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color tint)
        {
            Vector2 normal = new Vector2(from.y - to.y, to.x - from.x).normalized * (width * 0.5f);
            Quad(mesh, from - normal, to - normal, to + normal, from + normal, tint);
        }
        private static void Triangle(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Color tint)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero); mesh.AddVert(c, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
        }
        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero);
            mesh.AddVert(c, tint, Vector2.zero); mesh.AddVert(d, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
