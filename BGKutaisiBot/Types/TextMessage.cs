using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BGKutaisiBot.Types
{
	internal class TextMessage(string text, bool addRollDiceKeyboard = false)
	{
		public string Text = text;
		public bool AddRollDiceKeyboard = addRollDiceKeyboard;
		public int? MessageThreadId = null;
		public ParseMode? ParseMode = null;
		public IEnumerable<MessageEntity>? Entities = null;
		public bool? DisableWebPagePreview = null;
		public bool? DisableNotification = null;
		public bool? ProtectContent = null;
		public int? ReplyToMessageId = null;
		public bool? AllowSendingWithoutReply = null;
		public IReplyMarkup? ReplyMarkup = null;
		public CancellationToken CancellationToken = default;

		public async Task<Message> SendTextMessageAsync(ChatId chatId, ITelegramBotClient botClient)
		{
			Message message = await botClient.SendTextMessageAsync(chatId, this.Text, this.MessageThreadId, this.ParseMode, this.Entities, this.DisableWebPagePreview,
				this.DisableNotification, this.ProtectContent, this.ReplyToMessageId, this.AllowSendingWithoutReply,
				this.ReplyMarkup ?? (this.AddRollDiceKeyboard
					? new ReplyKeyboardMarkup(new KeyboardButton(TelegramUpdateHandler.ROLL_DICE_KEYBOARD_TEXT)) { ResizeKeyboard = true } : new ReplyKeyboardRemove()),
				this.CancellationToken)
				?? throw new NullReferenceException($"Не удалось отправить {chatId} сообщение {this}");

			Logs.Instance.Add($"@{message.Chat.Username} получил сообщение (ID {message.MessageId}):  {this}");
			return message;
		}
		public override string ToString()
		{
			int length = this.Text.Length;
			string result = this.Text.Replace("\n", "\\n").Replace("\r", "\\r");
			const int MAX_LENGTH = 60;
			result = result.Length > MAX_LENGTH ? result.Remove(MAX_LENGTH) : result;
			return $"{result}{(result.Length < length ? " ..." : string.Empty)}";
		}
	}
}