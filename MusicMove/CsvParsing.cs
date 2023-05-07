using CsvHelper;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace MusicMove
{
    public abstract class CsvParsing<RT, T>
    {
        public IReadOnlyList<T> Entries { get; }

        protected CsvParsing(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException("Album CSV file not found: " + csvFilePath);
            using var csvReader = new StreamReader(csvFilePath, Encoding.UTF8);
            using var csv = new CsvReader(csvReader, CultureInfo.InvariantCulture);
            Entries = csv.GetRecords<RT>().Select(e => ParseRawEntry(e)).ToList();
        }

        public bool UpdateTags(string songFilePath)
        {
            var info = SongFileName.ParseFileName(Path.GetFileNameWithoutExtension(songFilePath));
            var comparer = new StringIgnoreCaseComparer();
            var matching = Entries.Where(entry =>
                GetTrackName(entry).Equals(info.SongName, StringComparison.InvariantCultureIgnoreCase)
                && new HashSet<string>(GetArtistList(entry), comparer).SetEquals(new HashSet<string>(info.Artists, comparer))).ToList();
            return UpdateTagsInternal(songFilePath, matching);
        }

        protected abstract T ParseRawEntry(RT raw);

        protected abstract string GetTrackName(T entry);
        protected abstract string[] GetArtistList(T entry);
        protected abstract bool UpdateTagsInternal(string songFilePath, IList<T> matchingEntries);

        protected sealed class StringIgnoreCaseComparer : IEqualityComparer<string>
        {
            public bool Equals(string? x, string? y) => x?.Equals(y, StringComparison.OrdinalIgnoreCase) ?? y == null;
            public int GetHashCode([DisallowNull] string obj) => obj.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
