using Serilog;

namespace MusicMove
{
    public class Music
    {
        private readonly string filePath;

        public Music(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is null or blank", nameof(filePath));
            this.filePath = Path.GetFullPath(filePath);
            Log.Verbose("Music file accepted: {path} -> {fullPath}", filePath, this.filePath);
        }

        public sealed record FileNameInfo(string[] Artists, string SongName, string[] Featuring, string RemixTag, string ReleaseTag)
        {
            public bool Equals(FileNameInfo? other) => other != null && Artists.SequenceEqual(other.Artists) && SongName.Equals(other.SongName);

            public override int GetHashCode() => throw new NotImplementedException();
        }

        public string GetDestination(string rootDir)
        {
            var info = SongFileName.ParseFileName(Path.GetFileNameWithoutExtension(filePath));
            return Path.Combine(rootDir, info.Artists[0], Path.GetFileName(filePath));
        }

        public string GetCoverImagePath(string extension)
        {
            var myDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(myDir))
                throw new IOException("Root directory unavailable: " + filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var myFile = new FileInfo(Path.Combine(myDir, fileName + extension));
            var myFile2 = new FileInfo(Path.Combine(myDir, fileName[..^2] + extension));
            if (myFile2.Exists)
                Log.Information("Original cover art file exists: {file}", myFile2.FullName);
            if (myFile2.Exists && !myFile.Exists)
            {
                myFile2.CopyTo(myFile.FullName);
                Log.Information("Copy Cover {src} -> {dst}", myFile2.FullName, myFile.FullName);
            }
            return myFile.FullName;
        }
    }
}
