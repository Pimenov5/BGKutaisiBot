using BGKutaisiBot.Types.Exceptions;
using BGKutaisiBot.Types.Logging;
using BGKutaisiBot.Commands;
using System.Reflection;
using Telegram.Bot;
using System.Text;

namespace BGKutaisiBot
{
    internal class Program
	{
		static async Task Main(string[] args)
		{
			ITelegramBotClient? botClient = null;
			StartBot.OnBotStartedEvent += (Type type, ITelegramBotClient newBotClient) => botClient = newBotClient;
			using CancellationTokenSource cancellationTokenSource = new();

			async Task ExecuteCommand(string line)
			{
				string[] lineSplitted = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (lineSplitted.Length == 0)
					return;

				string? testChatIdAlias = Environment.GetEnvironmentVariable("TEST_CHAT_ID_ALIAS");
				if (!string.IsNullOrEmpty(testChatIdAlias))
					for (int i = 0; i < lineSplitted.Length; i++)
						if (lineSplitted[i] == testChatIdAlias)
							lineSplitted[i] = Environment.GetEnvironmentVariable("TEST_CHAT_ID") ?? lineSplitted[i];

				Type? type = typeof(Program).Assembly.GetType(typeof(Program).Namespace + ".Commands." + lineSplitted[0], false, true);
				if (type is null)
					return;

				lineSplitted = lineSplitted[1..];
				Type[] types = new Type[lineSplitted.Length + 2];
				types[0] = typeof(ITelegramBotClient);
				types[types.Length - 1] = typeof(CancellationToken);
				for (int i = 1; i < types.Length - 1; i++)
					types[i] = typeof(string);

				MethodInfo? methodInfo = type?.GetMethod("RespondAsync", types) ?? type?.GetMethod("Respond", []);
				if (methodInfo is null)
					return;

				List<object?> parameters = [botClient];
				parameters.AddRange(lineSplitted);
				parameters.Add(cancellationTokenSource.Token);

				if (methodInfo.Name == "RespondAsync") {
					if (methodInfo.Invoke(null, parameters.ToArray()) is Task task)
						await task;
				}
				else
					methodInfo.Invoke(null, []);
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
						if (args.Length > 0)
						{
							string commandLine = new StringBuilder().AppendJoin(' ', args).ToString();
							Console.WriteLine(commandLine);
							await ExecuteCommand(commandLine);
						}
					}

					string? line = Console.ReadLine()?.Trim();
					if (!string.IsNullOrEmpty(line))
						await ExecuteCommand(line);
				}
				catch (TargetInvocationException e) when (e.InnerException is ExitException) { break; }
				catch (Exception e) { Logs.Instance.AddError(e); }
			}
		}
	}
}