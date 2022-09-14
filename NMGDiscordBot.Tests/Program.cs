using BenchmarkDotNet.Running;
using NMGDiscordBot.Tests.Benchmarks;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NMGDiscordBot.Tests
{
    public class Program
    {
        private static ParsedLogData parsedLogData = new ParsedLogData();

        public static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<MemoryBenchmarkerDemo>();
            string file = @"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs\DESKTOP-4GOD64O - 2022.1.28.1335 - 2022-09-08 19_56_03.log";
            ParseLog(file);

            Console.WriteLine();
            Console.WriteLine("Present Plugins: (" + parsedLogData.PresentPlugins.Count + ")");
            foreach (var item in parsedLogData.PresentPlugins)
            {
                Console.WriteLine("- " + item.Name);
            }
            Console.WriteLine("Present Mods: (" + parsedLogData.PresentMods.Count + ")");
            foreach (var item in parsedLogData.PresentMods)
            {
                Console.WriteLine("- " + item.Name);
                Console.WriteLine("--- " + item.RichVersion);
                Console.WriteLine("--- " + item.Author);
            }
            Console.WriteLine("Is NML Present: " + parsedLogData.NMLStatus.IsNMLPresent);
            Console.WriteLine("Is NML Loaded: " + parsedLogData.NMLStatus.IsNMLLoaded);
            Console.WriteLine("NML Version: " + parsedLogData.NMLStatus.NMLVersion);
            Console.WriteLine("NML Version String Fallback: " + parsedLogData.NMLStatus.NMLVersionFallback);
        }

        private static void ParseLog(string file)
        {
            var enumerator = File.ReadLines(file).GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (Utils.TimePrefix.IsMatch(enumerator.Current))
                {
                    string current = TrimTime(enumerator.Current);
                    Console.WriteLine(current);

                    // Get the names of all loaded plugins
                    if (MatchesName(current, "Argument: -LoadAssembly"))
                    {
                        if (enumerator.MoveNext())
                        {
                            current = TrimTime(enumerator.Current);

                            NeosPlugin neosPlugin = new();
                            neosPlugin.Name = current.Replace("Argument: ", "");
                            if (!parsedLogData.PresentPlugins.Add(neosPlugin))
                                Console.WriteLine("Duplicate Plugin!");

                            parsedLogData.NMLStatus.IsNMLPresent = parsedLogData.NMLStatus.IsNMLPresent | MatchesName(current, "", "NeosModLoader.dll");
                        }
                    }

                    // Check if NML is among the plugins to be loaded
                    if (MatchesName(current, "Loaded Extra Assembly: ", "NeosModLoader.dll"))
                    {
                        parsedLogData.NMLStatus.IsNMLLoaded = true;
                    }

                    // Check if a log starts with an NML-formatted string
                    if (Utils.NMLPrefix.IsMatch(current))
                    {
                        ParseNMLLog(current);
                    }
                }
            }
        }

        private static void ParseNMLLog(string current)
        {
            string currentNML = TrimNMLLog(current);

            // Get the current NML Version and fallback string
            if (MatchesName(currentNML, "NeosModLoader ", " starting up!"))
            {
                var v = Utils.TryGetVersionFromSentence(currentNML);

                parsedLogData.NMLStatus.NMLVersionFallback = v.Trimmed;
                if (v.Parsed is not null)
                    parsedLogData.NMLStatus.NMLVersion = v.Parsed;
            }

            // Get all loaded mods and parse their info
            if (MatchesName(currentNML, "loaded mod "))
            {
                var split = currentNML.Split(' ').Skip(2).ToArray();
                var mod = split[0].Substring(1, split[0].Length - 2).Split('/');

                NeosMod neosMod = new NeosMod();
                neosMod.Name = mod[0];
                neosMod.Version = mod[1];
                neosMod.Author = currentNML.Substring(currentNML.IndexOf("by ") + 3);

                var v = Utils.TryGetVersionFromWord(mod[1]);
                if (v is not null)
                    neosMod.RichVersion = v;

                if (!parsedLogData.PresentMods.Add(neosMod))
                    Console.WriteLine("Duplicate mod!");
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