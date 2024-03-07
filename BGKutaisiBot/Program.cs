using BGKutaisiBot.Types.Exceptions;
using BGKutaisiBot.Types.Logging;
using BGKutaisiBot.UI.Commands;
using Telegram.Bot;

namespace BGKutaisiBot
{
    internal class Program
	{
		static async Task Main(string[] args)
		{
			ITelegramBotClient? botClient = null;
			UI.AllCommands uiCommands = new(() => botClient, (newBotClient) => botClient = newBotClient);

			async void ExecuteCommand(string line)
			{
				string[] lineSplitted = line.Split(' ');
				if (lineSplitted.Length > 0 && uiCommands.ContainsCommand(lineSplitted[0]))
				{
					string commandName = lineSplitted[0];
					Array.Copy(lineSplitted, 1, lineSplitted, 0, lineSplitted.Length - 1);
					Array.Resize<string>(ref lineSplitted, lineSplitted.Length - 1);

					string? testChatIdAlias = Environment.GetEnvironmentVariable("TEST_CHAT_ID_ALIAS");
					if (lineSplitted.Length > 0 && !string.IsNullOrEmpty(testChatIdAlias))
						for (int i = 0; i < lineSplitted.Length; i++)
							if (lineSplitted[i] == testChatIdAlias)
								lineSplitted[i] = Environment.GetEnvironmentVariable("TEST_CHAT_ID")?.ToString() ?? lineSplitted[i];

					await uiCommands.TryExecuteAsync(commandName, lineSplitted);
				}
			}
			BotCommands.Admin.CommandCallback = ExecuteCommand;

			bool start = true;
			while (true)
			{
				try
				{
					if (start)
					{
						start = false;
						switch (args.Length)
						{
							case 0:
								await uiCommands.TryExecuteAsync("help", []);
								break;
							case <= 2 when args[0].Equals(typeof(StartBot).Name, StringComparison.OrdinalIgnoreCase):
								Console.WriteLine($"{typeof(StartBot).Name.ToLower()}");
								await uiCommands.TryExecuteAsync(typeof(StartBot).Name, args.Length == 1 ? [] : [args[1]]);
								break;
						}
					}

					string? line = Console.ReadLine()?.Trim();
					if (!string.IsNullOrEmpty(line))
						ExecuteCommand(line);
				}
				catch (ExitException) { break; }
				catch (Exception e) { Logs.Instance.AddError(e); }
			}
		}
	}
}