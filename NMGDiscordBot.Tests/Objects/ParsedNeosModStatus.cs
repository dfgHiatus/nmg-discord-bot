using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMGDiscordBot.Tests
{
    internal class ParsedNeosModLoaderStatus
    {
        public Version ParsedVersion { get; set; }
        public bool IsLoaded { get; set; }

        public ParsedNeosModLoaderStatus(Version parsedVersion, bool isLoaded)
        {
            ParsedVersion = parsedVersion;
            IsLoaded = isLoaded;
        }
    }
}
