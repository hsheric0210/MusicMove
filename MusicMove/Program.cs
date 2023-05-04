using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Runtime;

namespace MusicMove
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.File("MusicMove.log", buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(1)))
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            try
            {
                var rootDir = Environment.CurrentDirectory; // If you will execute this by Drag-and-Drop, cwd will be set to the specified directory.

                var targets = new List<string>(args);
                foreach (var target in args)
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

                        var music = new Music(file);

                        var dest = music.GetDestination(rootDir);
                        var destDir = Path.GetDirectoryName(dest);
                        if (string.IsNullOrEmpty(destDir))
                            throw new IOException("Invalid destination parent directory of the file: " + file);
                        var destDirInfo = new DirectoryInfo(destDir);
                        if (!destDirInfo.Exists)
                            destDirInfo.Create();

                        info.MoveTo(dest);
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