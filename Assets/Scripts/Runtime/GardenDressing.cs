using MazeOfEgos.Core;
using UnityEngine;

namespace MazeOfEgos.Runtime
{
    public static class GardenDressing
    {
        // Low trim preserves the original footprints and reveals neighbouring
        // corridors. Moving shortcuts must not retain static foliage.
        public static void Build(MazeMap maze, Transform architecture)
        {
            Material stone = SceneMeshes.Material("Garden dressed stone", new Color(.44f, .46f, .37f), 0, .18f);
            Material wood = SceneMeshes.Material("Painted garden timber", new Color(.32f, .43f, .32f), 0, .25f);
            Material leaves = SceneMeshes.Material("Garden rosemary", new Color(.19f, .36f, .21f), 0, .12f);
            Material flower = SceneMeshes.Material("Garden cream flowers", new Color(.87f, .86f, .73f), 0, .15f);
            Material brass = SceneMeshes.Material("Garden ornament brass", new Color(.68f, .48f, .22f), .5f, .45f);
            for (int y = 0; y < maze.Definition.height; y++) for (int x = 0; x < maze.Definition.width; x++)
            {
                Cell cell = new Cell(x, y);
                bool moving = false;
                foreach (ShortcutDefinition shortcut in maze.Definition.shortcutCandidates)
                    foreach (int[] pair in shortcut.wallCells) if (MazeMap.FromPair(pair) == cell) moving = true;
                if (maze.IsWalkable(cell, true) || moving) continue;
                bool exposed = maze.IsWalkable(new Cell(x - 1, y), true) || maze.IsWalkable(new Cell(x + 1, y), true) ||
                    maze.IsWalkable(new Cell(x, y - 1), true) || maze.IsWalkable(new Cell(x, y + 1), true);
                if (!exposed) continue;
                Vector3 position = MazeView.World(cell.Position);
                SceneMeshes.Primitive("Stone garden plinth", architecture, PrimitiveType.Cube, stone, position + Vector3.up * .18f, new Vector3(2.56f, .35f, 2.56f));
                SceneMeshes.Primitive("Timber wall coping", architecture, PrimitiveType.Cube, wood, position + Vector3.up * (MazeView.WallHeight + .04f), new Vector3(2.48f, .08f, 2.48f));
                if ((x + y * 3) % 5 != 0) continue;
                // Rounded, low-poly shrubs sit on the wall, outside corridor sight lines.
                SceneMeshes.Primitive("Rosemary crown", architecture, PrimitiveType.Sphere, leaves, position + Vector3.up * (MazeView.WallHeight + .12f), new Vector3(2.5f, .35f, 2.1f));
                for (int i = 0; i < 3; i++)
                    SceneMeshes.Primitive("Coral blossom", architecture, PrimitiveType.Sphere, flower,
                        position + new Vector3((i - 1) * .65f, MazeView.WallHeight + .30f, i % 2 * .4f), Vector3.one * .14f);
                for (int i = 0; i < 2; i++)
                {
                    float height = .45f + i * .32f;
                    SceneMeshes.Primitive("Inset vine leaf", architecture, PrimitiveType.Sphere, leaves,
                        position + new Vector3(Mathf.Sin(i * 2.1f) * .24f, height, 1.56f), new Vector3(.22f, .35f, .07f));
                }
            }
            // Different silhouettes mark the four corners even above tall garden walls.
            int[,] corners = { { 0, 0 }, { maze.Definition.width - 1, 0 }, { 0, maze.Definition.height - 1 }, { maze.Definition.width - 1, maze.Definition.height - 1 } };
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = MazeView.World(new Point2(corners[i, 0], corners[i, 1]), .7f);
                SceneMeshes.Primitive("Corner garden pedestal", architecture, PrimitiveType.Cube, stone, p, new Vector3(1, 1.2f + i * .15f, 1));
                SceneMeshes.Primitive("Corner " + i + " topiary", architecture, i % 2 == 0 ? PrimitiveType.Sphere : PrimitiveType.Capsule,
                    leaves, p + Vector3.up * 1.1f, new Vector3(1.8f, i % 2 == 0 ? 1.8f : 1.5f, 1.8f));
                SceneMeshes.Primitive("Landmark finial", architecture, PrimitiveType.Sphere, brass, p + Vector3.up * 2.1f, Vector3.one * .25f);
            }
            // Harmless prestige clutter in the pursuers' enclosed base.
            foreach (int[] pair in maze.Definition.ghostDoorCells)
            {
                Vector3 p = MazeView.World(MazeMap.FromPair(pair).Position) + new Vector3(0, 1.8f, 0);
                SceneMeshes.Primitive("Base prestige shelf", architecture, PrimitiveType.Cube, wood, p, new Vector3(2.3f, .10f, .30f));
                for (int i = 0; i < 3; i++)
                {
                    Vector3 trophy = p + new Vector3((i - 1) * .62f, .22f, 0);
                    SceneMeshes.Primitive("Tiny ego trophy", architecture, PrimitiveType.Cylinder, brass, trophy, new Vector3(.12f, .16f, .12f));
                    SceneMeshes.Primitive("Trophy cup", architecture, PrimitiveType.Sphere, brass, trophy + Vector3.up * .18f, new Vector3(.30f, .25f, .30f));
                }
            }
        }
    }
}
