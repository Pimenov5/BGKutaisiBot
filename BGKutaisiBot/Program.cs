using BGKutaisiBot.Types;
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

			bool start = true;
			while (true)
			{
				try
				{
					if (start)
					{
						start = false;
						await uiCommands.TryExecuteAsync("help", []);
						Console.WriteLine($"readconfig {ReadConfig.FILE_NAME}");
						await uiCommands.TryExecuteAsync("readconfig", []);
					}

					string? line = Console.ReadLine()?.Trim();
					if (!String.IsNullOrEmpty(line))
					{
						string[] lineSplitted = line.Split(' ');
						if (lineSplitted.Length > 0 && uiCommands.ContainsCommand(lineSplitted[0]))
						{
							string commandName = lineSplitted[0];
							Array.Copy(lineSplitted, 1, lineSplitted, 0, lineSplitted.Length - 1);
							Array.Resize<string>(ref lineSplitted, lineSplitted.Length - 1);

							if (lineSplitted.Length > 0 && !string.IsNullOrEmpty(Configuration.Instance.TestChatIdAlias))
								for (int i = 0; i < lineSplitted.Length; i++)
									if (lineSplitted[i] == Configuration.Instance.TestChatIdAlias)
										lineSplitted[i] = Configuration.Instance.TestChatId?.ToString() ?? lineSplitted[i];

							await uiCommands.TryExecuteAsync(commandName, lineSplitted);
						}
					}
				}
				catch (Types.ExitException) { break; }
				catch (Exception e) { Logs.Instance.AddError(e); }
			}
		}
	}
}