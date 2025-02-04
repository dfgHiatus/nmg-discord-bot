using Discord;
using Discord.WebSocket;
using NMGDiscordBot.Managers;

namespace NMGDiscordBot
{
	internal static class Util
	{
		public static string FormatedName(this IUser user) => $"{user.Username} # {user.Discriminator} ({user.Id})";
		public static string FormatedName(this ISocketMessageChannel channel) => $"{channel.Name} (st{channel.Id})";
		public static string FormatedName(this IGuild guild) => $"{guild.Name} ({guild.Id})";
		public static string FormatedName(this IChannel Channel) => $"{Channel.Name} ({Channel.Id})";
		public static string GuildNameFromId(ulong GuildId) => GuildFromId(GuildId).Name;
		public static SocketGuild GuildFromId(ulong GuildId) => BotManager.client.Guilds.First((e) => GuildId == e.Id);
		public static SocketGuild? GuildFromChannel(this ISocketMessageChannel channel) => (channel as SocketGuildChannel)?.Guild;
		public static SocketTextChannel ChannelFromId(this SocketGuild guild, ulong ChannelId) => guild.TextChannels.First((e) => ChannelId == e.Id);
		public static string OptionsToString(this IReadOnlyCollection<IApplicationCommandInteractionDataOption> options)
		{
			string output = "";
			foreach (var cmd in options) output += $"[ {cmd.Type} {cmd.Name}:{cmd.AutoFormatedValue()} {cmd.Options.OptionsToString()}] ";
			return output;
		}
		public static string AutoFormatedValue(this IApplicationCommandInteractionDataOption oacodo)
		{
			switch (oacodo.Type)
			{
				case ApplicationCommandOptionType.User:
					return ((IUser)oacodo.Value).FormatedName();
				case ApplicationCommandOptionType.Channel:
					return ((IChannel)oacodo.Value).FormatedName();
				case ApplicationCommandOptionType.Mentionable:
					return (oacodo.Value is IUser) ? ((IUser)oacodo.Value).FormatedName() : oacodo.Value.ToString();
				default:
					return oacodo.Value.ToString();
			}
		}
		public static ulong? NullableUlongParse(string? u) => u == null ? null : ulong.Parse(u); //there is probably a better way to do this
		public static async Task<string> GetString(string url)
		{
			using (HttpClient client = new())
			{
				client.DefaultRequestHeaders.Add("User-Agent", "nmg-discord-bot");
				return await client.GetStringAsync(url);
			}
		}

		public static string LenCap(this string str, int cap)
		{
			if (str == null) return null;
			return str.Length > cap ? str.Substring(0, cap) : str;
		}

		public static string ConcatStrs(this IEnumerable<string> strings, string seporator = " ")
		{
			string o = "";
			foreach (string str in strings)
			{
				o += str + seporator;
			}
			if (strings.Count() > 0) o = o.Substring(0, o.Length - seporator.Length);
			return o;
		}
	}
}