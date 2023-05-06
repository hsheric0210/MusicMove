﻿using Serilog;
using System.Text;
using static MusicMove.Music;

namespace MusicMove
{
    public static class SongFileName
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

        public static FileNameInfo ParseFileName(string fileName)
        {
            var separated = fileName.Split(" - ", 2, StringSplitOptions.RemoveEmptyEntries);
            if (separated.Length == 1)
                throw new FormatException("Artist-SongName separator not found: " + fileName);

            var featuring = new List<string>();
            var artistsPart = separated[0];
            int frontFeaturingBegin = -1, frontFeaturingEnd = -1;
            if (artistsPart.IndexOf("feat.") != 0)
                featuring.AddRange(ParseFeaturing(artistsPart, "feat.", ref frontFeaturingBegin, ref frontFeaturingEnd));
            if (artistsPart.IndexOf("ft.") != 0)
                featuring.AddRange(ParseFeaturing(artistsPart, "ft.", ref frontFeaturingBegin, ref frontFeaturingEnd));

            if (frontFeaturingBegin > 0)
                artistsPart = artistsPart[..(frontFeaturingBegin - 1)];
            var artists = EncodeSpecialArtists(artistsPart).Split(tokenDelimiters, StringSplitOptions.RemoveEmptyEntries).Select(a => DecodeSpecialArtists(a));
            Log.Verbose("Artists: {artists}", string.Join("; ", artists));

            var namePart = separated[1];
            Log.Verbose("Name: {name}", namePart);

            int backFeaturingBegin = -1, backFeaturingEnd = 0;
            featuring.AddRange(ParseFeaturing(namePart, "feat.", ref backFeaturingBegin, ref backFeaturingEnd));
            featuring.AddRange(ParseFeaturing(namePart, "ft.", ref backFeaturingBegin, ref backFeaturingEnd));

            if (backFeaturingEnd == -1) // if there're no featuring tag, search from the scratch.
                backFeaturingEnd = 0;

            var remixTag = "";
            var releaseTag = "";
            int remixTagBegin = namePart.Length - 1, releaseTagBegin = namePart.Length - 1;
            if (backFeaturingEnd < namePart.Length - 1)
            {
                remixTagBegin = namePart.IndexOf('(', backFeaturingEnd);
                if (remixTagBegin > 0)
                {
                    var remixTagEnd = namePart.LastIndexOf(')', namePart.Length - 1, namePart.Length - remixTagBegin - 1) + 1;
                    if (remixTagEnd > remixTagBegin)
                    {
                        remixTag = namePart[remixTagBegin..remixTagEnd];
                    }
                }

                releaseTagBegin = namePart.IndexOf('[', backFeaturingEnd);
                if (releaseTagBegin > 0)
                {
                    var releaseTagEnd = namePart.LastIndexOf(']', namePart.Length - 1, namePart.Length - remixTagBegin - 1) + 1;
                    if (releaseTagEnd > releaseTagBegin)
                        releaseTag = namePart[releaseTagBegin..releaseTagEnd];
                }
            }

            var nameEnd = namePart.Length;
            if (backFeaturingBegin > 0)
                nameEnd = Math.Min(nameEnd, backFeaturingBegin);
            if (remixTagBegin > 0)
                nameEnd = Math.Min(nameEnd, remixTagBegin);
            if (releaseTagBegin > 0)
                nameEnd = Math.Min(nameEnd, releaseTagBegin);
            var name = namePart[..nameEnd].Trim();

            return new FileNameInfo(artists.Select(a => a.Trim()).ToArray(), name, featuring.Select(ft => ft.Trim()).ToArray(), remixTag, releaseTag);
        }

        private static string[] ParseFeaturing(string namePart, string prefix, ref int beginIndex, ref int finIndex, string closingChar = ")")
        {
            var startIndex = namePart.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1)
                return Array.Empty<string>();
            if (beginIndex == -1 || beginIndex > startIndex)
            {
                beginIndex = startIndex;
                if (namePart[startIndex - 1] == '(')
                    beginIndex--;
            }
            startIndex += prefix.Length;
            var endIndex = string.IsNullOrWhiteSpace(closingChar) ? namePart.Length : namePart.IndexOf(closingChar, startIndex);
            var remixTagStartIndex = namePart.IndexOf(" (", startIndex); // Start of remix tag (e.g. ' (Nightcore Mix)')
            if (remixTagStartIndex != -1)
                endIndex = Math.Min(endIndex, remixTagStartIndex);
            if (endIndex == -1)
                endIndex = namePart.IndexOf(" [", startIndex); // Start of release tag (e.g. ' [NCS Release]')
            if (endIndex == -1)
                endIndex = namePart.Length;
            if (finIndex == -1 || finIndex < endIndex + 1)
                finIndex = endIndex + 1;
            return namePart[startIndex..endIndex].Split(tokenDelimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public static string GetNormalizedFileName(FileNameInfo info)
        {
            var builder = new StringBuilder();
            var artistPart = (info.Artists.Length > 1 ? (string.Join(", ", info.Artists[..^1]) + " & ") : "") + info.Artists.Last();
            builder.Append(artistPart);
            builder.Append(" - ").Append(info.SongName);
            if (info.Featuring.Length > 0)
            {
                var featuringPart = (info.Featuring.Length > 1 ? (string.Join(", ", info.Featuring[..^1]) + " & ") : "") + info.Featuring.Last();
                builder.Append(" (ft. ").Append(featuringPart).Append(')');
            }
            if (!string.IsNullOrWhiteSpace(info.RemixTag))
            {
                builder.Append(' ').Append(info.RemixTag);
            }
            if (!string.IsNullOrWhiteSpace(info.ReleaseTag))
                builder.Append(' ').Append(info.ReleaseTag);
            return builder.ToString();
        }

    }
}