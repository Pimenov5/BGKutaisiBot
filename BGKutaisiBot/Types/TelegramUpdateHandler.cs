using BGKutaisiBot.BotCommands;
using BGKutaisiBot.Types.Exceptions;
using BGKutaisiBot.Types.Logging;
using System.Reflection;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BGKutaisiBot.Types
{
    internal class TelegramUpdateHandler : IUpdateHandler
	{
		static readonly Dictionary<long, BotCommand> _chats = [];
		static Type? GetTypeByName(string? typeName, bool ignoreCase = false)
		{
			string name = typeof(TelegramUpdateHandler).Namespace?.Replace("Types", "BotCommands.") + typeName;
			return typeof(TelegramUpdateHandler).Assembly.GetType(name, false, ignoreCase);
		}

		async static Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string callbackData, CancellationToken cancellationToken)
		{
			Logs.Instance.Add($"@{callbackQuery.From.Username} нажал \"{callbackQuery.Data}\" в сообщении с ID {callbackQuery.Message?.MessageId}", System.Diagnostics.Debugger.IsAttached);
			await botClient.SendChatActionAsync(callbackQuery.From.Id, ChatAction.Typing, cancellationToken: cancellationToken);

			Type? type = BotCommand.TryParseCallbackData(callbackData, out string? typeName, out string? methodName, out string[]? args) ? GetTypeByName(typeName) : null;
			MethodInfo? methodInfo = methodName is null ? null : type?.GetMethod(methodName);

			string? reason = null;
			object? response = null;
			try
			{
				object?[]? parameters = args is not null && args.Length != 0 ? args : methodInfo?.GetParameters().Length == 1 && callbackQuery.Message?.Text is string text ? [text] : null;
				response = methodInfo?.Invoke(null, parameters);
			}
			catch (CancelException e)
			{
				reason = e.Reason;
			}


			if (response is TextMessage textMessage)
			{
				try
				{
					await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
				}
				catch (Exception e)
				{
					if (e is not ApiRequestException)
						throw;
				}

				textMessage.CancellationToken = cancellationToken;
				await textMessage.SendTextMessageAsync(callbackQuery.From.Id, botClient);
			}
			else
			{
				if (type is null || methodInfo is null)
					reason = "не удалось выделить или найти обработчик";
				await botClient.AnswerCallbackQueryAsync(callbackQuery.Id,
					$"Отсутствует результат нажатия \"{callbackData}\"{(reason is null ? string.Empty : ". Причина:" + reason)}", true, cancellationToken: cancellationToken);
			}
		}

		static async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, string messageText, CancellationToken cancellationToken) {
			Logs.Instance.Add($"@{message.From?.Username}: {(message.Text ?? $"[{message.Type}]")}");
			if (message.Type == MessageType.ChatMemberLeft)
				return;

			long chatId = message.Chat.Id;
			if (messageText == ROLL_DICE_KEYBOARD_TEXT)
			{
				await botClient.SendDiceAsync(chatId, cancellationToken: cancellationToken);
				return;
			}

			BotCommand? prevCommand = null, command = null;
			if (messageText.StartsWith('/'))
			{
				string commandName = messageText[1..(messageText.Contains(' ') ? messageText.IndexOf(' ') : messageText.Length)];
				Type? type = GetTypeByName(commandName, true);
				if (type is null || !type.IsSubclassOf(typeof(BotCommand)))
				{
					await botClient.SendTextMessageAsync(chatId, $"\"{commandName}\" не является командой", cancellationToken: cancellationToken);
					return;
				}

				_chats.TryGetValue(chatId, out prevCommand);
				if (prevCommand is not null)
					_chats.Remove(chatId);

				command = (BotCommand)(type.GetConstructor([])?.Invoke([]) ?? throw new NullReferenceException($"Не удалось создать объект класса {type.FullName}"));
				if (command is BotForm)
					_chats.Add(chatId, command);
			}

			if (command is not null || _chats.TryGetValue(chatId, out command))
			{
				List<object> parameters = [];
				List<Type> types = new(parameters.Count);
				Type commandType = command.GetType();

				MethodInfo? GetMethodInfo(Action<List<object>>? callback)
				{
					callback?.Invoke(parameters);

					types.Clear();
					types.Capacity = parameters.Count;
					foreach (object item in parameters)
						types.Add(item.GetType());

					const string METHOD_NAME = "Respond";
					const string ASYNC_METHOD_NAME = METHOD_NAME + "Async";
					return commandType.GetMethod(METHOD_NAME, [..types]) ?? commandType.GetMethod(ASYNC_METHOD_NAME, [..types]);
				}

				MethodInfo? methodInfo = GetMethodInfo(null); // Respond()
				if (methodInfo is null)
				{
					string[] args = command.GetArguments(message);
					methodInfo ??= GetMethodInfo((List<object> parameters) => parameters.Add(args))  // Respond(string[] args)
						?? GetMethodInfo((List<object> parameters) => parameters.Insert(0, botClient)); // Respond(ITelegramBotClient botClient, string[] args)
				}

				if (methodInfo is null)
					throw new NullReferenceException($"Не удалось вызвать /{command.GetType().Name.ToLower()}");

				if (command.IsLong)
					await botClient.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken: cancellationToken);

				TextMessage? response = null;
				try
				{
					object? result = methodInfo.Invoke(command, [..parameters]);
					response = result switch
					{
						null => null,
						TextMessage => (TextMessage)result,
						string => new TextMessage((string)result),
						Task<string> => new TextMessage(await (Task<string>)result),
						Task<TextMessage> => await (Task<TextMessage>)result,
						Task => null,
						_ => throw new InvalidCastException($"Неизвестный тип результата: {result.GetType().Name}"),
					};

					if (result is Task task && !task.IsCompleted)
						await task;
				}
				catch (Exception exception)
				{
					if (exception.InnerException is CancelException e)
					{
						string? text = e.Cancelling switch
						{
							CancelException.Cancel.Previous when prevCommand is not null => $"Выполнение /{prevCommand.GetType().Name.ToLower()} отменено",
							CancelException.Cancel.Current => $"Выполнение /{command.GetType().Name.ToLower()} отменено",
							_ => null
						};

						if (!string.IsNullOrEmpty(text))
						{
							if (!string.IsNullOrEmpty(e.Reason))
								text = $"{text}. Причина: {e.Reason}";
							response = new(text, true);
						}
					}
					else if (exception.InnerException is RollDiceException diceException)
					{
						for (int i = 0; i < diceException.Count; i++)
							await botClient.SendDiceAsync(chatId, cancellationToken: cancellationToken);
					}
					else
						throw;
				}

				if (command is BotForm botForm && botForm.IsCompleted)
					_chats.Remove(chatId);
				if (response is not null)
				{
					response.CancellationToken = cancellationToken;
					await response.SendTextMessageAsync(chatId, botClient);
				}
			}
			else
				await new TextMessage("Не является командой или ответом на выполняемую команду") { ReplyToMessageId = message.MessageId, 
					CancellationToken = cancellationToken }.SendTextMessageAsync(chatId, botClient);
		}

		static async Task HandleMessageWithPollAsync(ITelegramBotClient botClient, Message message, Poll poll, CancellationToken cancellationToken)
		{
			Logs.Instance.Add($"@{message.From?.Username}: [{poll.Question}]");

			Dictionary<string, int> options = [];
			foreach (var item in poll.Options)
				if (item.VoterCount != 0)
					options.Add(item.Text.Contains('+') ? item.Text.Remove(item.Text.IndexOf('+')).TrimEnd() : item.Text, item.VoterCount);

			long chatId = message.Chat.Id;
			if (options.Count == 0)
			{
				await botClient.SendTextMessageAsync(chatId, "В опросе ещё никто не проголосовал", cancellationToken: cancellationToken);
				return;
			}
			
			List<KeyValuePair<string, int>> optionsList = options.ToList();
			optionsList.Sort((KeyValuePair<string, int> x, KeyValuePair<string, int> y) => y.Value.CompareTo(x.Value));

			StringBuilder stringBuilder = new();
			stringBuilder.AppendLine("Количество голосов: " + poll.TotalVoterCount);
			optionsList.ForEach((KeyValuePair<string, int> item) => stringBuilder.AppendLine(item.ToString()));
			await new TextMessage(stringBuilder.ToString()) { CancellationToken = cancellationToken }.SendTextMessageAsync(chatId, botClient);
		}

		public static string ROLL_DICE_KEYBOARD_TEXT = "🎲";

		public delegate Task NotPrivateTextMessageHandler(Type type, ITelegramBotClient botClient, Message message, string messageText, CancellationToken cancellationToken);
		public static event NotPrivateTextMessageHandler? NotPrivateTextMessageEvent;

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			try
			{
				switch (update.Type)
				{
					case UpdateType.Message:
					case UpdateType.EditedMessage:
						Message? message = update.Message ?? update.EditedMessage;
						if (message is null)
							break;

						if (message.Text is string messageText) {
							if (message.Chat.Type is ChatType.Private)
								await HandleMessageAsync(botClient, message, messageText, cancellationToken);
							else if (NotPrivateTextMessageEvent is not null)
								await NotPrivateTextMessageEvent(typeof(TelegramUpdateHandler), botClient, message, messageText, cancellationToken);
						} else if (message.Poll is Poll messagePoll && Admin.Contains(message.Chat.Id))
							await HandleMessageWithPollAsync(botClient, message, messagePoll, cancellationToken);
						break;

					case UpdateType.CallbackQuery when update.CallbackQuery is CallbackQuery callbackQuery && callbackQuery.Data is string callbackData:
						await HandleCallbackQueryAsync(botClient, callbackQuery, callbackData, cancellationToken);
						break;

					default:
						Logs.Instance.Add(update.Type.ToString());
						break;
				}
			}
			catch (Exception e)
			{
				Logs.Instance.AddError(e);
			}
		}
		public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			Logs.Instance.AddError(exception);
			return Task.CompletedTask;
		}
	}
}