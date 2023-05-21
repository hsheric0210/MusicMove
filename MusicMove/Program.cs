using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Data;
using System.Text;

namespace MusicMove
{
    internal static class Program
    {
        private enum OperatingMode
        {
            Move,
            Parse_CoverArt,
            Move_Instrumental,
            Rename_Instrumental,
            S3RL_Name_Normalize,
            Name_Normalize,
            Import_Tags,
            Import_Album_Mapping,
            Import_Dump
        }

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.File("MusicMove.log", buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(1)))
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            try
            {
                TagLib.Id3v2.Tag.DefaultVersion = 4;
                TagLib.Id3v2.Tag.ForceDefaultVersion = true;

                var input = new List<string>(args);
                if (args.Length == 1 && args[0].StartsWith('@')) // input file support
                {
                    input = File.ReadAllLines(args[0][1..].Trim('"')).Select(i => i.Trim().Trim('"')).ToList();
                }

                var op = OperatingMode.Move;
                AlbumMapping? amap = null;
                InfoCsvDump? dmp = null;
                if (Environment.GetEnvironmentVariable("MM_COVER")?.Equals("1") ?? false)
                {
                    Log.Information("Parse_CoverArt mode.");
                    op = OperatingMode.Parse_CoverArt;
                }
                else if (Environment.GetEnvironmentVariable("MM_INSTRU")?.Equals("1") ?? false)
                {
                    Log.Information("Move_Instrumental mode.");
                    op = OperatingMode.Move_Instrumental;
                }
                else if (Environment.GetEnvironmentVariable("MM_RINSTRU")?.Equals("1") ?? false)
                {
                    Log.Information("Rename_Instrumental mode.");
                    op = OperatingMode.Rename_Instrumental;
                }
                else if (Environment.GetEnvironmentVariable("MM_NAME_NORMALIZE")?.Equals("1") ?? false)
                {
                    Log.Information("Name_Normalize mode.");
                    op = OperatingMode.Name_Normalize;
                }
                else if (Environment.GetEnvironmentVariable("MM_NAME_NORMALIZE_S3RL")?.Equals("1") ?? false)
                {
                    Log.Information("S3RL_Name_Normalize mode.");
                    op = OperatingMode.S3RL_Name_Normalize;
                }
                else if (Environment.GetEnvironmentVariable("MM_TAGS")?.Equals("1") ?? false)
                {
                    Log.Information("Import_Tags mode.");
                    op = OperatingMode.Import_Tags;
                }
                else if (Environment.GetEnvironmentVariable("MM_ALBUM_MAPPING") != null)
                {
                    Log.Information("Import_Album_Mapping mode.");
                    op = OperatingMode.Import_Album_Mapping;
                    amap = new AlbumMapping(Environment.GetEnvironmentVariable("MM_ALBUM_MAPPING"));
                }
                else if (Environment.GetEnvironmentVariable("MM_DUMP") != null)
                {
                    Log.Information("Import_Dump mode.");
                    op = OperatingMode.Import_Dump;
                    dmp = new InfoCsvDump(Environment.GetEnvironmentVariable("MM_DUMP"));
                }

                var rootDir = Environment.CurrentDirectory; // If you will execute this by Drag-and-Drop, cwd will be set to the specified directory.

                var targets = new List<string>(input);
                foreach (var target in input)
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(target);
                        if (!dirInfo.Exists)
                            continue;
                        targets.Remove(target);
                        targets.AddRange(dirInfo.EnumerateFilesRecursive("*.mp3"));
                        targets.AddRange(dirInfo.EnumerateFilesRecursive("*.wav"));
                        targets.AddRange(dirInfo.EnumerateFilesRecursive("*.ogg"));
                        targets.AddRange(dirInfo.EnumerateFilesRecursive("*.jpg"));
                        targets.AddRange(dirInfo.EnumerateFilesRecursive("*.png"));
                        targets.AddRange(dirInfo.EnumerateFilesRecursive("*.webp"));
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to enumerate files in a directory: {dir}", target);
                    }
                }

                foreach (var file in targets)
                {
                    try
                    {
                        var info = new FileInfo(file);
                        if (!info.Exists)
                        {
                            Log.Warning("File not exists: {file}", file);
                            continue;
                        }
                        Log.Verbose("Execution for file {file}", file);

                        var music = new Music(file);

                        switch (op)
                        {
                            case OperatingMode.Move:
                            {
                                var dest = music.GetDestination(rootDir);
                                var destDir = Path.GetDirectoryName(dest);
                                if (string.IsNullOrEmpty(destDir))
                                    throw new IOException("Invalid destination parent directory of the file: " + file);
                                var destDirInfo = new DirectoryInfo(destDir);
                                if (!destDirInfo.Exists)
                                    destDirInfo.Create();

                                info.MoveTo(dest);
                                Log.Information("Move {src} -> {dst}", info.FullName, dest);
                                break;
                            }
                            case OperatingMode.Parse_CoverArt:
                            {
                                music.GetCoverImagePath(".jpg");
                                music.GetCoverImagePath(".png");
                                music.GetCoverImagePath(".webp");
                                break;
                            }
                            case OperatingMode.Move_Instrumental:
                            {
                                var instrumental = new FileInfo(file);
                                var parent = Path.GetDirectoryName(file);
                                if (parent == null)
                                    throw new IOException("Parent directory empty: " + file);
                                var regularFn = Path.Combine(parent, Path.GetFileNameWithoutExtension(file)[..^2] + Path.GetExtension(file));
                                Log.Information("Regular Filename is {file}", regularFn);
                                var regular = new FileInfo(regularFn);
                                var dest = Path.Combine(rootDir, "RegularAndInstrumental");
                                if (!Directory.Exists(dest))
                                    Directory.CreateDirectory(dest);
                                if (regular.Exists)
                                {
                                    var idest = Path.Combine(dest, instrumental.Name);
                                    var rdest = Path.Combine(dest, regular.Name);
                                    instrumental.MoveTo(idest);
                                    regular.MoveTo(rdest);
                                    Log.Information("Move instrumental {src} -> {dst}", instrumental.FullName, idest);
                                    Log.Information("Move regular {src} -> {dst}", regular.FullName, rdest);
                                }
                                break;
                            }
                            case OperatingMode.Rename_Instrumental:
                            {
                                var isInstrumental = false;
                                var parent = Path.GetDirectoryName(file);
                                if (parent == null)
                                    throw new IOException("Parent directory empty: " + file);
                                using (var tFile = TagLib.File.Create(file))
                                {
                                    if (tFile is null or (default(TagLib.File)))
                                        throw new FormatException("File " + file + " doesn't have any tag");
                                    var tag = tFile.GetTag(TagLib.TagTypes.Id3v2);
                                    if (tag == null)
                                        throw new FormatException("File " + file + " doesn't have ID3v2 tag");
                                    isInstrumental = tag.Title.Contains("Instru", StringComparison.OrdinalIgnoreCase);
                                }
                                if (isInstrumental)
                                {
                                    var ren_to = Path.Combine(parent, Path.GetFileNameWithoutExtension(file) + ".Instrumental" + Path.GetExtension(file));
                                    File.Move(file, ren_to);
                                    Log.Information("Rename instrumental {src} -> {dst}", file, ren_to);
                                }
                                break;
                            }
                            case OperatingMode.Name_Normalize:
                            {
                                var parent = Path.GetDirectoryName(file);
                                if (parent == null)
                                    throw new IOException("Parent directory empty: " + file);
                                var normalized = SongFileName.GetNormalizedFileName(SongFileName.ParseFileName(Path.GetFileNameWithoutExtension(file)));
                                var dest = Path.Combine(parent, normalized + Path.GetExtension(file));
                                if (!Path.GetFileName(file).Equals(Path.GetFileName(dest)))
                                {
                                    if (File.Exists(dest))
                                    {
                                        var dir = new DirectoryInfo(Path.Combine(parent, "_duplicated"));
                                        if (!dir.Exists)
                                            dir.Create();
                                        var dupFile = Path.Combine(parent, "_duplicated", Path.GetFileName(file));
                                        File.Move(file, dupFile);
                                        Log.Warning("Destination file already exists: {src} -> {dst}. The file is moved to {dupFile}", file, dest, dupFile);
                                        break;
                                    }

                                    File.Move(file, dest);
                                    Log.Information("Normalized song file name: {src} -> {dst}", file, dest);
                                }
                                break;
                            }
                            case OperatingMode.S3RL_Name_Normalize:
                            {
                                var parent = Path.GetDirectoryName(file);
                                if (parent == null)
                                    throw new IOException("Parent directory empty: " + file);
                                var normalized = SongFileName.GetNormalizedFileName(SongFileName.ParseFileName(Path.GetFileNameWithoutExtension(file), s3rl: true));
                                var dest = Path.Combine(parent, normalized + Path.GetExtension(file));
                                if (!Path.GetFileName(file).Equals(Path.GetFileName(dest)))
                                {
                                    if (File.Exists(dest))
                                    {
                                        var dir = new DirectoryInfo(Path.Combine(parent, "_duplicated"));
                                        if (!dir.Exists)
                                            dir.Create();
                                        var dupFile = Path.Combine(parent, "_duplicated", Path.GetFileName(file));
                                        File.Move(file, dupFile);
                                        Log.Warning("Destination file already exists: {src} -> {dst}. The file is moved to {dupFile}", file, dest, dupFile);
                                        break;
                                    }

                                    File.Move(file, dest);
                                    Log.Information("Normalized song file name: {src} -> {dst}", file, dest);
                                }
                                break;
                            }
                            case OperatingMode.Import_Tags:
                            {
                                var fName = Path.GetFileNameWithoutExtension(file);
                                var nInfo = SongFileName.ParseFileName(fName);
                                var isNCS = fName.Contains("[NCS Release]");
                                using (var tFile = TagLib.File.Create(file))
                                {
                                    if (tFile is null or (default(TagLib.File)))
                                        throw new FormatException("File " + file + " cannot be opened.");

                                    // Drop unnecessary ID3v1 tags
                                    try
                                    {
                                        tFile.RemoveTags(TagLib.TagTypes.Id3v1);
                                    }
                                    catch { }

                                    var tag = tFile.GetTag(TagLib.TagTypes.Id3v2, true);
                                    if (tag == null)
                                        throw new FormatException("File " + file + " doesn't support ID3v2 tag");

                                    var composerList = nInfo.Artists.Concat(nInfo.Featuring).ToArray();
                                    tag.Performers = composerList;
                                    Log.Information("File {file} set performers to {performers}", file, tag.JoinedPerformers);

                                    if (tag.Composers == null || tag.Composers.Length == 0) // Do not overwrite composers.
                                    {
                                        tag.Composers = composerList;
                                        Log.Information("File {file} set composers to {composers}", file, tag.JoinedComposers);
                                    }

                                    if (isNCS && string.IsNullOrWhiteSpace(tag.Publisher)) // Do not overwrite publisher.
                                    {
                                        tag.Publisher = "NoCopyrightSounds";
                                        Log.Information("File {file} set publisher to {publisher}", file, tag.Publisher);
                                    }

                                    if (isNCS && string.IsNullOrWhiteSpace(tag.Copyright)) // Do not overwrite publisher.
                                    {
                                        tag.Copyright = "NoCopyrightSounds";
                                        Log.Information("File {file} set copyright to {copyright}", file, tag.Copyright);
                                    }

                                    var titleBuilder = new StringBuilder();
                                    titleBuilder.Append(nInfo.SongName);
                                    if (!string.IsNullOrWhiteSpace(nInfo.RemixTag))
                                        titleBuilder.Append(' ').Append(nInfo.RemixTag);
                                    if (!string.IsNullOrWhiteSpace(nInfo.ReleaseTag))
                                        titleBuilder.Append(' ').Append(nInfo.ReleaseTag);
                                    tag.Title = titleBuilder.ToString();
                                    Log.Information("File {file} set title to {title}", file, tag.Title);
                                    tFile.Save();
                                }
                                break;
                            }
                            case OperatingMode.Import_Album_Mapping:
                            {
                                amap.UpdateTags(file);
                                break;
                            }
                            case OperatingMode.Import_Dump:
                            {
                                dmp.UpdateTags(file);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Exception occurred while processing the specified file: {file}", file);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Exception during initialization or core execution.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IEnumerable<string> EnumerateFilesRecursive(this DirectoryInfo dirInfo, string searchPattern)
        {
            return dirInfo.EnumerateFiles(searchPattern, new EnumerationOptions
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = 0,
                IgnoreInaccessible = true,
                MaxRecursionDepth = 16,
                MatchCasing = MatchCasing.CaseInsensitive
            }).Select(fi => fi.FullName);
        }
    }
}