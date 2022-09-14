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
        public Version NMLVersion;
        public string NMLVersionFallback;

        public NeosModLoaderStatus()
        {
            IsNMLPresent = false;
            IsNMLLoaded = false;
            NMLVersion = Utils.NullVersion;
            NMLVersionFallback = string.Empty;
        }
    }
}
