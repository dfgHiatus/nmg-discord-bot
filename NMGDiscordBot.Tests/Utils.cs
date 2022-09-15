using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace NMGDiscordBot.Tests
{
    internal static class Utils
    {
        public static readonly Version NullVersion = new Version(0, 0, 0);
        public static readonly Regex TimePrefix = new Regex(@"([0-9]+(:[0-9]+)+)\s.*\s\(\s.*\s[a-zA-Z]+\)", RegexOptions.IgnoreCase);
        public static readonly Regex NMLPrefix = new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\]\s\[[^\]]*\]", RegexOptions.IgnoreCase);

        public static int IndexOfNth(string str, char c, int n)
        {
            int s = -1;
            for (int i = 0; i < n; i++)
            {
                s = str.IndexOf(c, s + 1);
                if (s == -1) break;
            }
            return s;
        }

        public static (string Trimmed, Version? Parsed) TryGetVersionFromSentence (string input)
        {
            Version v;
            string s = Regex.Replace(input.Split(' ')[1], "[^0-9.]", "");
            if (Version.TryParse(s, out v))
            {
                if (v is not null)
                    return (s, v);
                else
                    Console.WriteLine("Bad version format detected");
            }
            return (s, NullVersion);
        }

        public static Version TryGetVersionFromWord (string input)
        {
            Version v;
            if (Version.TryParse(input, out v))
            {
                if (v is not null)
                    return v;
                else
                    Console.WriteLine("Bad version format detected");
            }
            return NullVersion;
        }
    }
}
