using CsvHelper.Configuration.Attributes;
using Serilog;
using static MusicMove.Album;

namespace MusicMove
{
    public class Album : CsvParsing<AlbumSongRawEntry, AlbumSongEntry>
    {
        public string AlbumName { get; }

        public Album(string albumName, string csvFilePath) : base(csvFilePath) => AlbumName = albumName;

        public class AlbumSongRawEntry
        {
            [Name("No.")]
            public string No { get; set; }

            public string Track { get; set; }

            [Name("Artist(s)")]
            public string Artists { get; set; }

            [Optional]
            public string Length { get; set; }

            [Name("Release Date")]
            [Optional]
            public string ReleaseDate { get; set; }

            [Name("Genre(s)")]
            public string Genres { get; set; }

            [Name("Featured Artist(s)")]
            [Optional]
            public string FeaturedArtists { get; set; }

            [Name("Remixer(s)")]
            [Optional]
            public string Remixers { get; set; }
        }

        public sealed record AlbumSongEntry(int No, string Track, string[] Artists, DateTime ReleaseDate, string[] Genres, string[] FeaturedArtists);

        protected override AlbumSongEntry ParseRawEntry(AlbumSongRawEntry raw)
        {
            var n = raw.No;
            while (n.EndsWith('.'))
                n = n[..^1];
            if (!int.TryParse(n, out var noInt))
                noInt = -1;
            if (!DateTime.TryParse(raw.ReleaseDate, out var releaseDate))
                releaseDate = DateTime.MinValue;

            return new AlbumSongEntry(
                noInt,
                raw.Track.Trim(),
                ArtistSplitter.SplitArtists(raw.Artists).ToArray(),
                releaseDate,
                raw.Genres.Split(new string[] { " / ", " | " }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                ArtistSplitter.SplitArtists(raw.FeaturedArtists).ToArray()
            );
        }

        protected override string GetTrackName(AlbumSongEntry entry) => entry.Track;
        protected override string[] GetArtistList(AlbumSongEntry entry) => entry.Artists;

        protected override bool UpdateTagsInternal(string songFilePath, IList<AlbumSongEntry> matchingEntries)
        {
            if (matchingEntries.Count > 1)
                throw new AggregateException("Duplicate album track entry matches: " + songFilePath + " -> " + string.Join(" | ", matchingEntries));
            else if (matchingEntries.Count == 0)
            {
                Log.Verbose("No album entry on the album {albumName} matches the file {file}", AlbumName, songFilePath);
                return false;
            }

            using var tagFile = TagLib.File.Create(songFilePath);
            var id3 = tagFile.GetTag(TagLib.TagTypes.Id3v2, true);
            var trackEntry = matchingEntries[0];
            if (!string.IsNullOrWhiteSpace(id3.Album) && !id3.Album.Equals(AlbumName, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("Album is not empty: {album}. Appending {newAlbum}", id3.Album, AlbumName);
            }
            else
            {
                Log.Information("Song {file} has album tag: {album}", songFilePath, id3.Album);
                id3.Album = AlbumName;
                id3.Track = (uint)trackEntry.No;
                id3.TrackCount = (uint)Entries.Count;
                if (trackEntry.ReleaseDate != DateTime.MinValue)
                    id3.Year = (uint)trackEntry.ReleaseDate.Year;
            }

            var genres = id3.Genres.Concat(trackEntry.Genres).Distinct(new StringIgnoreCaseComparer()).ToArray();
            Log.Information("Genre list update: {from} -> {to}", id3.JoinedGenres, string.Join("; ", genres));
            id3.Genres = genres;
            tagFile.Save();
            return true;
        }
    }
}
