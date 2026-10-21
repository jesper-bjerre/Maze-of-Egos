using System;
using System.IO;
using System.Text;

namespace MazeOfEgos.Core
{
    public static class AtomicFile
    {
        public static void Write(string path, string text)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            Directory.CreateDirectory(directory);
            string temporary = path + ".tmp";
            byte[] bytes = new UTF8Encoding(false).GetBytes(text);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }
        public static bool TryRead<T>(string path, Func<string, T> deserialize, out T value) where T : class
        {
            foreach (string candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try { value = deserialize(File.ReadAllText(candidate)); if (value != null) return true; }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is FormatException || e is InvalidOperationException) { }
            }
            value = null; return false;
        }
    }
}
