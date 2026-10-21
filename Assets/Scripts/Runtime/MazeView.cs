using System;
using System.Collections.Generic;
using MazeOfEgos.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace MazeOfEgos.Runtime
{
    public sealed class MazeView
    {
        private readonly MazeMap maze;
        private readonly Transform root;
        private readonly Transform[] hearts, items;
        private readonly Transform[][] walls;
        private readonly Avatar[] avatars = new Avatar[4], tunnelAvatars = new Avatar[4];
        private readonly Transform[] trapRoots = new Transform[4];
        private readonly GameObject[] bananaViews = new GameObject[4], oilViews = new GameObject[4];
        private readonly Material[] shortcutSignals;
        private readonly Transform[] gates;
        private readonly Material banana, oil, frightened, returnMaterial;
        private readonly Mesh heartMesh;
        private readonly Font labelFont;
        private readonly Material labelMaterial;
        private readonly Material[] clothes = new Material[4];
        private readonly bool[] seen = new bool[4];
        private double nextScare;
        private readonly GameApp app;
        public readonly Camera Camera;
        private sealed class Avatar
        {
            public Transform root, body, head, leftArm, rightArm, leftLeg, rightLeg, bubble;
            public Renderer jacket;
            public TextMesh label;
        }
        public static Vector3 World(Point2 p, float height = 0) => new Vector3((float)(p.x * MazeMap.GridMeters), height, (float)(-p.y * MazeMap.GridMeters));

        public MazeView(GameApp app, MazeMap maze)
        {
            this.app = app; this.maze = maze;
            labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelMaterial = new Material(Resources.Load<Material>("Materials/WorldText"));
            labelMaterial.mainTexture = labelFont.material.mainTexture;
            root = new GameObject("Mechanical garden").transform;
            var cameraObject = new GameObject("First person camera", typeof(Camera), typeof(AudioListener));
            Camera = cameraObject.GetComponent<Camera>(); Camera.nearClipPlane = 0.06f; Camera.farClipPlane = 100;
            Camera.backgroundColor = new Color(0.58f, 0.73f, 0.76f); Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.transform.position = World(new Point2(maze.Definition.playerStart.x, maze.Definition.playerStart.y), 1.55f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.73f, 0.81f, 0.83f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.65f, 0.57f);
            RenderSettings.ambientGroundColor = new Color(0.33f, 0.35f, 0.29f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.59f, 0.70f, 0.66f); RenderSettings.fogDensity = 0.018f;
            Light sun = new GameObject("Warm afternoon", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional; sun.color = new Color(1, 0.86f, 0.65f); sun.intensity = 1.8f;
            sun.transform.rotation = Quaternion.Euler(48, -30, 0); sun.shadows = LightShadows.Soft;
            Material floor = SceneMeshes.Material("Warm limestone", new Color(0.66f, 0.60f, 0.43f), 0, 0.12f);
            Material brass = SceneMeshes.Material("Brushed brass", new Color(0.67f, 0.45f, 0.16f), 0.55f, 0.5f);
            Material[] wallMaterials = {
                SceneMeshes.Material("North sage", new Color(0.33f, 0.49f, 0.39f)),
                SceneMeshes.Material("East terracotta", new Color(0.66f, 0.39f, 0.29f)),
                SceneMeshes.Material("West blue wood", new Color(0.30f, 0.44f, 0.52f)),
                SceneMeshes.Material("South warm plaster", new Color(0.70f, 0.57f, 0.36f)) };
            var staticRoot = new GameObject("Static architecture").transform; staticRoot.SetParent(root);
            SceneMeshes.Primitive("Garden floor", staticRoot, PrimitiveType.Cube, floor,
                World(new Point2((maze.Definition.width - 1) / 2.0, (maze.Definition.height - 1) / 2.0), -0.15f),
                new Vector3(maze.Definition.width * 2.5f, 0.30f, maze.Definition.height * 2.5f));
            Mesh wallMesh = SceneMeshes.RoundedBox(new Vector3(3.2f, 4.5f, 3.2f), 0.10f);
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
                    walls[i][j] = SceneMeshes.MeshObject("Retracting wall " + i, root, wallMesh, brass, World(cell.Position, 2.25f), Vector3.one).transform;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector3 stripe = World(cell.Position, 0.015f) + approach * (side * 1.77f);
                        SceneMeshes.Primitive("Shortcut floor signal " + i, root, PrimitiveType.Cube, shortcutSignals[i], stripe,
                            acrossX ? new Vector3(0.14f, 0.025f, 1.75f) : new Vector3(1.75f, 0.025f, 0.14f));
                        SceneMeshes.Primitive("Shortcut wall signal " + i, walls[i][j], PrimitiveType.Sphere, shortcutSignals[i],
                            approach * (side * 1.62f) + Vector3.down * 0.65f, Vector3.one * 0.18f);
                    }
                }
            }
            for (int y = 0; y < maze.Definition.height; y++) for (int x = 0; x < maze.Definition.width; x++)
            {
                Cell cell = new Cell(x, y);
                if (maze.IsWalkable(cell, true) || movingCells.Contains(cell)) continue;
                int zone = (y < 15 ? 0 : 2) + (x < 14 ? 0 : 1);
                GameObject wall = SceneMeshes.MeshObject("Wall " + cell, staticRoot, wallMesh, wallMaterials[zone], World(cell.Position, 2.25f), Vector3.one);
                wall.isStatic = true;
            }
            // Matching tunnel extensions mask the logical wrap; character copies preserve visibility.
            for (int side = 0; side < 2; side++) for (int i = 1; i <= 6; i++)
            {
                int x = side == 0 ? -i : maze.Definition.width - 1 + i;
                foreach (int y in new[] { 13, 15 })
                    SceneMeshes.MeshObject("Tunnel wall", staticRoot, wallMesh, wallMaterials[y < 15 ? 0 : 2], World(new Point2(x, y), 2.25f), Vector3.one).isStatic = true;
                SceneMeshes.Primitive("Tunnel floor", staticRoot, PrimitiveType.Cube, floor, World(new Point2(x, 14), -0.15f), new Vector3(2.5f, 0.3f, 2.5f));
                SceneMeshes.Primitive("Tunnel roof", staticRoot, PrimitiveType.Cube, wallMaterials[0], World(new Point2(x, 14), 4.6f), new Vector3(2.5f, 0.2f, 2.5f));
            }
            gates = new Transform[maze.Definition.ghostDoorCells.Length];
            for (int door = 0; door < gates.Length; door++)
            {
                Cell c = MazeMap.FromPair(maze.Definition.ghostDoorCells[door]);
                SceneMeshes.Primitive("Base threshold", staticRoot, PrimitiveType.Cube, brass, World(c.Position, 0.04f), new Vector3(2.5f, 0.08f, 0.12f));
                gates[door] = new GameObject("Pursuer gate " + door).transform; gates[door].SetParent(root, false);
                gates[door].position = World(c.Position) + Vector3.left * 1.25f;
                SceneMeshes.Primitive("Gate upper bar", gates[door], PrimitiveType.Cube, brass, new Vector3(1.25f, 1.4f, 0), new Vector3(2.35f, 0.10f, 0.12f));
                SceneMeshes.Primitive("Gate lower bar", gates[door], PrimitiveType.Cube, brass, new Vector3(1.25f, 0.65f, 0), new Vector3(2.35f, 0.10f, 0.12f));
                for (int bar = 0; bar < 5; bar++)
                    SceneMeshes.Primitive("Gate spindle", gates[door], PrimitiveType.Cube, brass, new Vector3(0.3f + bar * 0.44f, 1.03f, 0), new Vector3(0.055f, 1.35f, 0.055f));
            }
            Point2 sign = MazeMap.FromPair(maze.Definition.ghostDoorCells[0]).Position;
            sign.x += 0.5; sign.y -= 0.1;
            Material signBacking = SceneMeshes.Material("Base sign enamel", new Color(0.06f, 0.13f, 0.11f), 0.15f, 0.35f);
            SceneMeshes.Primitive("Base sign", staticRoot, PrimitiveType.Cube, signBacking, World(sign, 2.3f), new Vector3(3.2f, 0.48f, 0.10f));
            TextMesh gateSign = Label("KUN STORE EGOER", root, World(sign, 2.3f) + Vector3.forward * 0.065f, 0.025f, new Color(1, 0.88f, 0.54f));
            gateSign.transform.rotation = Quaternion.Euler(0, 180, 0);
            StaticBatchingUtility.Combine(staticRoot.gameObject);
            heartMesh = SceneMeshes.Heart(); hearts = new Transform[maze.Hearts.Count];
            Material small = SceneMeshes.Material("Pink heart", new Color(0.96f, 0.16f, 0.30f), 0.05f, 0.7f, true);
            Material power = SceneMeshes.Material("Golden power heart", new Color(1, 0.69f, 0.13f), 0.35f, 0.7f, true);
            for (int i = 0; i < hearts.Length; i++)
            {
                bool big = maze.PowerHearts[i];
                hearts[i] = SceneMeshes.MeshObject(big ? "Power heart" : "Heart", root, heartMesh, big ? power : small,
                    World(maze.Hearts[i].Position, big ? 1.05f : 1), Vector3.one * (big ? 0.65f : 0.32f)).transform;
            }
            items = new Transform[maze.Definition.itemCells.Length];
            Material question = SceneMeshes.Material("Question token", new Color(0.23f, 0.51f, 0.62f), 0.3f, 0.6f);
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = SceneMeshes.Primitive("Question mark", root, PrimitiveType.Cube, question, World(MazeMap.FromPair(maze.Definition.itemCells[i]).Position, 1.65f), Vector3.one * 0.45f);
                TextMesh text = Label("?", items[i], new Vector3(0, 0, -0.53f), 0.18f, Color.white);
                text.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            banana = SceneMeshes.Material("Banana", new Color(1, 0.8f, 0.12f), 0, 0.5f);
            banana.SetFloat("_Cull", 0);
            oil = SceneMeshes.Material("Cartoon oil", new Color(0.14f, 0.09f, 0.24f), 0.35f, 0.95f);
            frightened = SceneMeshes.Material("Power surprise", new Color(0.40f, 0.57f, 0.90f), 0, 0.45f);
            returnMaterial = SceneMeshes.Material("Pause bubble", new Color(0.66f, 0.87f, 0.96f), 0.15f, 0.9f);
            Color[] suitColors = { new Color(0.15f, 0.19f, 0.24f), new Color(0.16f, 0.20f, 0.29f), new Color(0.14f, 0.16f, 0.19f), new Color(0.13f, 0.23f, 0.40f) };
            Mesh peelMesh = SceneMeshes.BananaPeel();
            for (int i = 0; i < 4; i++)
            {
                clothes[i] = SceneMeshes.Material("Suit " + i, suitColors[i]);
                avatars[i] = BuildAvatar(i); tunnelAvatars[i] = BuildAvatar(i); tunnelAvatars[i].root.gameObject.SetActive(false);
                trapRoots[i] = new GameObject("Reusable trap " + i).transform; trapRoots[i].SetParent(root, false);
                bananaViews[i] = SceneMeshes.MeshObject("Banana peel", trapRoots[i], peelMesh, banana, Vector3.zero, Vector3.one);
                oilViews[i] = new GameObject("Oil slick"); oilViews[i].transform.SetParent(trapRoots[i], false);
                for (int blob = 0; blob < 3; blob++)
                    SceneMeshes.Primitive("Oil pool", oilViews[i].transform, PrimitiveType.Sphere, oil,
                        new Vector3((blob - 1) * 0.20f, 0.025f, blob == 1 ? 0.10f : -0.08f), new Vector3(0.62f, 0.045f, 0.54f));
                trapRoots[i].gameObject.SetActive(false);
            }
        }

        private Avatar BuildAvatar(int index)
        {
            string[] names = { "Putin", "Xi", "Kim", "Trump" };
            Transform avatarRoot = new GameObject(names[index] + " temporary full-body model").transform; avatarRoot.SetParent(root);
            var a = new Avatar { root = avatarRoot };
            Material skin = SceneMeshes.Material("Skin " + index, index == 3 ? new Color(0.91f, 0.58f, 0.35f) : new Color(0.88f, 0.69f, 0.53f), 0, 0.35f);
            Material hair = SceneMeshes.Material("Hair " + index, index == 3 ? new Color(0.94f, 0.67f, 0.22f) : index == 0 ? new Color(0.35f, 0.29f, 0.22f) : new Color(0.075f, 0.065f, 0.06f));
            Material black = SceneMeshes.Material("Eyes/shoes " + index, new Color(0.055f, 0.05f, 0.04f));
            Material white = SceneMeshes.Material("Shirt/eyes " + index, new Color(0.96f, 0.93f, 0.85f));
            Material tie = SceneMeshes.Material("Tie " + index, index == 3 ? new Color(0.8f, 0.08f, 0.08f) : new Color(0.35f, 0.13f, 0.2f));
            float width = index >= 2 ? 0.75f : 0.60f;
            a.body = SceneMeshes.Primitive("Jacket", avatarRoot, PrimitiveType.Capsule, clothes[index], new Vector3(0, 0.98f, 0), new Vector3(width, 0.48f, 0.48f));
            a.jacket = a.body.GetComponent<Renderer>();
            SceneMeshes.Primitive("Shirt", avatarRoot, PrimitiveType.Cube, white, new Vector3(0, 1.17f, 0.25f), new Vector3(0.23f, 0.31f, 0.035f));
            if (index != 2) SceneMeshes.Primitive("Tie", avatarRoot, PrimitiveType.Cube, tie, new Vector3(0, 1.10f, 0.28f), new Vector3(0.09f, index == 3 ? 0.5f : 0.27f, 0.04f));
            a.head = SceneMeshes.Primitive("Head", avatarRoot, PrimitiveType.Sphere, skin, new Vector3(0, 1.62f, 0.01f), new Vector3(0.62f, 0.67f, 0.57f));
            Transform hairPiece = SceneMeshes.Primitive("Recognizable hair silhouette", avatarRoot, index == 2 ? PrimitiveType.Cube : PrimitiveType.Sphere,
                hair, new Vector3(index == 3 ? -0.06f : 0, index == 0 ? 1.84f : 1.90f, -0.04f),
                new Vector3(index == 3 ? 0.72f : 0.56f, index == 0 ? 0.12f : index == 2 ? 0.22f : 0.27f, 0.50f));
            if (index == 3) hairPiece.localRotation = Quaternion.Euler(0, 0, -12);
            for (int side = -1; side <= 1; side += 2)
            {
                SceneMeshes.Primitive("Eye white", avatarRoot, PrimitiveType.Sphere, white, new Vector3(side * 0.115f, 1.67f, 0.263f), new Vector3(0.13f, 0.11f, 0.055f));
                SceneMeshes.Primitive("Eye", avatarRoot, PrimitiveType.Sphere, black, new Vector3(side * 0.115f, 1.67f, 0.29f), new Vector3(0.055f, 0.067f, 0.025f));
                Transform brow = SceneMeshes.Primitive("Eyebrow", avatarRoot, PrimitiveType.Cube, hair, new Vector3(side * 0.12f, 1.76f, 0.26f), new Vector3(0.15f, 0.035f, 0.04f));
                brow.localRotation = Quaternion.Euler(0, 0, side * 15);
            }
            SceneMeshes.Primitive("Nose", avatarRoot, PrimitiveType.Sphere, skin, new Vector3(0, 1.60f, 0.30f), new Vector3(0.12f, 0.14f, 0.14f));
            SceneMeshes.Primitive("Mouth", avatarRoot, PrimitiveType.Cube, black, new Vector3(0, 1.48f, 0.26f), new Vector3(0.18f, 0.025f, 0.025f));
            a.leftArm = Limb("Left arm", avatarRoot, clothes[index], new Vector3(-width * 0.58f, 1.22f, 0), new Vector3(0.15f, 0.43f, 0.17f), skin);
            a.rightArm = Limb("Right arm", avatarRoot, clothes[index], new Vector3(width * 0.58f, 1.22f, 0), new Vector3(0.15f, 0.43f, 0.17f), skin);
            a.leftLeg = Limb("Left leg", avatarRoot, clothes[index], new Vector3(-0.17f, 0.62f, 0), new Vector3(0.18f, 0.46f, 0.2f), black);
            a.rightLeg = Limb("Right leg", avatarRoot, clothes[index], new Vector3(0.17f, 0.62f, 0), new Vector3(0.18f, 0.46f, 0.2f), black);
            a.label = Label(names[index], avatarRoot, new Vector3(0, 2.22f, 0), 0.065f, new Color(1, 0.92f, 0.65f));
            a.bubble = SceneMeshes.Primitive("Return bubble", avatarRoot, PrimitiveType.Sphere, returnMaterial, new Vector3(0, 0.55f, 0), Vector3.one * 0.85f);
            a.bubble.gameObject.SetActive(false); return a;
        }
        private static Transform Limb(string name, Transform parent, Material clothing, Vector3 pivot, Vector3 size, Material tip)
        {
            Transform root = new GameObject(name + " pivot").transform; root.SetParent(parent, false); root.localPosition = pivot;
            SceneMeshes.Primitive(name, root, PrimitiveType.Capsule, clothing, new Vector3(0, -size.y * 0.48f, 0), new Vector3(size.x, size.y * 0.5f, size.z));
            SceneMeshes.Primitive("Hand/shoe", root, PrimitiveType.Sphere, tip, new Vector3(0, -size.y, 0.03f), new Vector3(size.x * 1.2f, size.x, size.z * 1.35f));
            return root;
        }
        private TextMesh Label(string text, Transform parent, Vector3 position, float size, Color color)
        {
            var go = new GameObject(text, typeof(TextMesh)); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            TextMesh label = go.GetComponent<TextMesh>(); label.text = text; label.characterSize = size; label.fontSize = 48;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.color = color;
            label.font = labelFont; label.GetComponent<Renderer>().sharedMaterial = labelMaterial;
            return label;
        }
        public void ResetEncounters() { Array.Clear(seen, 0, seen.Length); nextScare = 0; }

        public void Render(GameSession session, Point2 previousPlayer, Point2[] previousGhosts, float alpha)
        {
            // Dynamic font atlases may be replaced when UI glyphs are requested.
            labelMaterial.mainTexture = labelFont.material.mainTexture;
            Point2 end = maze.Near(session.Player, previousPlayer);
            Point2 cameraPosition = (end - previousPlayer).Length > 2 ? session.Player : Point2.Lerp(previousPlayer, end, alpha);
            Camera.transform.position = World(cameraPosition, 1.55f);
            Camera.transform.rotation = Quaternion.Euler((float)-session.Pitch, (float)session.Yaw, 0);
            Camera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(app.Settings.fov, Camera.aspect);
            float time = (float)session.Time;
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].gameObject.SetActive(!session.Collected[i]);
                if (session.Collected[i]) continue;
                hearts[i].rotation = Quaternion.Euler(0, time * 40 + i * 21, 0);
                hearts[i].position = World(maze.Hearts[i].Position, 1 + Mathf.Sin(time * 2 + i) * 0.055f);
            }
            for (int i = 0; i < items.Length; i++)
            {
                items[i].gameObject.SetActive(session.Rules.itemsEnabled && session.ItemAvailable[i]);
                items[i].rotation = Quaternion.Euler(0, time * 50 + i * 30, 0);
            }
            for (int i = 0; i < walls.Length; i++)
            {
                WallState wall = session.Walls[i]; float fraction = wall.phase == WallPhase.Open ? 1 :
                    wall.phase == WallPhase.Opening ? 1 - Mathf.Clamp01((float)(wall.phaseUntil - session.Time) / 1.25f) :
                    wall.phase == WallPhase.Closing ? Mathf.Clamp01((float)(wall.phaseUntil - session.Time) / 1.25f) : 0;
                fraction = Mathf.SmoothStep(0, 1, fraction);
                foreach (Transform piece in walls[i])
                { Vector3 position = piece.position; position.y = 2.25f - fraction * 4.8f; piece.position = position; }
                Color signal = wall.phase == WallPhase.Open ? new Color(0.28f, 0.82f, 0.59f) :
                    wall.phase == WallPhase.Closed ? new Color(0.36f, 0.26f, 0.12f) : new Color(1, 0.66f, 0.16f);
                float pulse = wall.phase == WallPhase.Warning && !app.Settings.reducedMotion ? 0.65f + 0.35f * Mathf.Sin(time * 8) : 1;
                shortcutSignals[i].color = signal;
                shortcutSignals[i].SetColor("_EmissionColor", signal * (wall.phase == WallPhase.Closed ? 0.1f : pulse));
            }
            for (int door = 0; door < gates.Length; door++)
            {
                Point2 cell = MazeMap.FromPair(maze.Definition.ghostDoorCells[door]).Position; bool open = false;
                foreach (PursuerState pursuer in session.Pursuers)
                    if ((pursuer.life == PursuerLife.Exiting || pursuer.life == PursuerLife.Returning) &&
                        (maze.Near(pursuer.Position(maze), cell) - cell).Length < 1.8) { open = true; break; }
                float angle = Mathf.MoveTowardsAngle(gates[door].localEulerAngles.y, open ? 90 : 0,
                    session.Phase == SessionPhase.Playing ? UnityEngine.Time.deltaTime * 220 : 0);
                gates[door].localRotation = Quaternion.Euler(0, angle, 0);
            }
            for (int i = 0; i < 4; i++)
            {
                PursuerState p = session.Pursuers[i]; Point2 current = p.Position(maze), previous = previousGhosts[i];
                Point2 point = (maze.Near(current, previous) - previous).Length > 2 ? current : Point2.Lerp(previous, maze.Near(current, previous), alpha);
                RenderAvatar(avatars[i], session, i, point, time);
                bool tunnel = Math.Abs(current.y - maze.Definition.horizontalTunnelRow) < 0.3 && (current.x < 4 || current.x > maze.Definition.width - 5);
                tunnelAvatars[i].root.gameObject.SetActive(tunnel);
                if (tunnel)
                {
                    point.x += current.x < 4 ? maze.Definition.width : -maze.Definition.width;
                    RenderAvatar(tunnelAvatars[i], session, i, point, time);
                }
                Point2 relative = maze.Near(current, session.Player) - session.Player;
                bool visible = session.IsDangerous(p) && relative.Length < 1.8 &&
                    Point2.Dot(relative.Normalized, Point2.Forward(session.Yaw)) > 0.35 && maze.HasLineOfSight(session.Player, current);
                if (session.Phase == SessionPhase.Playing && visible && !seen[i] && session.Time >= nextScare)
                {
                    nextScare = session.Time + 8;
                    app.Encounter(i);
                }
                seen[i] = visible;
            }
            for (int i = 0; i < trapRoots.Length; i++)
            {
                bool active = i < session.Traps.Count; trapRoots[i].gameObject.SetActive(active); if (!active) continue;
                TrapState trap = session.Traps[i]; trapRoots[i].position = World(trap.position);
                bool isBanana = trap.kind == ItemKind.BananaPeel;
                bananaViews[i].SetActive(isBanana); oilViews[i].SetActive(!isBanana);
            }
        }
        private void RenderAvatar(Avatar avatar, GameSession session, int index, Point2 position, float time)
        {
            PursuerState state = session.Pursuers[index]; bool returning = state.life == PursuerLife.Returning;
            avatar.root.position = World(position, returning ? 0.3f : 0);
            avatar.root.localScale = Vector3.one * (returning ? 0.38f : 1);
            Point2 direction = maze.Near(state.to.Position, state.from.Position) - state.from.Position;
            if (direction.LengthSquared > 0) avatar.root.rotation = Quaternion.Euler(0, (float)(Math.Atan2(direction.x, -direction.y) * 180 / Math.PI), 0);
            float walk = (float)session.PursuerSpeed(state) > 0 ? Mathf.Sin(time * 12 + index) * 28 : 0;
            bool slip = session.Time < state.stunUntil || session.Time < state.oilUntil;
            avatar.leftLeg.localRotation = Quaternion.Euler(walk, 0, 0); avatar.rightLeg.localRotation = Quaternion.Euler(-walk, 0, 0);
            avatar.leftArm.localRotation = Quaternion.Euler(-walk, 0, slip ? -35 : 7); avatar.rightArm.localRotation = Quaternion.Euler(walk, 0, slip ? 35 : -7);
            avatar.head.localRotation = Quaternion.Euler(0, 0, slip ? Mathf.Sin(time * 18) * 18 : 0);
            bool vulnerable = session.IsFrightened(state);
            avatar.jacket.sharedMaterial = vulnerable ? frightened : clothes[index];
            avatar.label.text = vulnerable ? "!" : returning ? "Til pause" : state.id == "putin" ? "Putin" : state.id == "xi" ? "Xi" : state.id == "kim" ? "Kim" : "Trump";
            avatar.label.transform.rotation = Camera.transform.rotation;
            avatar.label.gameObject.SetActive((Camera.transform.position - avatar.label.transform.position).sqrMagnitude > 9);
            avatar.bubble.gameObject.SetActive(returning);
        }
    }
}
