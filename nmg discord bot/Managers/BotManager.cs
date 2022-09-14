using Discord;
using Discord.WebSocket;
using NMGDiscordBot.Commands;

namespace NMGDiscordBot.Managers
{
	internal static class BotManager
	{
		static internal readonly DiscordSocketClient client = new();
		public static async Task MainAsync()
		{
			client.Log += async (msg) =>
			{
				Console.WriteLine(msg.ToString());
				await Task.CompletedTask;
			};

			var token = Environment.GetEnvironmentVariable("nmg_bot_token", EnvironmentVariableTarget.User);

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();


			LogManager.SetUp();
			ExtCmds.SetUp();
			SearchCmd.SetUp();
			ManifestManager.SetUp();
			// Block this task until the program is closed.
			await Task.Delay(-1);
		}
	}
}