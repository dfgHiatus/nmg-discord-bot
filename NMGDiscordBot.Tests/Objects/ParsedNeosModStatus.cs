using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMGDiscordBot.Tests
{
    internal class NeosModLoaderStatus
    {
        public bool IsNMLPresent = false;
        public bool IsNMLLoaded = false;
        public bool Is0HarmonyPresent = false;
        public Version NMLVersion;
        public Version HarmonyVersion;
        public string NMLVersionFallback;
        public string HarmonyVersionFallback;

        public NeosModLoaderStatus()
        {
            IsNMLPresent = false;
            IsNMLLoaded = false;
            Is0HarmonyPresent = false;
            NMLVersion = Utils.NullVersion;
            HarmonyVersion = Utils.NullVersion;
            NMLVersionFallback = string.Empty;
            HarmonyVersionFallback = string.Empty;
        }
    }
}
