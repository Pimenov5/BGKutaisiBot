using Telegram.Bot;
using Telegram.Bot.Types;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;

namespace BGKutaisiBot.UI
{
	internal abstract class BotCommand : Command
	{
		readonly Func<ITelegramBotClient?> _getBotClient;
		private protected ITelegramBotClient? GetBotClient() => _getBotClient();
		private protected ITelegramBotClient BotClient { get => _getBotClient() ?? throw new NullReferenceException("Отсутствует ссылка на бота, возможно, он ещё не был запущен"); }

		public BotCommand(string description, Func<ITelegramBotClient?> getBotClient) : base(description) => _getBotClient = getBotClient;
		public async Task<Message> SendTextMessageAsync(ChatId chatId, TextMessage textMessage)
		{
			Message message = await this.BotClient.SendTextMessageAsync(chatId, textMessage.Text, textMessage.MessageThreadId,
				textMessage.ParseMode, textMessage.Entities, textMessage.DisableWebPagePreview, textMessage.DisableNotification, textMessage.ProtectContent, textMessage.ReplyToMessageId,
				textMessage.AllowSendingWithoutReply, textMessage.ReplyMarkup, textMessage.CancellationToken)
				?? throw new NullReferenceException($"Не удалось отправить {chatId} сообщение {textMessage}");

			Logs.Instance.Add($"В @{message.Chat.Username} отправлено сообщение (ID {message.MessageId}):  {textMessage}");
			return message;
		}
	}
}