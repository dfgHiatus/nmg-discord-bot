using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NMGDiscordBot.Tests
{
    internal class ParsedNeosMod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public Version RichVersion { get; set; }
        public string Link { get; set; }

        public ParsedNeosMod(string name, string author, string version, Version richVersion, string link)
        {
            Name = name;
            Author = author;
            Version = version;
            RichVersion = richVersion;
            Link = link;
        }
    }
}
