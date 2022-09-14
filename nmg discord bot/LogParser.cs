using System.Text.RegularExpressions;

namespace NMGDiscordBot
{
	public class LogParser
	{
		// The idea being if a Regex matches a line we can perform certain operations on a it. I love brainstorming.

		// Regex produced by https://regex-generator.olafneumann.org
		// - Regex can be slow
		// - Will other locales/languages throw this off?

		// We might not need Regex for the following, IE some of these are hard-coded into every log/
		// The real question is, what is it we're after that we're parsing?
		// - Presence: (Argument: Libraries\NeosModLoader.dll)
		// - Version: ([INFO] [NeosModLoader] NeosModLoader {v1.11.0} starting up!)
		// - Exceptions
		// - Loaded mods: ([INFO] [NeosModLoader] loaded mod...)
		// --- Made it anyways kek: new Regex(@"\[(INFO)\].*\[(NeosModLoader)\].*\[.*\].*\(.*\.(dll)+\)", RegexOptions.IgnoreCase);

		// This might be the only actual good use case, identifying if a log is "valid" based on its name.
		// Ex DESKTOP-4GOD64O - 2022.1.28.1335 - 2022-09-12 20_41_36.log
		// Look for [2022.1.28.1335, 2022-09-12, 20_41_36.log]
		private static readonly Regex validLogName = new Regex(@"([0-9]+(\.[0-9]+)+) - [0-9]{4}-[0-9]{2}-[0-9]{2} ([0-9]+(_[0-9]+)+)\.[a-zA-Z]+", RegexOptions.IgnoreCase);
		
		public LogParser()
		{

		}
	}
}
