using UnityEngine;
using UnityEngine.UI;

namespace MazeOfEgos.Runtime
{
    // Blend the cover's empty right edge into its dark extension on wide screens.
    public sealed class MenuEdgeFade : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); Rect r = GetPixelAdjustedRect();
            Color clear = color; clear.a = 0;
            mesh.AddVert(new Vector3(r.xMin, r.yMin), clear, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMin, r.yMax), clear, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax, r.yMax), color, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax, r.yMin), color, Vector2.zero);
            mesh.AddTriangle(0, 1, 2); mesh.AddTriangle(0, 2, 3);
        }
    }
}
