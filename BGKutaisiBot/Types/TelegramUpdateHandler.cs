using BGKutaisiBot.Types.Exceptions;
using BGKutaisiBot.Types.Logging;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BGKutaisiBot.Types
{
	internal static class TelegramUpdateHandler
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
			BotCommand? prevCommand = null;
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

				_chats.Add(chatId, (BotCommand)(type.GetConstructor([])?.Invoke([]) ?? throw new NullReferenceException($"Не удалось создать объект класса {type.FullName}")));
				messageText = messageText.Replace($"/{commandName}", "").TrimStart();
			}

			if (_chats.TryGetValue(chatId, out BotCommand? command))
			{
				if (command.IsLong)
					await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);

				bool finished = true;
				TextMessage? response = null;
				try
				{
					response = command.Respond(messageText, out finished);
				}
				catch (CancelException e)
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
						response = new(text);
					}
				}

				if (finished)
					_chats.Remove(chatId);
				if (response is not null)
				{
					response.CancellationToken = cancellationToken;
					await response.SendTextMessageAsync(chatId, botClient);
				}
			}
			else
				await botClient.SendTextMessageAsync(chatId, $"\"{messageText}\" не является командой или ответом на выполняемую команду", cancellationToken: cancellationToken);
		}

		public async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			try
			{
				switch (update.Type)
				{
					case UpdateType.Message when update.Message is { } message && message.Text is { } messageText:
						await HandleMessageAsync(botClient, message, messageText, cancellationToken);
						break;
					case UpdateType.CallbackQuery when update.CallbackQuery is { } callbackQuery && callbackQuery.Data is { } callbackData:
						await HandleCallbackQueryAsync(botClient, callbackQuery, callbackData, cancellationToken);
						break;
				}
			}
			catch (Exception e)
			{
				Logs.Instance.AddError(e);
			}
		}
	}
}