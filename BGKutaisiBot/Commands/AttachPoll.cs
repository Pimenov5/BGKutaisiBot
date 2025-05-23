using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Отправить опрос в комментарии в новому сообщению")]
	internal class AttachPoll : SendPoll
	{
		public static new async Task RespondAsync(ITelegramBotClient botClient, string chatId, string pollCollectionId, CancellationToken cancellationToken)
		{
			Poll poll = await PreparePollAsync(int.Parse(pollCollectionId));

			string text = poll.Question + "\nГолосуйте за одну или несколько игр в комментариях";
			await new TextMessage("> " + text) { ParseMode = ParseMode.MarkdownV2, CancellationToken = cancellationToken }.
				SendTextMessageAsync(chatId, botClient);

			async Task HandleNotPrivateTextMessage(Type type, ITelegramBotClient botClient, Message message, string messageText, CancellationToken cancellationToken)
			{
				if (text != messageText.TrimStart())
					return;

				TelegramUpdateHandler.NotPrivateTextMessageEvent -= HandleNotPrivateTextMessage;
				Message pollMessage = await botClient.SendPollAsync(message.Chat.Id, poll.Question, poll.Options, isAnonymous: false, allowsMultipleAnswers: true, replyToMessageId: message.MessageId,
					replyMarkup: poll.ReplyMarkup, cancellationToken: cancellationToken) ?? throw new NullReferenceException($"Не удалось отправить в {message.Chat.Id} опрос \"{poll.Question}\"");
				Logs.Instance.Add($"{(pollMessage.Chat.Username is null ? $"ID {pollMessage.Chat.Id}" : $"@{pollMessage.Chat.Username}")} получил сообщение ID {pollMessage.MessageId} c опросом \"{poll.Question}\"");
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