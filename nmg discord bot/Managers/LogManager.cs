using Discord.WebSocket;

namespace NMGDiscordBot.Managers
{
	internal static class LogManager
	{
		internal static void SetUp()
		{
			BotManager.client.SlashCommandExecuted += SlashCommandHandler;
			BotManager.client.SelectMenuExecuted += Client_SelectMenuExecuted;
		}

		private static async Task SlashCommandHandler(SocketSlashCommand command) =>
			LogWrapper.Log($"{command.User.FormatedName()} ran SlashCommand: {command.Data.Name}{(command.Data.Options.Count > 0 ? " with options " + command.Data.Options.OptionsToString() : " ")}in {command.Channel.Name} ({command.Channel.Id}) {(command.GuildId.HasValue ? Util.GuildNameFromId(command.GuildId.Value) : "")} ({command.GuildId})");


		private static async Task Client_SelectMenuExecuted(SocketMessageComponent component) =>
			LogWrapper.Log($"{component.User.FormatedName()} interacted with MessageComponent CustomId: {component.Data.CustomId} Type: {component.Data.Type} Value: {component.Data.Value} Values: {component.Data.Values.ConcatStrs()} on {component.Message.Id} in {component.Channel.Name} ({component.Channel.Id}) {(component.GuildId.HasValue ? Util.GuildNameFromId(component.GuildId.Value) : "")} ({component.GuildId})");
	}
}
