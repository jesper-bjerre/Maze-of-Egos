using MazeOfEgos.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MazeOfEgos.Runtime
{
    // North stays up. Colours and numbers match the five HUD entries.
    public sealed class CompetitionMinimap : MaskableGraphic
    {
        public static readonly Color[] Colours = {
            new Color(.25f,.85f,1), new Color(1,.64f,.23f), new Color(1,.4f,.65f),
            new Color(.73f,.58f,1), new Color(.55f,1,.36f)
        };
        private CompetitionSession session;
        private long lastTick = -1;
        public Vector2 MapPoint(Point2 point)
        {
            Rect r = rectTransform.rect;
            float unit = Mathf.Min(r.width / session.Maze.Definition.width, r.height / session.Maze.Definition.height);
            return new Vector2(((float)point.x + .5f - session.Maze.Definition.width / 2f) * unit,
                (session.Maze.Definition.height / 2f - (float)point.y - .5f) * unit);
        }
        public void Render(CompetitionSession value)
        {
            if (session != value || value.TickIndex / 6 != lastTick) { session = value; lastTick = value.TickIndex / 6; SetVerticesDirty(); }
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); if (session == null) return;
            var map = session.Maze;
            float unit = Mathf.Min(rectTransform.rect.width / map.Definition.width, rectTransform.rect.height / map.Definition.height);
            for (int y = 0; y < map.Definition.height; y++) for (int x = 0; x < map.Definition.width; x++)
                if (map.IsWalkable(new Cell(x,y),true)) Square(mesh,MapPoint(new Point2(x,y)),unit*.5f,new Color(.24f,.35f,.39f));
            for (int i = 0; i < session.HeartCount; i++)
            {
                Vector2 p = MapPoint(session.HeartCell(i).Position); float d = unit * .22f;
                Tri(mesh,p+new Vector2(0,d),p+new Vector2(d,0),p+new Vector2(0,-d),new Color(1,.53f,.62f));
                Tri(mesh,p+new Vector2(0,d),p+new Vector2(0,-d),p+new Vector2(-d,0),new Color(1,.53f,.62f));
            }
            for (int i = 0; i < 5; i++)
            {
                var actor = session.Actor(i); if (actor.Life != CompetitionLife.Active) continue;
                Vector2 p = MapPoint(actor.Position); float radius = unit * .9f;
                Square(mesh,p,radius + 1.3f,actor.Id == session.HumanId ? Color.white : new Color(.015f,.03f,.04f));
                Square(mesh,p,radius,Colours[i]);
            }
        }
        private static void Square(VertexHelper mesh,Vector2 p,float d,Color tint)
        { Tri(mesh,p+new Vector2(-d,-d),p+new Vector2(-d,d),p+new Vector2(d,d),tint); Tri(mesh,p+new Vector2(-d,-d),p+new Vector2(d,d),p+new Vector2(d,-d),tint); }
        private static void Tri(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Color tint)
        { int n=mesh.currentVertCount;mesh.AddVert(a,tint,Vector2.zero);mesh.AddVert(b,tint,Vector2.zero);mesh.AddVert(c,tint,Vector2.zero);mesh.AddTriangle(n,n+1,n+2); }
    }
}
