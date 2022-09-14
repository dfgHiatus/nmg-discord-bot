namespace NMGDiscordBot.Tests
{
    internal class NeosMod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public Version RichVersion { get; set; }
        public string Link { get; set; }

        public NeosMod()
        {
            Name = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            RichVersion = Utils.NullVersion;
            Link = string.Empty;
        }
    }
}
