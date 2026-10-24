using System;
using System.Collections.Generic;
using MazeOfEgos.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace MazeOfEgos.Runtime
{
    public sealed class CompetitionView
    {
        private readonly CompetitionApp app;
        private readonly MazeMap maze;
        private readonly Transform root;
        private readonly Transform[][] walls;
        private readonly Material[] shortcutSignals;
        private readonly Transform[] hearts = new Transform[240], crates;
        private readonly Transform[] avatars = new Transform[5], bubbles = new Transform[5], markers = new Transform[5];
        private readonly ImportedCharacterPose[] poses = new ImportedCharacterPose[5];
        private readonly TextMesh[] names = new TextMesh[5];
        private readonly Dictionary<int, Effect> effects = new Dictionary<int, Effect>();
        private readonly HashSet<int> live = new HashSet<int>();
        private readonly List<int> removed = new List<int>();
        private readonly Material blue, cream, banana, oil, warning;
        private readonly Transform aim;
        private readonly LineRenderer aimLine;
        private readonly Font labelFont;
        private readonly Material labelMaterial;
        private const float WallHeight = 1.15f;
        public readonly Camera Camera;
        private sealed class Effect { public Transform root, projectile, warning; public LineRenderer line; }
        public static Vector3 World(Point2 p, float h = 0) => MazeView.World(p, h);
        public CompetitionView(CompetitionApp app)
        {
            this.app = app; maze = app.Session.Maze;
            labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelMaterial = new Material(Resources.Load<Material>("Materials/WorldText")); labelMaterial.mainTexture = labelFont.material.mainTexture;
            root = new GameObject("Competition garden").transform;
            Camera = new GameObject("Local competition camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            Camera.orthographic = true; Camera.orthographicSize = (float)LocalArenaView.HalfHeight; Camera.nearClipPlane = .06f; Camera.farClipPlane = 100;
            Camera.backgroundColor = new Color(.58f, .73f, .76f); Camera.clearFlags = CameraClearFlags.SolidColor; Camera.cullingMask &= ~(1 << 30);
            RenderSettings.ambientMode = AmbientMode.Trilight; RenderSettings.ambientSkyColor = new Color(.73f,.81f,.83f);
            RenderSettings.ambientEquatorColor = new Color(.62f,.65f,.57f); RenderSettings.ambientGroundColor = new Color(.33f,.35f,.29f); RenderSettings.fog = false;
            var sun = new GameObject("Warm afternoon", typeof(Light)).GetComponent<Light>(); sun.type = LightType.Directional;
            sun.color = new Color(1,.86f,.65f); sun.intensity = 1.8f; sun.transform.rotation = Quaternion.Euler(48,-30,0); sun.shadows = LightShadows.Soft;
            Material floor = SceneMeshes.Material("Warm limestone", new Color(0.66f, 0.60f, 0.43f), 0, 0.12f);
            Material brass = SceneMeshes.Material("Brushed brass", new Color(0.67f, 0.45f, 0.16f), 0.55f, 0.5f);
            Material[] wallMaterials = {
                SceneMeshes.Material("North sage", new Color(0.33f, 0.49f, 0.39f)),
                SceneMeshes.Material("East terracotta", new Color(0.66f, 0.39f, 0.29f)),
                SceneMeshes.Material("West blue wood", new Color(0.30f, 0.44f, 0.52f)),
                SceneMeshes.Material("South warm plaster", new Color(0.70f, 0.57f, 0.36f)) };
            var staticRoot = new GameObject("Static architecture").transform; staticRoot.SetParent(root);
            Material grass = SceneMeshes.Material("Garden surroundings", new Color(.24f, .35f, .25f), 0, .12f);
            SceneMeshes.Primitive("Garden surroundings", staticRoot, PrimitiveType.Cube, grass,
                World(new Point2((maze.Definition.width - 1) / 2.0, (maze.Definition.height - 1) / 2.0), -.4f),
                new Vector3((maze.Definition.width + 40) * 2.5f, .2f, (maze.Definition.height + 40) * 2.5f));
            SceneMeshes.Primitive("Garden floor", staticRoot, PrimitiveType.Cube, floor,
                World(new Point2((maze.Definition.width - 1) / 2.0, (maze.Definition.height - 1) / 2.0), -0.15f),
                new Vector3(maze.Definition.width * 2.5f, 0.30f, maze.Definition.height * 2.5f));
            Mesh wallMesh = SceneMeshes.RoundedBox(new Vector3(2.5f, WallHeight, 2.5f), 0.10f);
            walls = new Transform[maze.Definition.shortcutCandidates.Length][];
            shortcutSignals = new Material[walls.Length];
            var movingCells = new HashSet<Cell>();
            for (int i = 0; i < walls.Length; i++)
            {
                shortcutSignals[i] = SceneMeshes.Material("Shortcut signal " + i, new Color(1, 0.66f, 0.16f), 0.3f, 0.45f, true);
                int[][] endpoints = maze.Definition.shortcutCandidates[i].endpoints;
                bool acrossX = Math.Abs(endpoints[1][0] - endpoints[0][0]) >= Math.Abs(endpoints[1][1] - endpoints[0][1]);
                Vector3 approach = acrossX ? Vector3.right : Vector3.forward;
                int[][] cells = maze.Definition.shortcutCandidates[i].wallCells; walls[i] = new Transform[cells.Length];
                for (int j = 0; j < cells.Length; j++)
                {
                    Cell cell = MazeMap.FromPair(cells[j]); movingCells.Add(cell);
                    walls[i][j] = SceneMeshes.MeshObject("Retracting wall " + i, root, wallMesh, brass, World(cell.Position, WallHeight / 2), Vector3.one).transform;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector3 stripe = World(cell.Position, 0.015f) + approach * (side * 1.77f);
                        SceneMeshes.Primitive("Shortcut floor signal " + i, root, PrimitiveType.Cube, shortcutSignals[i], stripe,
                            acrossX ? new Vector3(0.14f, 0.025f, 1.75f) : new Vector3(1.75f, 0.025f, 0.14f));
                        SceneMeshes.Primitive("Shortcut wall signal " + i, walls[i][j], PrimitiveType.Sphere, shortcutSignals[i],
                            approach * (side * 1.27f), Vector3.one * 0.18f);
                    }
                }
            }
            for (int y = 0; y < maze.Definition.height; y++) for (int x = 0; x < maze.Definition.width; x++)
            {
                Cell cell = new Cell(x, y);
                if (maze.IsWalkable(cell, true) || movingCells.Contains(cell)) continue;
                int zone = (y < 15 ? 0 : 2) + (x < 14 ? 0 : 1);
                GameObject wall = SceneMeshes.MeshObject("Wall " + cell, staticRoot, wallMesh, wallMaterials[zone], World(cell.Position, WallHeight / 2), Vector3.one);
                wall.isStatic = true;
            }
            // Matching tunnel extensions mask the logical wrap; character copies preserve visibility.
            for (int side = 0; side < 2; side++) for (int i = 1; i <= 6; i++)
            {
                int x = side == 0 ? -i : maze.Definition.width - 1 + i;
                foreach (int y in new[] { 13, 15 })
                    SceneMeshes.MeshObject("Tunnel wall", staticRoot, wallMesh, wallMaterials[y < 15 ? 0 : 2], World(new Point2(x, y), WallHeight / 2), Vector3.one).isStatic = true;
                SceneMeshes.Primitive("Tunnel floor", staticRoot, PrimitiveType.Cube, floor, World(new Point2(x, 14), -0.15f), new Vector3(2.5f, 0.3f, 2.5f));
            }
            GardenDressing.Build(maze, staticRoot); StaticBatchingUtility.Combine(staticRoot.gameObject);
            Material pink = SceneMeshes.Material("Ordinary heart", new Color(.96f,.16f,.3f), .05f,.7f,true);
            Mesh heart = SceneMeshes.Heart();
            for (int i = 0; i < hearts.Length; i++) hearts[i] = SceneMeshes.MeshObject("Ordinary heart " + i, root, heart, pink, Vector3.zero, Vector3.one * .32f).transform;
            Material teal = SceneMeshes.Material("Question box", new Color(.23f,.51f,.62f), .3f,.6f);
            crates = new Transform[maze.Definition.itemCells.Length];
            for (int i = 0; i < crates.Length; i++) {
                crates[i] = SceneMeshes.Primitive("Question box " + i, root, PrimitiveType.Cube, teal, World(MazeMap.FromPair(maze.Definition.itemCells[i]).Position, 1.35f), Vector3.one * .65f);
                var text = Label("?", crates[i], new Vector3(0,.6f,0), .16f, Color.white); text.transform.rotation = Quaternion.Euler(55,25,0);
            }
            blue = SceneMeshes.Material("Water balloon", new Color(.15f,.67f,1),.1f,.8f,true);
            cream = SceneMeshes.Material("Whipped cream", new Color(1,.92f,.75f),0,.15f);
            banana = SceneMeshes.Material("Banana peel", new Color(1,.8f,.12f),0,.5f); banana.SetFloat("_Cull",0);
            oil = SceneMeshes.Material("Cartoon oil", new Color(.14f,.09f,.24f),.35f,.95f);
            warning = SceneMeshes.Material("Attack warning", new Color(.3f,.8f,1),0,.3f,true);
            Material protection = SceneMeshes.Material("Protection", new Color(.4f,.85f,1),.1f,.5f,true);
            Material gold = SceneMeshes.Material("You marker", new Color(1,.74f,.18f),.1f,.6f,true);
            for (int i = 0; i < 5; i++) {
                string id = CompetitionSession.ParticipantIds[i]; avatars[i] = new GameObject(id + " competitor").transform; avatars[i].SetParent(root);
                poses[i] = new ImportedCharacterPose(avatars[i], Resources.Load<GameObject>("CharacterModels/" + id), id);
                names[i] = Label(CharacterRoster.Get(id).displayName, avatars[i], new Vector3(0,2.25f,0), .065f, Color.white);
                markers[i] = Ring("YOU marker", avatars[i], gold, .7f, .06f); markers[i].localPosition = new Vector3(0,.035f,0);
                bubbles[i] = Ring("Respawn protection", avatars[i], protection, .85f, .12f); bubbles[i].localPosition = new Vector3(0,.055f,0);
            }
            aim = Ring("Aim landing area", root, warning, 1,.055f); aimLine = Line("Aim corridor", root, warning, .065f);
        }
        private TextMesh Label(string text, Transform parent, Vector3 position, float size, Color color)
        {
            var go = new GameObject(text, typeof(TextMesh)); go.transform.SetParent(parent,false); go.transform.localPosition = position;
            var label = go.GetComponent<TextMesh>(); label.text = text; label.characterSize = size; label.fontSize = 48; label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center; label.color = color; label.font = labelFont; label.GetComponent<Renderer>().sharedMaterial = labelMaterial; return label;
        }
        private static LineRenderer Line(string name, Transform parent, Material material, float width)
        {
            var line = new GameObject(name,typeof(LineRenderer)).GetComponent<LineRenderer>(); line.transform.SetParent(parent,false);
            line.sharedMaterial = material; line.startWidth = line.endWidth = width; line.positionCount = 2; line.useWorldSpace = true; line.numCapVertices = 3; return line;
        }
        private static Transform Ring(string name, Transform parent, Material material, float radius, float width)
        {
            var line = Line(name,parent,material,width); line.useWorldSpace = false; line.loop = true; line.positionCount = 40;
            for (int i = 0; i < 40; i++) { float a = i * Mathf.PI * 2 / 40; line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)); }
            return line.transform;
        }
        public Point2 ScreenDirection(Point2 direction) => LocalArenaView.Project(direction, LocalArenaView.TargetHeight);
        public void ResetPoses() { foreach (var pose in poses) pose.Reset(); foreach (var pair in effects) UnityEngine.Object.Destroy(pair.Value.root.gameObject); effects.Clear(); }
        public void Render(CompetitionSession s, Point2[] previous, float alpha, bool playingView)
        {
            var human = s.Actor(s.HumanIndex); Point2 centre = human.Position;
            bool interpolate = s.Phase == CompetitionPhase.Playing;
            if (interpolate) centre = s.Maze.Wrap(Point2.Lerp(previous[s.HumanIndex], s.Maze.Near(centre,previous[s.HumanIndex]),alpha));
            Quaternion rotation = Quaternion.Euler((float)LocalArenaView.Pitch,(float)LocalArenaView.Yaw,0);
            Camera.aspect = (float)Screen.width / Screen.height;
            Camera.transform.SetPositionAndRotation(World(centre,(float)LocalArenaView.TargetHeight) - rotation * Vector3.forward * (float)LocalArenaView.Distance, rotation);
            root.gameObject.SetActive(playingView);
            for (int i = 0; i < hearts.Length; i++) {
                bool active = i < s.HeartCount; hearts[i].gameObject.SetActive(active);
                if (active) { hearts[i].position = World(s.Maze.Near(s.HeartCell(i).Position,centre),1); hearts[i].rotation = Quaternion.Euler(0,app.Settings.reducedMotion ? 25 : (float)(s.TickIndex*.7),0); }
            }
            for (int i = 0; i < crates.Length; i++) { crates[i].gameObject.SetActive(s.CrateAvailable(i)); crates[i].position = World(s.Maze.Near(s.CrateCell(i).Position,centre),1.35f); }
            for (int i = 0; i < 5; i++) {
                var a = s.Actor(i); Point2 p = interpolate ? s.Maze.Wrap(Point2.Lerp(previous[i],s.Maze.Near(a.Position,previous[i]),alpha)) : a.Position;
                avatars[i].position = World(s.Maze.Near(p,centre)); avatars[i].rotation = Quaternion.Euler(0,a.Heading*90,0);
                // Core north is world +Z. The model's forward marker is normalised in ImportedCharacterPose.
                poses[i].Instance.SetActive(a.Life == CompetitionLife.Active || s.TickIndex - a.KnockoutAt < 36);
                poses[i].RenderCompetition(s,a,app.Settings.reducedMotion);
                names[i].transform.rotation = rotation;
                names[i].text = a.Id == s.HumanId ? "YOU" : CharacterRoster.Get(a.Id).displayName;
                names[i].color = a.Id == s.HumanId ? new Color(1,.85f,.3f) : Color.white;
                markers[i].gameObject.SetActive(a.Id == s.HumanId && a.Life == CompetitionLife.Active);
                bubbles[i].gameObject.SetActive(a.ProtectedUntil > s.TickIndex && a.Life == CompetitionLife.Active);
            }
            for (int i = 0; i < walls.Length; i++) {
                WallPhase phase = s.ShortcutPhase(i); float fraction = phase == WallPhase.Open ? 1 : phase == WallPhase.Opening ? 1 - (float)(s.ShortcutUntil(i)-s.TickIndex)/75 : phase == WallPhase.Closing ? (float)(s.ShortcutUntil(i)-s.TickIndex)/75 : 0;
                foreach (Transform wall in walls[i]) { Vector3 p = wall.position; p.y = WallHeight/2 - Mathf.Clamp01(fraction)*(WallHeight+.15f); wall.position = p; }
                shortcutSignals[i].color = phase == WallPhase.Open ? new Color(.25f,1,.65f) : new Color(1,.66f,.16f);
            }
            bool targeting = human.Life == CompetitionLife.Active && human.Inventory != CompetitionItem.None && human.Inventory != CompetitionItem.Boost && s.Phase == CompetitionPhase.Playing;
            aim.localScale=Vector3.one*(float)(s.SplashRadius*MazeMap.GridMeters);
            aim.gameObject.SetActive(targeting && human.Inventory == CompetitionItem.WaterBalloon); aimLine.gameObject.SetActive(targeting);
            if (targeting) { Vector3 target = World(s.Maze.Near(s.Aim(s.HumanId),centre),.07f); aim.position = target; aimLine.SetPosition(0,World(human.Position,.07f)); aimLine.SetPosition(1,target); }
            live.Clear();
            for (int i = 0; i < s.AttackCount; i++) {
                var a = s.Attack(i); live.Add(a.id);
                if (!effects.TryGetValue(a.id,out Effect e)) {
                    e = new Effect { root = new GameObject("Item " + a.id + " " + a.kind).transform }; e.root.SetParent(root);
                    Material material = a.kind == CompetitionItem.Banana ? banana : a.kind == CompetitionItem.Oil ? oil : a.kind == CompetitionItem.CreamPie ? cream : blue;
                    e.projectile = a.kind == CompetitionItem.Banana ? SceneMeshes.MeshObject("Banana peel",e.root,SceneMeshes.BananaPeel(),material,Vector3.zero,Vector3.one).transform : SceneMeshes.Primitive("Comic item",e.root,PrimitiveType.Sphere,material,Vector3.zero,a.kind == CompetitionItem.Oil ? new Vector3(1.4f,.08f,1.1f) : a.kind == CompetitionItem.CreamPie ? new Vector3(.6f,.22f,.6f) : Vector3.one*.5f);
                    e.warning = Ring("Warned splash",e.root,warning,(float)s.SplashRadius*(float)MazeMap.GridMeters,.12f); e.line = Line("Warned shot",e.root,warning,.12f); effects.Add(a.id,e);
                }
                Point2 position = a.Position(s.Maze,s.TickIndex); float h = a.kind == CompetitionItem.Oil ? .05f : a.kind == CompetitionItem.Banana && s.TickIndex >= a.landTick ? .08f : 1;
                if ((a.kind == CompetitionItem.WaterBalloon || a.kind == CompetitionItem.Banana) && s.TickIndex >= a.launchTick && s.TickIndex < a.landTick) h += Mathf.Sin((float)(s.TickIndex-a.launchTick)/(a.landTick-a.launchTick)*Mathf.PI)*1.5f;
                if(a.kind==CompetitionItem.WaterBalloon) {
                    bool landed=s.TickIndex>=a.landTick;float radius=(float)(s.SplashRadius*MazeMap.GridMeters);
                    e.projectile.localScale=landed?new Vector3(radius*2,.05f,radius*2):Vector3.one*.5f;
                    h=landed?.08f:h-Mathf.Clamp01((float)(s.TickIndex-a.launchTick)/Math.Max(1,a.landTick-a.launchTick));
                }
                e.projectile.position = World(s.Maze.Near(position,centre),h);
                e.warning.gameObject.SetActive(a.kind == CompetitionItem.WaterBalloon); e.warning.position = World(s.Maze.Near(a.target,centre),.08f);
                e.line.gameObject.SetActive(a.kind == CompetitionItem.CreamPie && s.TickIndex < a.launchTick);
                e.line.SetPosition(0,World(s.Maze.Near(a.origin,centre),.09f)); e.line.SetPosition(1,World(s.Maze.Near(a.target,centre),.09f));
            }
            removed.Clear(); foreach (var pair in effects) if (!live.Contains(pair.Key)) removed.Add(pair.Key);
            foreach (int id in removed) { UnityEngine.Object.Destroy(effects[id].root.gameObject); effects.Remove(id); }
        }
    }
}
