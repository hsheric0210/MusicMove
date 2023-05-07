using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MusicMove
{
    [XmlRoot("album_mapping")]
    public class Mapping
    {
        [XmlArray("albums")]
        [XmlArrayItem("album")]
        public List<AlbumEntry> Albums;
    }

    public class AlbumEntry
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlText]
        public string AlbumCSV;
    }

    public class AlbumMapping
    {
        private readonly Album[] albums;

        public AlbumMapping(string xmlFilePath)
        {
            using var reader = new StreamReader(xmlFilePath, Encoding.UTF8);
            var xml = new XmlSerializer(typeof(Mapping));
            var dto = (Mapping)(xml.Deserialize(reader) ?? throw new XmlException("Album mapping XML deserializer returned null"));
            var csvDir = Path.GetDirectoryName(xmlFilePath);
            if (csvDir == null)
                throw new IOException("XML file parent directory is null");
            albums = dto.Albums.Select(a => new Album(a.Name, Path.Combine(csvDir, a.AlbumCSV))).ToArray();
        }

        public bool UpdateTags(string songFilePath)
        {
            foreach (var entry in albums)
            {
                if (entry.UpdateTags(songFilePath))
                    return true;
            }

            return false;
        }
    }
}
