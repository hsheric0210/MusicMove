using Serilog;

namespace MusicMove
{
    public class Music
    {
        private string filePath;

        public Music(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is null or blank", nameof(filePath));
            this.filePath = Path.GetFullPath(filePath);
            Log.Verbose("Music file accepted: {path} -> {fullPath}", filePath, this.filePath);
        }

        public sealed record FileNameInfo(string[] Artists, string SongName, string[] Featuring)
        {
            public bool Equals(FileNameInfo? other) => other != null && Artists.SequenceEqual(other.Artists) && SongName.Equals(other.SongName);

            public override int GetHashCode() => throw new NotImplementedException();
        }

        public static FileNameInfo ParseFileName(string fileName)
        {
            var separated = fileName.Split(" - ", 2, StringSplitOptions.RemoveEmptyEntries);
            if (separated.Length == 1)
                throw new FormatException("Artist-SongName separator not found: " + fileName);

            var artists = separated[0].Split(new string[] { ", ", " & " }, StringSplitOptions.RemoveEmptyEntries);
            Log.Verbose("Artists: {artists}", string.Join("; ", artists));

            var namePart = separated[1];
            Log.Verbose("Name: {name}", namePart);

            var featuring = ParseFeaturing(namePart, "(feat.").Concat(ParseFeaturing(namePart, "(ft."));
            Log.Verbose("Featuring: {featuring}", string.Join("; ", featuring));

            return new FileNameInfo(artists, namePart, Array.Empty<string>());
        }

        public static string[] ParseFeaturing(string namePart, string prefix)
        {
            var startIndex = namePart.IndexOf(prefix);
            if (startIndex == -1)
                return Array.Empty<string>();
            if (prefix.Contains('('))
                startIndex++; // skip '('
            startIndex += prefix.Length;
            var endIndex = namePart.IndexOf(')', startIndex);
            if (endIndex == -1)
                throw new FormatException("Missing featuring list closing parenthese: " + namePart);
            return namePart[startIndex..(endIndex - 1)].Split(new string[] { ", ", " & " }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public string GetDestination(string rootDir)
        {
            var info = ParseFileName(Path.GetFileNameWithoutExtension(filePath));
            return Path.Combine(rootDir, info.Artists[0], Path.GetFileName(filePath));
        }
    }
}
