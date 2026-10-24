using System;
using System.IO;
using MazeOfEgos.Core;
using MazeOfEgos.Runtime;
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
        snapshot = original.Snapshot(); snapshot.player = new Point2(10.25, 5); snapshot.yaw = 90;
        snapshot.autoMoving = true; snapshot.queuedHeading = 0; snapshot.trail = new[] { snapshot.player };
        original.Restore(snapshot); original.Resume(0);
        serialized = JsonConvert.SerializeObject(original.Snapshot(), json);
        restored.Restore(JsonConvert.DeserializeObject<SessionSnapshot>(serialized, json)); restored.Resume(0);
        if (restored.QueuedHeading != 0 || !restored.AutoMoving) throw new Exception("Queued turn lost in Unity JSON.");
        for (int i = 0; i < 90; i++) { original.Tick(); restored.Tick(); }
        if (restored.Yaw != 0 || restored.Player.x != 12 || restored.Player.y >= 5 ||
            JsonConvert.SerializeObject(restored.Snapshot(), json) != JsonConvert.SerializeObject(original.Snapshot(), json))
            throw new Exception("Restored automatic turn diverged.");
        var legacy = Newtonsoft.Json.Linq.JObject.Parse(serialized); legacy["version"] = 1;
        foreach (var trap in legacy["traps"]) { ((Newtonsoft.Json.Linq.JObject)trap).Remove("launchPosition"); ((Newtonsoft.Json.Linq.JObject)trap).Remove("launchedAt"); ((Newtonsoft.Json.Linq.JObject)trap).Remove("landsAt"); }
        restored.Restore(legacy.ToObject<SessionSnapshot>());
        if (restored.Snapshot().version != 3 || restored.Traps[0].FlightProgress(restored.Time) != 1)
            throw new Exception("Version 1 save migration failed.");
        Console.WriteLine("PASS actual Unity Newtonsoft JSON roundtrip, paused restore, queued automatic turn, no derived-property recursion, deterministic simulation, airborne banana/landing and version 1 migration.");
        CheckCompetition(definition, json);
        var arena = new CompetitionSession(definition, "trump", 71, countdown: false, computerPlayers: true);
        for(int i=0;i<1200;i++){arena.DriveHumanForAutomation();arena.Tick();arena.Events.Clear();}
        string arenaJson=JsonConvert.SerializeObject(arena.Snapshot(),json);
        var arenaCopy=new CompetitionSession(definition);arenaCopy.Restore(JsonConvert.DeserializeObject<CompetitionSnapshot>(arenaJson,json));arenaCopy.Resume(false);
        for(int i=0;i<1200;i++){arena.DriveHumanForAutomation();arenaCopy.DriveHumanForAutomation();arena.Tick();arenaCopy.Tick();arena.Events.Clear();arenaCopy.Events.Clear();}
        if(JsonConvert.SerializeObject(arena.Snapshot(),json)!=JsonConvert.SerializeObject(arenaCopy.Snapshot(),json))throw new Exception("Actual Unity JSON lost competition combat/AI memory.");
        var v3=Newtonsoft.Json.Linq.JObject.Parse(arenaJson);v3["version"]=3;v3["rules"]["id"]="competition-v3";v3.Remove("roundStartedTick");v3.Remove("roundEnd");((Newtonsoft.Json.Linq.JObject)v3["rules"]).Remove("endWhenHeartsCollected");
        arenaCopy.Restore(v3.ToObject<CompetitionSnapshot>());var migratedV3=arenaCopy.Snapshot();
        if(migratedV3.version!=4||migratedV3.roundStartedTick!=0||!migratedV3.rules.endWhenHeartsCollected||migratedV3.tick!=(long)v3["tick"])
            throw new Exception("Actual build 16 JSON migration lost round timing.");
        var v2 = Newtonsoft.Json.Linq.JObject.Parse(arenaJson);v2["version"]=2;v2["rules"]["id"]="competition-v2";
        v2.Remove("spawnRandom");((Newtonsoft.Json.Linq.JObject)v2["rules"]).Remove("randomRoundStarts");((Newtonsoft.Json.Linq.JObject)v2["rules"]).Remove("replenishHearts");((Newtonsoft.Json.Linq.JObject)v2["rules"]).Remove("sharedMinimap");
        arenaCopy.Restore(v2.ToObject<CompetitionSnapshot>());var upgraded=arenaCopy.Snapshot();
        if(upgraded.version!=4||upgraded.spawnRandom==0||!upgraded.rules.randomRoundStarts||upgraded.rules.replenishHearts||upgraded.tick!=(long)v2["tick"]||upgraded.hearts.Length!=((Newtonsoft.Json.Linq.JArray)v2["hearts"]).Count)
            throw new Exception("Actual build 15 JSON migration lost state or new rules.");
        var oldArena=arena.Snapshot();oldArena.version=1;oldArena.rules.id="competition-n2-v1";oldArena.attacks=null;oldArena.brains=null;oldArena.statistics=null;
        arenaCopy.Restore(JsonConvert.DeserializeObject<CompetitionSnapshot>(JsonConvert.SerializeObject(oldArena,json),json));
        if(arenaCopy.Snapshot().version!=4)throw new Exception("Actual Unity JSON migration failed.");
        Console.WriteLine("PASS competition-v4 combat and production shared-map/local-combat AI deterministic continuation with actual Unity Newtonsoft; v1 migration without legacy writes.");
        return 0;
    }
    private static void CheckCompetition(MazeDefinition definition, JsonSerializerSettings json)
    {
        var original = new CompetitionSession(definition, "xi", 123, new CompetitionRules { movingWalls = false }, false);
        var fixture = original.Snapshot();
        foreach (var p in fixture.participants) p.moving = false;
        fixture.participants[0].position = new Point2(5.79, 5); fixture.participants[0].heading = 1; fixture.participants[0].moving = true;
        original.Restore(fixture); original.Resume(false); for (int i = 0; i < 10; i++) original.Tick();
        fixture = original.Snapshot(); fixture.participants[0].moving = false; original.Restore(fixture); original.Resume(false);
        string text = JsonConvert.SerializeObject(original.Snapshot(), json);
        if (text.Contains("Normalized") || text.Contains("IsFinite")) throw new Exception("Competition JSON recursed into derived geometry.");
        var restored = new CompetitionSession(definition);
        restored.Restore(JsonConvert.DeserializeObject<CompetitionSnapshot>(text, json));
        if (restored.Phase != CompetitionPhase.Paused) throw new Exception("Competition restore did not pause.");
        restored.Resume(false);
        if (JsonConvert.SerializeObject(restored.Snapshot(), json) != text) throw new Exception("Competition JSON lost state.");
        for (int i = 0; i < 700; i++) { original.Tick(); restored.Tick(); }
        if (JsonConvert.SerializeObject(original.Snapshot(), json) != JsonConvert.SerializeObject(restored.Snapshot(), json))
            throw new Exception("Competition diverged through replenishment/cooldown.");
        var missingVersion = Newtonsoft.Json.Linq.JObject.Parse(text); missingVersion.Remove("version");
        try { restored.Restore(missingVersion.ToObject<CompetitionSnapshot>()); throw new Exception("Unversioned competition file accepted."); }
        catch (ArgumentException) { }
        try { restored.Restore(JsonConvert.DeserializeObject<CompetitionSnapshot>(JsonConvert.SerializeObject(new GameSession(new MazeMap(definition)).Snapshot(), json), json));
            throw new Exception("Classic snapshot accepted as a competition."); }
        catch (ArgumentException) { }

        string directory = Path.Combine(Path.GetTempPath(), "maze-competition-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string legacyPath = Path.Combine(directory, "session-v1.json"), settingsPath = Path.Combine(directory, "settings.json");
            File.WriteAllText(legacyPath, "existing classic save"); File.WriteAllText(settingsPath, "existing preferences/highscore");
            var store = new CompetitionSaveStore(directory); string backup = JsonConvert.SerializeObject(original.Snapshot(), json);
            store.Save(original); original.Tick(); store.Save(original);
            File.WriteAllText(Path.Combine(directory, CompetitionSnapshot.FileName), "{ truncated JSON");
            if (!store.TryLoad(restored) || restored.Phase != CompetitionPhase.Paused) throw new Exception("Competition backup was not recovered.");
            restored.Resume(false);
            if (JsonConvert.SerializeObject(restored.Snapshot(), json) != backup) throw new Exception("Wrong competition backup recovered.");
            if (File.ReadAllText(legacyPath) != "existing classic save" || File.ReadAllText(settingsPath) != "existing preferences/highscore")
                throw new Exception("Competition storage touched classic data.");
            fixture = restored.Snapshot(); fixture.tick = fixture.roundElapsedTicks = 7199;
            fixture.nextHeartWaveTick = 7200; fixture.nextWallAttemptTick = 8099;
            fixture.hearts = Array.Empty<CompetitionHeart>(); fixture.participants[0].roundScore = fixture.participants[1].roundScore = 10;
            restored.Restore(fixture); restored.Resume(false); restored.Tick(); store.Save(restored);
            string result = JsonConvert.SerializeObject(restored.Snapshot(), json);
            if (!store.TryLoad(original)) throw new Exception("Round result failed to load.");
            original.Resume(false); for (int i = 0; i < 30; i++) original.Tick();
            if (JsonConvert.SerializeObject(original.Snapshot(), json) != result || original.Participants[0].bankedScore != 10 || original.Participants[0].trophies != 1)
                throw new Exception("Round points or trophies awarded twice after file reload.");
            File.WriteAllText(Path.Combine(directory, CompetitionSnapshot.FileName), "null");
            File.WriteAllText(Path.Combine(directory, CompetitionSnapshot.FileName + ".bak"), "{}");
            if (store.TryLoad(original) || JsonConvert.SerializeObject(original.Snapshot(), json) != result)
                throw new Exception("Bad competition files mutated the running match.");
        }
        finally { Directory.Delete(directory, true); }
        Console.WriteLine("PASS N2 actual Unity Newtonsoft competition roundtrip/determinism, version/legacy rejection, atomic backup recovery, old-file preservation and exactly-once round result after disk reload.");
    }
}
