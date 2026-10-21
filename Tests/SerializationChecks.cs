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
        Console.WriteLine("PASS actual Unity Newtonsoft JSON snapshot roundtrip, paused restore, no derived-property recursion, resumed deterministic simulation.");
        return 0;
    }
}
