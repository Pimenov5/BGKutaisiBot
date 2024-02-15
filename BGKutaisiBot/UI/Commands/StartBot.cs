using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace BGKutaisiBot.UI.Commands
{
	internal class StartBot : BotCommand
	{
		readonly Lazy<CancellationTokenSource> _lazyCTS = new();

		public StartBot(Func<ITelegramBotClient?> getBotClient, Action<ITelegramBotClient, CancellationTokenSource> onBotStarted) : base("запустить бота", getBotClient)
		{
			async Task Function(string[] args)
			{
				if (this.GetBotClient() is not null)
					throw new InvalidOperationException("Бот уже был запущен");

				TelegramBotClient botClient = new(args[0]);
				if (!await botClient.TestApiAsync(_lazyCTS.Value.Token))
					throw new ArgumentException($"Токен {args[0]} бота не прошёл проверку API");

				List<Telegram.Bot.Types.BotCommand> botCommands = [];
				IEnumerable<Type> types = this.GetType().Assembly.GetTypes().Where((Type type) => type.IsSubclassOf(typeof(Types.BotCommand)));
				foreach (Type type in types)
					if (type.GetProperty("Description") is { } property && property.GetValue(null) is { } propertyValue && propertyValue is string description)
						botCommands.Add(new Telegram.Bot.Types.BotCommand() { Command = type.Name.ToLower(), Description = description });

				await botClient.SetMyCommandsAsync(botCommands);
				botClient.StartReceiving(Types.TelegramUpdateHandler.HandleUpdateAsync, HandlePollingErrorAsync, new ReceiverOptions { AllowedUpdates = [] }, _lazyCTS.Value.Token);

				User user = await botClient.GetMeAsync();
				Logs.Instance.Add($"@{user.Username} запущен", true);
				onBotStarted.Invoke(botClient, _lazyCTS.Value);

				Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
				{
					Logs.Instance.AddError(exception);
					return Task.CompletedTask;
				}
			}

			string? telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
			if (!string.IsNullOrEmpty(telegramBotToken))
				this.Add(0, (args) => Function([telegramBotToken]));
			this.Add(1, Function);
		} 
	}
}