using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace BGKutaisiBot.UI.Commands
{
	internal class StartBot : BotCommand
	{
		readonly Dictionary<long, Types.Command> _chats = [];
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

				botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, new ReceiverOptions { AllowedUpdates = [] }, _lazyCTS.Value.Token);

				User user = await botClient.GetMeAsync();
				Logs.Instance.Add($"@{user.Username} запущен", true);
				onBotStarted.Invoke(botClient, _lazyCTS.Value);

				async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
				{
					if (update.CallbackQuery is { } callbackQuery && callbackQuery.Data is { } callbackData)
					{
						Logs.Instance.Add($"@{callbackQuery.From.Username} нажал \"{callbackQuery.Data}\" в сообщении с ID {callbackQuery.Message?.MessageId}", System.Diagnostics.Debugger.IsAttached);

						Type? type = Types.Command.TryParseCallbackData(callbackData, out KeyValuePair<string, string>? parsedCallbackData)
							? this.GetType().Assembly.GetType($"{this.GetType().Namespace?.Replace("UI.", "")}.{parsedCallbackData?.Key}") : null;
						object? response = type?.GetMethod(parsedCallbackData?.Value)?.Invoke(null, [callbackQuery.Message?.Text]);
						if (response is TextMessage textMessage)
						{
							await this.BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
							await this.SendTextMessageAsync(callbackQuery.From.Id, textMessage);
						}
						else
							await this.BotClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Не удалось обработать нажатие \"{callbackData}\"", true);
					}

					if (update.Message is not { } message)
						return;

					long chatId = message.Chat.Id;
					Logs.Instance.Add($"@{message.From?.Username}: {(message.Text ?? $"[{message?.Type.ToString()}]")}");

					if (message?.Type == Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft || message?.Text is not { } messageText)
						return;

					Types.Command? command = null;
					messageText = messageText.Trim();
					if (messageText.StartsWith('/'))
					{
						string commandName = messageText[1..(messageText.Contains(' ') ? messageText.IndexOf(' ') : messageText.Length)];
						if (commandName == "cancel")
						{
							if (_chats.TryGetValue(chatId, out command))
								await this.BotClient.SendTextMessageAsync(chatId, $"Отменено выполнение команды /{command.GetType().Name.ToLower()}");
							_chats.Remove(chatId);
							return;
						}

						Type? type = this.GetType().Assembly.GetType($"{this.GetType().Namespace?.Replace("UI.", "")}.{commandName}", false, true);
						if (type is null)
						{
							await this.BotClient.SendTextMessageAsync(chatId, $"\"{commandName}\" не является командой");
							return;
						}
						
						_chats.Remove(chatId);
						_chats.Add(chatId, (Types.Command)(type.GetConstructor([])?.Invoke([]) ?? throw new NullReferenceException($"Не удалось создать объект класса {type.FullName}")));
						messageText = messageText.Replace($"/{commandName}", "").TrimStart();
					}

					if (_chats.TryGetValue(chatId, out command))
					{
						TextMessage response = command.Respond(messageText, out bool finished);
						if (finished)
							_chats.Remove(chatId);

						await this.SendTextMessageAsync(chatId, response);
					}
					else
						await this.BotClient.SendTextMessageAsync(chatId, $"\"{messageText}\" не является командой или ответом на выполняемую команду");
				}

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