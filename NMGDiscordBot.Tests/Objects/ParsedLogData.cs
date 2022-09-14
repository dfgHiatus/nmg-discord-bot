using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMGDiscordBot.Tests
{
    internal class ParsedLogData
    {
        public HashSet<NeosPlugin> PresentPlugins;
        public HashSet<NeosMod> PresentMods;
        public NeosModLoaderStatus NMLStatus;
        public OperatingSystem OperatingSystem;

        public ParsedLogData()
        {
            PresentPlugins = new();
            PresentMods = new();
            NMLStatus = new();
            OperatingSystem = new(PlatformID.Other, Utils.NullVersion);
        }
    }
}
