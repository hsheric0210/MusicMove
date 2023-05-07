namespace MusicMove
{
    public static class ArtistSplitter
    {
        private static readonly string[] tokenDelimiters = new string[] { ", ", " & ", " X ", " x " };
        private static Dictionary<string, string> ArtistMap = new()
        {
            ["T & Sugah"] = "__t_and_sugah__",
            ["Zeus X Crona"] = "__zeus_x_crona__",
            ["Raven & Kreyn"] = "__raven_and_kreyn__",
        };

        private static Dictionary<string, string> ArtistReverseMap = ArtistMap.ToDictionary((i) => i.Value, (i) => i.Key);

        private static string EncodeSpecialArtists(string artistsPart)
        {
            var artists = artistsPart;
            foreach (var entry in ArtistMap)
                artists = artists.Replace(entry.Key, entry.Value, StringComparison.OrdinalIgnoreCase);
            return artists;
        }

        private static string DecodeSpecialArtists(string artist) => ArtistReverseMap.TryGetValue(artist, out var decodedName) ? decodedName : artist;

        public static IEnumerable<string> SplitArtists(string artistsPart) => EncodeSpecialArtists(artistsPart).Split(tokenDelimiters, StringSplitOptions.RemoveEmptyEntries).Select(a => DecodeSpecialArtists(a));
    }
}
