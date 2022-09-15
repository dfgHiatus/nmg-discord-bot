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
                @"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Tests\12.log"
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
                    Console.WriteLine("--- SHA256: " + item.SHA_256 ?? $"No hash present for {item.Version}.");
                }
                Console.WriteLine("Is NML Present: " + parsedLogData.NMLStatus.IsNMLPresent);
                Console.WriteLine("Is NML Loaded: " + parsedLogData.NMLStatus.IsNMLLoaded);
                Console.WriteLine("NML Version: " + parsedLogData.NMLStatus.NMLVersion);
                Console.WriteLine("NML Version String Fallback: " + parsedLogData.NMLStatus.NMLVersionFallback);
                Console.WriteLine();
            }
        }

        private static void ParseLog(string file, ParsedLogData parsedLogData)
        {
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line is null || !Utils.TimePrefix.IsMatch(line))
                    {
                        continue;
                    }

                    string current = TrimTime(line);

                    #if DEBUG
                        Console.WriteLine(current);
                    #endif

                    // Get the current OS
                    if (MatchesName(current, "Platform: "))
                    {
                        string os = current.Substring(10, current.IndexOf(',') - 10);
                        switch(os)
                        {
                            case "WindowsPlayer":
                                parsedLogData.OperatingSystem = new OperatingSystem(PlatformID.Win32NT, Utils.NullVersion);
                                break;
                            default:
                                Console.WriteLine("Unrecognized Operating System found in log.");
                                break;
                        }
                    }

                    // Get the names of all loaded plugins
                    if (MatchesName(current, "Argument: -LoadAssembly"))
                    {
                        line = reader.ReadLine();
                        if (line is null)
                        {
                            continue;
                        }

                        current = TrimTime(line);

                        NeosPlugin neosPlugin = new();
                        neosPlugin.Name = current.Replace("Argument: ", "");
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
                        ParseNMLLog(current, parsedLogData);
                        continue;
                    }
                }
            }
            
            #if DEBUG
                Console.WriteLine();
            #endif
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
                var split = currentNML.Split(' ').Skip(2).ToArray();
                var mod = split[0].Substring(1, split[0].Length - 2).Split('/');

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
                    neosMod.SHA_256 = split[split.Length - 1];
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

        private static string TrimNMLLog(string input)
        {
            return input.Substring(Utils.IndexOfNth(input, ']', 2) + 2);
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
    }
}