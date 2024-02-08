using BGKutaisiBot.Types;
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

				ITelegramBotClient botClient = new TelegramBotClient(args[0]);
				botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, new ReceiverOptions { AllowedUpdates = [] }, _lazyCTS.Value.Token);

				User user = await botClient.GetMeAsync();
				Logs.Instance.Add($"@{user.Username} запущен", true);
				onBotStarted.Invoke(botClient, _lazyCTS.Value);

				async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
				{
					if (update.Message is not { } message)
						return;

					long chatId = message.Chat.Id;
					Logs.Instance.Add($"@{message?.From?.Username}: {(message?.Text ?? $"[{message?.Type.ToString()}]")}");

					if (message?.Type != Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft)
						await botClient.SendTextMessageAsync(chatId, "Извините, чат-бот пока не обрабатывает входящие сообщения", cancellationToken: cancellationToken);
				}

				Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
				{
					Logs.Instance.AddError(exception);
					return Task.CompletedTask;
				}
			}

			this.Add(0, (args) => Function([Configuration.Instance.BotToken]));
			this.Add(1, Function);
		} 
	}
}