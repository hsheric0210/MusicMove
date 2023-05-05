namespace MusicMove.Tests
{
    public class MusicTests
    {
        [Fact]
        public void ParseFileName_ParseSuccess_Normalized()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg (ft. acbd, efgh & foobar)");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_FrontFeaturing()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "");
            var actual = Music.ParseFileName("a, b, c, d & e (ft. acbd, efgh & foobar) - abcdefg");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_FeaturingWithoutParenthese()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_FrontFeaturingWithoutParenthese()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "");
            var actual = Music.ParseFileName("a, b, c, d & e ft. acbd, efgh & foobar - abcdefg");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_FeaturingWithoutParentheseRemix()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar (remix)");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_FeaturingWithoutParentheseRelease()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar [release]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_RemixTag_Single()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "(remix by A)", "");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar (remix by A)");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.RemixTag, actual.RemixTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_RemixTag_Multiple()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "(remix by A) (remix by B) (remix by C)", "");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar (remix by A) (remix by B) (remix by C)");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.RemixTag, actual.RemixTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_ReleaseTag_Single()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "[release by A]");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar [release by A]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.ReleaseTag, actual.ReleaseTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_ReleaseTag_Multiple()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "", "[release by A] [release by B] [release by C]");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar [release by A] [release by B] [release by C]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.ReleaseTag, actual.ReleaseTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_RemixAndReleaseTag_Complex()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "(remix by A) (remix by B) (remix by C)", "[release by A] [release by B] [release by C]");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg ft. acbd, efgh & foobar (remix by A) (remix by B) (remix by C) [release by A] [release by B] [release by C]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.RemixTag, actual.RemixTag);
            Assert.Equal(expected.ReleaseTag, actual.ReleaseTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_RemixAndReleaseTag_Complex_NoFeaturing()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", Array.Empty<string>(), "(remix by A) (remix by B) (remix by C)", "[release by A] [release by B] [release by C]");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg (remix by A) (remix by B) (remix by C) [release by A] [release by B] [release by C]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.RemixTag, actual.RemixTag);
            Assert.Equal(expected.ReleaseTag, actual.ReleaseTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_RemixAndReleaseTag_Complex_BothFeaturing()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "(remix by A) (remix by B) (remix by C)", "[release by A] [release by B] [release by C]");
            var actual = Music.ParseFileName("a, b, c, d & e ft. acbd - abcdefg (feat. efgh & foobar) (remix by A) (remix by B) (remix by C) [release by A] [release by B] [release by C]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.RemixTag, actual.RemixTag);
            Assert.Equal(expected.ReleaseTag, actual.ReleaseTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_RemixAndReleaseTag_Complex_NoFeaturing_LeastSpaces()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", Array.Empty<string>(), "(remix by A)(remix by B)(remix by C)", "[release by A][release by B][release by C]");
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg(remix by A)(remix by B)(remix by C)[release by A][release by B][release by C]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.RemixTag, actual.RemixTag);
            Assert.Equal(expected.ReleaseTag, actual.ReleaseTag);
        }

        [Fact]
        public void ParseFileName_ParseSuccess_RemixAndReleaseTag_Complex_BothFeaturing_LeastSpaces()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg", new string[] { "acbd", "efgh", "foobar" }, "(remix by A)(remix by B)(remix by C)", "[release by A][release by B][release by C]");
            var actual = Music.ParseFileName("a, b, c, d & e ft.acbd - abcdefg(feat.efgh, foobar)(remix by A)(remix by B)(remix by C)[release by A][release by B][release by C]");
            Assert.Equal(expected.Artists, actual.Artists);
            Assert.Equal(expected.SongName, actual.SongName);
            Assert.Equal(expected.Featuring, actual.Featuring);
            Assert.Equal(expected.RemixTag, actual.RemixTag);
            Assert.Equal(expected.ReleaseTag, actual.ReleaseTag);
        }

        [Fact]
        public void ParseFileName_ParseError_NoSeparator()
        {
            Assert.Throws<FormatException>(() => _ = Music.ParseFileName("a, b, c, d & e _ abcdefg (ft. abcdefgh)"));
        }

        [Fact]
        public void GetNormalizedName_MessedUp()
        {
            var expected = "a, b, c, d & e - abcdefg (ft. alpha, beta, gamma, delta & omikron) (Nightcore Mix) (REReRemix) [NCS Release] [Monstercat Release]";
            var actual = Music.GetNormalizedFileName(Music.ParseFileName("a x b X c & d, e (ft. alpha x beta & gamma) - abcdefg (feat. delta X omikron) (Nightcore Mix) (REReRemix) [NCS Release] [Monstercat Release]"));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetNormalizedName_CompoundArtists()
        {
            var expected = "Zeus X Crona - abcdefg (ft. alpha, beta, gamma, delta & omikron) (Nightcore Mix) (REReRemix) [NCS Release] [Monstercat Release]";
            var actual = Music.GetNormalizedFileName(Music.ParseFileName("zeus x crona (ft. alpha x beta & gamma) - abcdefg (feat. delta X omikron) (Nightcore Mix) (REReRemix) [NCS Release] [Monstercat Release]"));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDestination_ParseSuccess()
        {
            var expected = "D:\\Musics\\foo\\foo, bar & baz - foobar (ft. fizz).mp3";
            var actual = new Music("D:\\Musics\\foo, bar & baz - foobar (ft. fizz).mp3").GetDestination("D:\\Musics");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_EmptyFilePath()
        {
            Assert.Throws<ArgumentException>(() => _ = new Music("     "));
        }

    }
}