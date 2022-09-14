using BenchmarkDotNet.Running;
using NMGDiscordBot.Tests.Benchmarks;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NMGDiscordBot.Tests
{
    public class Program
    {
        private static HashSet<string> PresentPlugins = new();
        private static bool IsNMLPresent = false;
        private static bool IsNMLLoaded = false;
        private static Version NMLVersion;
        private static string NMLVersionFallback;

        public static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<MemoryBenchmarkerDemo>();
            string file = @"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs\DESKTOP-4GOD64O - 2022.1.28.1335 - 2022-09-08 19_56_03.log";
            ParseLog(file);

            Console.WriteLine();
            Console.WriteLine("Present Plugins: (" + PresentPlugins.Count + ")");
            foreach (var item in PresentPlugins)
            {
                Console.WriteLine("- " + item);
            }
            Console.WriteLine("Is NML Present: " + IsNMLPresent);
            Console.WriteLine("Is NML Loaded: " + IsNMLLoaded);
            Console.WriteLine("NML Version: " + NMLVersion);
            Console.WriteLine("NML Version String Fallback: " + NMLVersionFallback);
        }

        private static void ParseLog(string file)
        {
            var enumerator = File.ReadLines(file).GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (Utils.startsWithTimePrefix.IsMatch(enumerator.Current))
                {
                    string current = TrimTime(enumerator.Current);
                    Console.WriteLine(current);

                    if (MatchesName(current, "Argument: -LoadAssembly"))
                    {
                        if (enumerator.MoveNext())
                        {
                            current = TrimTime(enumerator.Current);
                            PresentPlugins.Add(current.Replace("Argument: ", ""));
                            IsNMLPresent = IsNMLPresent | MatchesName(current, "", "NeosModLoader.dll");
                        }
                    }

                    if (MatchesName(current, "Loaded Extra Assembly: ", "NeosModLoader.dll"))
                    {
                        IsNMLLoaded = true;
                    }

                    if (Utils.startsWithNMLPrefix.IsMatch(current))
                    {
                        ParseNMLLog(current);
                    }
                }
            }
        }

        private static void ParseNMLLog(string current)
        {
            string currentNML = TrimNMLLog(current);

            if (MatchesName(currentNML, "NeosModLoader ", " starting up!"))
            {
                Version v;
                string s = Regex.Replace(currentNML.Split(' ')[1], "[^0-9.]", "");
                if (Version.TryParse(s, out v))
                {
                    if (v is not null)
                        NMLVersion = v;
                    else
                        Console.WriteLine("Bad version format detected");
                }   
                NMLVersionFallback = s;
            }
        }

        private static string TrimTime(string input)
        {
            return input.Substring(input.IndexOf(')') + 2);
        }

        private static string TrimNMLLog(string input)
        {
            return input.Substring(IndexOfNth(input, ']', 2) + 2);
        }

        private static int IndexOfNth(string str, char c, int n)
        {
            int s = -1;
            for (int i = 0; i < n; i++)
            {
                s = str.IndexOf(c, s + 1);
                if (s == -1) break;
            }
            return s;
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