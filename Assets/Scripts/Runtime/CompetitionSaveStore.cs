using System;
using System.IO;
using MazeOfEgos.Core;
using Newtonsoft.Json;

namespace MazeOfEgos.Runtime
{
    // Wired into the player flow in N4. Never reads, renames or deletes a classic save.
    public sealed class CompetitionSaveStore
    {
        private readonly string path;
        private readonly JsonSerializerSettings json = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.None, MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace, MaxDepth = 64
        };
        public CompetitionSaveStore(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Missing save directory.");
            path = Path.Combine(directory, CompetitionSnapshot.FileName);
        }
        public void Save(CompetitionSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            AtomicFile.Write(path, JsonConvert.SerializeObject(session.Snapshot(), json));
        }
        public bool TryLoad(CompetitionSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return AtomicFile.TryRead(path, text => {
                CompetitionSnapshot state;
                try { state = JsonConvert.DeserializeObject<CompetitionSnapshot>(text, json); }
                catch (JsonException error) { throw new ArgumentException("Invalid competition JSON.", error); }
                session.Restore(state); return state;
            }, out CompetitionSnapshot ignored);
        }
    }
}
