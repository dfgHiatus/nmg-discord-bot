using System.Security.Cryptography;

namespace NMGDiscordBot.Tests
{
    internal class NeosMod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string RawVersion { get; set; }
        public Version Version { get; set; }
        public string SHA_256 { get; set; }
        public string Link { get; set; }

        public NeosMod()
        {
            Name = string.Empty;
            Author = string.Empty;
            RawVersion = string.Empty;
            Version = Utils.NullVersion;
            Link = string.Empty;
            SHA_256 = string.Empty;
        }
    }
}
