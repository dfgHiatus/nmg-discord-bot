using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NMGDiscordBot.Tests
{
    internal static class Utils
    {
        public static readonly Regex startsWithTimePrefix = new Regex(@"([0-9]+(:[0-9]+)+)\s.*\s\(\s.*\s[a-zA-Z]+\)", RegexOptions.IgnoreCase);
        public static readonly Regex startsWithNMLPrefix = new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\]\s\[[^\]]*\]", RegexOptions.IgnoreCase);
    }
}
