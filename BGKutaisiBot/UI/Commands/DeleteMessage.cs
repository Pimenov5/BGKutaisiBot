using BGKutaisiBot.Types.Logging;
using Telegram.Bot;

namespace BGKutaisiBot.UI.Commands
{
	internal class DeleteMessage
	{
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string messageId, CancellationToken cancellationToken)
		{
			await botClient.DeleteMessageAsync(chatId, int.Parse(messageId), cancellationToken);
			Logs.Instance.Add($"Сообщение (ID {messageId}) удалено в чате {chatId}");
		}
	}
}