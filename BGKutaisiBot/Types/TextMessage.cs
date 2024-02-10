using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BGKutaisiBot.Types
{
	internal class TextMessage
	{
		public string Text;
		public int? MessageThreadId;
		public ParseMode? ParseMode;
		public IEnumerable<MessageEntity>? Entities;
		public bool? DisableWebPagePreview;
		public bool? DisableNotification;
		public bool? ProtectContent;
		public int? ReplyToMessageId;
		public bool? AllowSendingWithoutReply;
		public IReplyMarkup? ReplyMarkup;
		public CancellationToken CancellationToken = default;

		public TextMessage(string text) => this.Text = text;
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