namespace MusicMove.Tests
{
    public class MusicTests
    {
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