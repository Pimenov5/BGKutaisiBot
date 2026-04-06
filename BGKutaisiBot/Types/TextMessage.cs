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
		public ParseMode ParseMode = ParseMode.None;
		public IEnumerable<MessageEntity>? Entities = null;
		public LinkPreviewOptions? LinkPreviewOptions = null;
		public bool DisableNotification = false;
		public bool ProtectContent = false;
		public int? ReplyToMessageId = null;
		public bool? AllowSendingWithoutReply = null;
		public ReplyMarkup? ReplyMarkup = null;
		public CancellationToken CancellationToken = default;

		public async Task<Message> SendTextMessageAsync(ChatId chatId, ITelegramBotClient botClient)
		{
			ReplyMarkup? replyMarkup = this.ReplyMarkup ?? (this.AddRollDiceKeyboard
				? new ReplyKeyboardMarkup(new KeyboardButton(TelegramUpdateHandler.ROLL_DICE_KEYBOARD_TEXT)) { ResizeKeyboard = true }
				: chatId.ToString() is string chat && (chat.StartsWith('@') || chat.StartsWith('-')) ? null : new ReplyKeyboardRemove());

			Message message = await botClient.SendMessage(chatId, this.Text, this.ParseMode, this.ReplyToMessageId, replyMarkup, this.LinkPreviewOptions, this.MessageThreadId,
				this.Entities, this.DisableNotification, this.ProtectContent, cancellationToken: this.CancellationToken) 
				?? throw new NullReferenceException($"Не удалось отправить {chatId} сообщение {this}");

			Logs.Instance.Add($"@{message.Chat.Username} получил сообщение (ID {message.MessageId}):  {this}");
			return message;
		}
		public override string ToString() => Truncator.Truncate(this.Text, replaceLineBreak: false);
	}
}