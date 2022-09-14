using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMGDiscordBot.Tests
{
    internal class ParsedLogData
    {
        public HashSet<ParsedNeosMod> ParsedNeosMods;
        public ParsedNeosModLoaderStatus ParsedNeosModLoaderStatus;
        public OperatingSystem ParsedOperatingSystem;
        public ParsedLogData() { }
        public ParsedLogData(HashSet<ParsedNeosMod> parsedNeosMods, ParsedNeosModLoaderStatus parsedNeosModLoaderStatus, OperatingSystem parsedOperatingSystem)
        {
            ParsedNeosMods = parsedNeosMods;
            ParsedNeosModLoaderStatus = parsedNeosModLoaderStatus;
            ParsedOperatingSystem = parsedOperatingSystem;
        }
    }
}
