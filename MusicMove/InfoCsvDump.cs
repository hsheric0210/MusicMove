using Serilog;
using static MusicMove.InfoCsvDump;

namespace MusicMove
{
    public class InfoCsvDump : CsvParsing<SongRawEntry, SongEntry>
    {
        public InfoCsvDump(string csvFilePath) : base(csvFilePath) { }

        public class SongRawEntry
        {
            public string Track { get; set; }

            public string Brand { get; set; }

            public string ReleaseDate { get; set; }

            public string Artists { get; set; }

            public string Featuring { get; set; }

            public string Albums { get; set; }

            public string CatalogNumber { get; set; }

            public string BPM { get; set; }

            public string Key { get; set; }

            public string ReleaseGenre { get; set; }

            public string CommunityGenre { get; set; }

            public string Subgenre { get; set; }

            public string Image { get; set; }
        }

        public sealed record SongEntry(string Track, string Brand, DateTime ReleaseDate, string[] Artists, string[] Featuring, string[] Albums, string[] CatalogNumber, int BPM, string Key, string[] Genre);

        protected override SongEntry ParseRawEntry(SongRawEntry raw)
        {
            if (!DateTime.TryParse(raw.ReleaseDate, out var releaseDate))
            {
                if (!string.IsNullOrWhiteSpace(raw.ReleaseDate))
                    Log.Warning("Failed to parse Release Date of the entry {artists} - {track}", raw.Artists, raw.Track);
                releaseDate = DateTime.MinValue;
            }

            if (!int.TryParse(raw.BPM, out var bpm))
            {
                if (!string.IsNullOrWhiteSpace(raw.BPM))
                    Log.Warning("Failed to parse BPM of the entry {artists} - {track}", raw.Artists, raw.Track);
                bpm = 0;
            }

            string[] SplitGenre(string genreString) => genreString.Split(new string[] { "; ", " / ", " | " }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var rgenre = SplitGenre(raw.ReleaseGenre);
            var cgenre = SplitGenre(raw.CommunityGenre);
            var subgenre = SplitGenre(raw.Subgenre);
            var genre = rgenre.Concat(cgenre).Concat(subgenre).Distinct(new StringIgnoreCaseComparer());

            var key = raw.Key.Trim();
            key = key.Replace('#', '♯');
            key = key.Replace("Maj", "maj");
            key = key.Replace("Min", "min");

            return new SongEntry(
                raw.Track.Trim(),
                raw.Brand.Trim(),
                releaseDate,
                ArtistSplitter.SplitArtists(raw.Artists).ToArray(),
                ArtistSplitter.SplitArtists(raw.Featuring).ToArray(),
                new string[] { raw.Albums.Trim() },
                raw.CatalogNumber.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                bpm,
                key,
                genre.ToArray()
            );
        }

        protected override string GetTrackName(SongEntry entry) => entry.Track;
        protected override string[] GetArtistList(SongEntry entry) => entry.Artists;

        protected override bool UpdateTagsInternal(string songFilePath, IList<SongEntry> matchingEntries)
        {
            if (matchingEntries.Count > 1)
                throw new AggregateException("Multiple entries match: " + songFilePath + " -> " + string.Join(" | ", matchingEntries));
            else if (matchingEntries.Count == 0)
            {
                Log.Verbose("No entries match the file {file}", songFilePath);
                return false;
            }

            using var tagFile = TagLib.File.Create(songFilePath);
            var id3 = tagFile.GetTag(TagLib.TagTypes.Id3v2, true);
            var entry = matchingEntries[0];

            if (string.IsNullOrWhiteSpace(id3.Copyright))
                id3.Copyright = entry.Brand;

            if (string.IsNullOrWhiteSpace(id3.Publisher))
                id3.Publisher = entry.Brand;

            if (entry.ReleaseDate != DateTime.MinValue)
                id3.Year = (uint)entry.ReleaseDate.Year;

            if (string.IsNullOrWhiteSpace(id3.Album))
                id3.Album = string.Join("; ", entry.Albums);

            id3.BeatsPerMinute = (uint)entry.BPM;
            id3.InitialKey = entry.Key;

            var genres = id3.Genres.Concat(entry.Genre).Distinct(new StringIgnoreCaseComparer()).ToArray();
            Log.Information("Genre list update: {from} -> {to}", id3.JoinedGenres, string.Join("; ", genres));
            id3.Genres = genres;
            tagFile.Save();
            return true;
        }
    }
}
