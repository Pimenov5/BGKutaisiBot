using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Отправить опрос в комментарии в новому сообщению")]
	internal class AttachPoll : SendPoll
	{
		public static new async Task RespondAsync(ITelegramBotClient botClient, string chatId, string pollCollectionId, CancellationToken cancellationToken)
		{
			string question = PreparePoll(int.Parse(pollCollectionId), out string[] options, out IReplyMarkup? replyMarkup);

			string text = question + "\nГолосуйте за одну или несколько игр в комментариях";
			await new TextMessage("> " + text) { ParseMode = ParseMode.MarkdownV2, CancellationToken = cancellationToken }.
				SendTextMessageAsync(chatId, botClient);

			async Task HandleNotPrivateTextMessage(Type type, ITelegramBotClient botClient, Message message, string messageText, CancellationToken cancellationToken)
			{
				if (text != messageText.TrimStart())
					return;

				TelegramUpdateHandler.NotPrivateTextMessageEvent -= HandleNotPrivateTextMessage;
				Message pollMessage = await botClient.SendPollAsync(message.Chat.Id, question, options, isAnonymous: false, allowsMultipleAnswers: true, replyToMessageId: message.MessageId,
					replyMarkup: replyMarkup, cancellationToken: cancellationToken) ?? throw new NullReferenceException($"Не удалось отправить в {message.Chat.Id} опрос \"{question}\"");
				Logs.Instance.Add($"{(pollMessage.Chat.Username is null ? $"ID {pollMessage.Chat.Id}" : $"@{pollMessage.Chat.Username}")} получил сообщение ID {pollMessage.MessageId} c опросом \"{question}\"");
			}

			TelegramUpdateHandler.NotPrivateTextMessageEvent += HandleNotPrivateTextMessage;
		}

		public static new async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
		{
			string pollCollectionId = Environment.GetEnvironmentVariable("POLL_COLLECTION_ID") ?? throw new NullReferenceException("В переменных окружения отсутствует идентификатор списка с играми для опроса");
			await RespondAsync(botClient, chatId, pollCollectionId, cancellationToken);
		}
	}
}