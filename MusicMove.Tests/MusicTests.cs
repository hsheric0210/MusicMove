namespace MusicMove.Tests
{
    public class MusicTests
    {
        [Fact]
        public void ParseFileName_ParseSuccess()
        {
            var expected = new Music.FileNameInfo(new string[] { "a", "b", "c", "d", "e" }, "abcdefg (ft. abcdefgh)", Array.Empty<string>());
            var actual = Music.ParseFileName("a, b, c, d & e - abcdefg (ft. abcdefgh)");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseFileName_ParseError_NoSeparator()
        {
            Assert.Throws<FormatException>(() => _ = Music.ParseFileName("a, b, c, d & e _ abcdefg (ft. abcdefgh)"));
        }

        [Fact]
        public void ParseFileName_ParseError_BadFeaturing()
        {
            Assert.Throws<FormatException>(() => _ = Music.ParseFileName("a, b, c, d & e - abcdefg (ft. abcdefgh]"));
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