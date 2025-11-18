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

			using HttpClientHandler httpClientHandler = new() { ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; } };
			using HttpClient httpClient = new(httpClientHandler);
			Tesera.TeseraClient.Instance = new(httpClient);

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
				Type? type = typeof(Program).Assembly.GetType(string.Format(name, "Commands"), false, true)
					?? typeof(Program).Assembly.GetType(string.Format(name, "BotCommands"), false, true);

				if (type is null)
					return;

				List<object?> parameters = [..args[1..]];
				List<Type> types = [];
				void UpdateTypes()
				{
					if (types.Count > 0)
						types.Clear();

					types.Capacity = parameters.Count;
					foreach (object? item in parameters)
						if (item is not null)
							types.Add(item.GetType());
				}
				UpdateTypes();

				const string METHOD_NAME = "Respond";
				const string ASYNC_METHOD_NAME = METHOD_NAME + "Async";
				MethodInfo? methodInfo = type.GetMethod(METHOD_NAME, [..types])
					?? type.GetMethod(ASYNC_METHOD_NAME, [typeof(ITelegramBotClient), ..types, cancellationTokenSource.Token.GetType()]);

				if (methodInfo is null)
				{
					parameters.Clear();
					parameters.Add(args[1..]);
					UpdateTypes();

					methodInfo = type.GetMethod(METHOD_NAME, [..types])
						?? type.GetMethod(ASYNC_METHOD_NAME, [typeof(ITelegramBotClient), ..types, cancellationTokenSource.Token.GetType()]);
				}

				if (methodInfo is null)
					return;

				if (methodInfo.Name.Equals(ASYNC_METHOD_NAME))
				{
					parameters.Insert(0, botClient);
					parameters.Add(cancellationTokenSource.Token);
				}					

				if (methodInfo.Invoke(null, [..parameters]) is Task task)
					await task;
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