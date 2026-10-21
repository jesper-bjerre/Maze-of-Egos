using System;
using System.IO;
using MazeOfEgos.Core;
using Newtonsoft.Json;

internal static class SerializationChecks
{
    private static int Main()
    {
        var json = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore, MaxDepth = 64, ObjectCreationHandling = ObjectCreationHandling.Replace };
        var definition = JsonConvert.DeserializeObject<MazeDefinition>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "classic_reference.json")), json);
        var original = new GameSession(new MazeMap(definition), 123, countdown: false);
        for (int i = 0; i < 3000; i++) original.Tick();
        SessionSnapshot snapshot = original.Snapshot();
        string serialized = JsonConvert.SerializeObject(snapshot, json);
        var restored = new GameSession(new MazeMap(definition));
        restored.Restore(JsonConvert.DeserializeObject<SessionSnapshot>(serialized, json));
        if (restored.Phase != SessionPhase.Paused) throw new Exception("Restore must pause.");
        restored.Resume(0);
        if (JsonConvert.SerializeObject(restored.Snapshot(), json) != serialized) throw new Exception("Snapshot lost state using the actual Unity Newtonsoft serializer.");
        if (serialized.Contains("Normalized") || serialized.Contains("Position") || serialized.Contains("IsFinite"))
            throw new Exception("Derived geometry properties leaked into save data.");
        for (int i = 0; i < 120; i++) { original.Tick(); restored.Tick(); }
        if (JsonConvert.SerializeObject(restored.Snapshot(), json) != JsonConvert.SerializeObject(original.Snapshot(), json))
            throw new Exception("Restored simulation diverged.");
        // Exercise the serializer actually shipped in Unity with an in-flight banana.
        snapshot = original.Snapshot(); snapshot.phase = snapshot.resumePhase = SessionPhase.Playing;
        snapshot.player = new Point2(6, 5); snapshot.trail = new[] { snapshot.player }; snapshot.yaw = 90;
        snapshot.inventory = ItemKind.BananaPeel;
        foreach (PursuerState p in snapshot.pursuers) { p.life = PursuerLife.InHouse; p.from = p.to = new Cell(13, 14); p.progress = 0; p.releaseAt = snapshot.time + 100; }
        original.Restore(snapshot); original.Resume(0); original.UseItem(); original.Tick();
        serialized = JsonConvert.SerializeObject(original.Snapshot(), json);
        restored.Restore(JsonConvert.DeserializeObject<SessionSnapshot>(serialized, json)); restored.Resume(0);
        if (JsonConvert.SerializeObject(restored.Snapshot(), json) != serialized) throw new Exception("Airborne banana lost state.");
        for (int i = 0; i < 30; i++) { original.Tick(); restored.Tick(); }
        if (JsonConvert.SerializeObject(restored.Snapshot(), json) != JsonConvert.SerializeObject(original.Snapshot(), json))
            throw new Exception("Restored banana diverged at landing.");
        var legacy = Newtonsoft.Json.Linq.JObject.Parse(serialized); legacy["version"] = 1;
        foreach (var trap in legacy["traps"]) { ((Newtonsoft.Json.Linq.JObject)trap).Remove("launchPosition"); ((Newtonsoft.Json.Linq.JObject)trap).Remove("launchedAt"); ((Newtonsoft.Json.Linq.JObject)trap).Remove("landsAt"); }
        restored.Restore(legacy.ToObject<SessionSnapshot>());
        if (restored.Snapshot().version != 2 || restored.Traps[0].FlightProgress(restored.Time) != 1)
            throw new Exception("Version 1 save migration failed.");
        Console.WriteLine("PASS actual Unity Newtonsoft JSON roundtrip, paused restore, no derived-property recursion, deterministic simulation, airborne banana/landing and version 1 migration.");
        return 0;
    }
}
