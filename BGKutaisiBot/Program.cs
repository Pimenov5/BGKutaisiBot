using BGKutaisiBot.Types.Exceptions;
using BGKutaisiBot.Types.Logging;
using BGKutaisiBot.Commands;
using System.Reflection;
using Telegram.Bot;
using BGKutaisiBot.Types;

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

				string name = typeof(Program).Namespace + ".{0}." + lineSplitted[0];
				Type? type = typeof(Program).Assembly.GetType(string.Format(name, "Commands"), false, true);
				if (type is null)
				{
					type = typeof(Program).Assembly.GetType(string.Format(name, "BotCommands"), false, true);
					if (type is null || !(type.GetInterfaces().Contains(typeof(IConsoleCommand)) || type.GetInterfaces().Contains(typeof(IAsyncConsoleCommand))))
						return;
				}

				bool isAsyncCommand = type.GetInterfaces().Contains(typeof(IAsyncConsoleCommand));

				lineSplitted = lineSplitted[1..];
				Type[] types = new Type[lineSplitted.Length + (isAsyncCommand ? 2 : 0)];
				if (isAsyncCommand) 
				{
					types[0] = typeof(ITelegramBotClient);
					types[^1] = typeof(CancellationToken);
				}
				for (int i = (isAsyncCommand ? 1 : 0); i < types.Length - (isAsyncCommand ? 1 : 0); i++)
					types[i] = typeof(string);

				MethodInfo? methodInfo = isAsyncCommand ? type?.GetMethod("RespondAsync", types) : type?.GetMethod("Respond", types);
				if (methodInfo is null)
					return;

				List<object?> parameters = isAsyncCommand ? [botClient] : [];
				parameters.AddRange(lineSplitted);
				if (isAsyncCommand)
					parameters.Add(cancellationTokenSource.Token);

				if (isAsyncCommand) {
					if (methodInfo.Invoke(null, parameters.ToArray()) is Task task)
						await task;
				}
				else
					methodInfo.Invoke(null, parameters.ToArray());
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
						foreach (string commandLine in args)
						{
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