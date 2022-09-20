using BenchmarkDotNet.Running;
using NMGDiscordBot.Tests.Benchmarks;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NMGDiscordBot.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<MemoryBenchmarkerDemo>();

            string[] files = new string[] {
                @"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Tests\11.log",
                @"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Tests\12.log",
                @"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Tests\Linux.log"
            };

            //List<string> files = new();
            //DirectoryInfo d = new DirectoryInfo(@"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs");
            //FileInfo[] f = d.GetFiles("*.log");
            //foreach (FileInfo file in f)
            //{
            //    files.Add(file.FullName);
            //}

            foreach (string file in files)
            {
                ParsedLogData parsedLogData = new ParsedLogData();
                ParseLog(file, parsedLogData);

                Console.WriteLine("Operating System: " + parsedLogData.OperatingSystem.Platform);
                Console.WriteLine("Present Plugins: (" + parsedLogData.PresentPlugins.Count + ")");
                foreach (var item in parsedLogData.PresentPlugins)
                {
                    Console.WriteLine("- Name: " + item.Name);
                }
                Console.WriteLine("Present Mods: (" + parsedLogData.PresentMods.Count + ")");
                foreach (var item in parsedLogData.PresentMods)
                {
                    Console.WriteLine("- Name: " + item.Name);
                    Console.WriteLine("--- Version: " + item.Version);
                    Console.WriteLine("--- Author: " + item.Author);
                    Console.WriteLine($"--- SHA256: {(string.IsNullOrEmpty(item.SHA_256) ? $"No hash present for NML v{parsedLogData.NMLStatus.NMLVersion}." : item.SHA_256)}");
                }
                Console.WriteLine("Is NML Present: " + parsedLogData.NMLStatus.IsNMLPresent);
                Console.WriteLine("Is NML Loaded: " + parsedLogData.NMLStatus.IsNMLLoaded);
                Console.WriteLine("NML Version: " + parsedLogData.NMLStatus.NMLVersion);
                Console.WriteLine("NML Version String Fallback: " + parsedLogData.NMLStatus.NMLVersionFallback);
                Console.WriteLine();
            }
        }

        // https://www.youtube.com/watch?v=FM5dpxJMULY
        // RegeX matching for ReadOnlySpan<char> needs .NET 7 IIRC

        private static void ParseLog(string file, ParsedLogData parsedLogData)
        {
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line is null || !(Utils.AMPM_Prefix.IsMatch(line) || Utils.TwentyFourHR_Prefix.IsMatch(line)))
                    {
                        continue;
                    }

                    ReadOnlySpan<char> current = TrimTime(line);

                    #if DEBUG
                        Console.WriteLine(current);
                    #endif

                    // Get the current OS
                    if (MatchesName(current, "Platform: ".AsSpan(), ReadOnlySpan<char>.Empty))
                    {
                        ReadOnlySpan<char> os = current.Slice(10, current.IndexOf(',') - 10);
                        if (os == "WindowsPlayer".AsSpan())
                            parsedLogData.OperatingSystem = new OperatingSystem(PlatformID.Win32NT, Utils.NullVersion);
                        else if (os == "LinuxPlayer".AsSpan())
                            parsedLogData.OperatingSystem = new OperatingSystem(PlatformID.Unix, Utils.NullVersion);
                        else
                            Console.WriteLine("Unrecognized Operating System found in log.");
                        continue;
                    }

                    // Get the names of all loaded plugins
                    if (MatchesName(current, "Argument: -LoadAssembly".AsSpan(), ReadOnlySpan<char>.Empty))
                    {
                        line = reader.ReadLine();
                        if (line is null)
                        {
                            continue;
                        }

                        current = TrimTime(line);

                        NeosPlugin neosPlugin = new();
                        neosPlugin.Name = current.Slice(10).ToString(); // current.Replace("Argument: ", "");
                        if (!parsedLogData.PresentPlugins.Add(neosPlugin))
                        {
                            Console.WriteLine("Duplicate Plugin!");     
                        }
                        else
                        {
                            parsedLogData.NMLStatus.IsNMLPresent = parsedLogData.NMLStatus.IsNMLPresent
                                | MatchesName(current, "", "NeosModLoader.dll");
                        }
                        continue;
                    }

                    // Check if NML is among the plugins to be loaded
                    if (MatchesName(current, "Loaded Extra Assembly: ", "NeosModLoader.dll"))
                    {
                        parsedLogData.NMLStatus.IsNMLLoaded = true;
                        continue;
                    }

                    // Check if a log starts with an NML-formatted string
                    if (Utils.NMLPrefix.IsMatch(current))
                    {
                        ParseNMLLog(ref current, parsedLogData);
                        continue;
                    }
                }
            }
            
            #if DEBUG
                Console.WriteLine();
            #endif
        }

        private static void ParseNMLLog(ref ReadOnlySpan<char> current, ParsedLogData parsedLogData)
        {
            ReadOnlySpan<char> currentNML = TrimNMLLog(current);

            // Get the current NML Version and fallback string
            if (MatchesName(currentNML, "NeosModLoader ", " starting up!"))
            {
                var v = Utils.TryGetVersionFromSentence(currentNML);

                parsedLogData.NMLStatus.NMLVersionFallback = v.Trimmed.ToString();
                if (v.Parsed is not null)
                    parsedLogData.NMLStatus.NMLVersion = v.Parsed;

                return;
            }

            // Get all loaded mods and parse their info
            if (MatchesName(currentNML, "loaded mod ".AsSpan(), ReadOnlySpan<char>.Empty))
            {
                var len = currentNML.IndexOf('[') + 1;
                var mod = currentNML.Slice(len, currentNML.IndexOf(']') - len).Split('/');

                NeosMod neosMod = new NeosMod();
                neosMod.Name = mod[0];
                neosMod.RawVersion = mod[1];

                var v = Utils.TryGetVersionFromWord(mod[1]);
                if (v is not null)
                    neosMod.Version = v;

                var index = currentNML.IndexOf("by ") + 3;
                if (parsedLogData.NMLStatus.NMLVersion >= new Version(1, 12, 0))
                {
                    neosMod.Author = currentNML.Slice(index, currentNML.IndexOf(" with 256hash: ") - index).ToString();
                    neosMod.SHA_256 = currentNML.Slice(currentNML.LastIndexOf(' ')).ToString();
                }
                else
                {
                    neosMod.Author = currentNML.Slice(index).ToString();
                }

                if (!parsedLogData.PresentMods.Add(neosMod))
                {
                    Console.WriteLine("Duplicate mod!");
                }

                return;
            }
        }

        private static void ParseNMLLog(string current, ParsedLogData parsedLogData)
        {
            string currentNML = TrimNMLLog(current);

            // Get the current NML Version and fallback string
            if (MatchesName(currentNML, "NeosModLoader ", " starting up!"))
            {
                var v = Utils.TryGetVersionFromSentence(currentNML);

                parsedLogData.NMLStatus.NMLVersionFallback = v.Trimmed;
                if (v.Parsed is not null)
                    parsedLogData.NMLStatus.NMLVersion = v.Parsed;

                return;
            }

            // Get all loaded mods and parse their info
            if (MatchesName(currentNML, "loaded mod "))
            {
                var len = currentNML.IndexOf('[') + 1;
                var mod = currentNML.Substring(len, currentNML.IndexOf(']') - len).Split('/');

                NeosMod neosMod = new NeosMod();
                neosMod.Name = mod[0];
                neosMod.RawVersion = mod[1];

                var v = Utils.TryGetVersionFromWord(mod[1]);
                if (v is not null)
                    neosMod.Version = v;

                var index = currentNML.IndexOf("by ") + 3;
                if (parsedLogData.NMLStatus.NMLVersion >= new Version(1, 12, 0))
                {
                    neosMod.Author = currentNML.Substring(index, currentNML.IndexOf(" with 256hash: ") - index);
                    neosMod.SHA_256 = currentNML.Substring(currentNML.LastIndexOf(' '));
                }
                else
                {
                    neosMod.Author = currentNML.Substring(index);
                }

                if (!parsedLogData.PresentMods.Add(neosMod))
                {
                    Console.WriteLine("Duplicate mod!");
                }
                
                return;
            }
        }

        private static string TrimTime(string input)
        {
            return input.Substring(input.IndexOf(')') + 2);
        }

        private static ReadOnlySpan<char> TrimTime(ReadOnlySpan<char> input)
        {
            return input.Slice(input.IndexOf(')') + 2);
        }

        private static string TrimNMLLog(string input)
        {
            return input.Substring(Utils.IndexOfNth(input, ']', 2) + 2);
        }

        private static ReadOnlySpan<char> TrimNMLLog(ReadOnlySpan<char> input)
        {
            return input.Slice(input.IndexOf(']')).Slice(input.IndexOf(']')); // Add 2?
        }

        private static bool MatchesName(string current, string prefix = "", string suffix = "")
        {
            var prefixEval = !string.IsNullOrEmpty(prefix);
            var suffixEval = !string.IsNullOrEmpty(suffix);

            if (prefixEval && suffixEval)
                return current.StartsWith(prefix) && current.EndsWith(suffix);
            else if (prefixEval) // Implies suffix is empty
                return current.StartsWith(prefix);
            else if (suffixEval) // Implies suffix is empty
                return current.EndsWith(suffix);
            else                  // Implies prefix and suffix are both empty
                return false;
        }

        private static bool MatchesName(ReadOnlySpan<char> current, ReadOnlySpan<char> prefix, ReadOnlySpan<char> suffix)
        {
            var prefixEval = prefix.Length > 0;
            var suffixEval = suffix.Length > 0;

            if (prefixEval && suffixEval)
                return current.StartsWith(prefix) && current.EndsWith(suffix);
            else if (prefixEval) // Implies suffix is empty
                return current.StartsWith(prefix);
            else if (suffixEval) // Implies suffix is empty
                return current.EndsWith(suffix);
            else                  // Implies prefix and suffix are both empty
                return false;
        }
    }
}