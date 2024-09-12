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

			async Task ExecuteCommand(string[] args)
			{
				if (args.Length == 0)
					return;

				string? testChatIdAlias = Environment.GetEnvironmentVariable("TEST_CHAT_ID_ALIAS");
				if (!string.IsNullOrEmpty(testChatIdAlias))
					for (int i = 0; i < args.Length; i++)
						if (args[i] == testChatIdAlias)
							args[i] = Environment.GetEnvironmentVariable("TEST_CHAT_ID") ?? args[i];

				string name = typeof(Program).Namespace + ".{0}." + args[0];
				Type? type = typeof(Program).Assembly.GetType(string.Format(name, "Commands"), false, true);
				if (type is null)
				{
					type = typeof(Program).Assembly.GetType(string.Format(name, "BotCommands"), false, true);
					if (type is null || !(type.GetInterfaces().Contains(typeof(IConsoleCommand)) || type.GetInterfaces().Contains(typeof(IAsyncConsoleCommand))))
						return;
				}

				bool isAsyncCommand = type.GetInterfaces().Contains(typeof(IAsyncConsoleCommand));

				args = args[1..];
				Type[] types = new Type[args.Length + (isAsyncCommand ? 2 : 0)];
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
				parameters.AddRange(args);
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
							await ExecuteCommand(Command.Split(commandLine));
						}
					}

					string? line = Console.ReadLine()?.Trim();
					await ExecuteCommand(Command.Split(line ?? string.Empty));
				}
				catch (TargetInvocationException e) when (e.InnerException is ExitException) { break; }
				catch (Exception e) { Logs.Instance.AddError(e); }
			}
		}
	}
}