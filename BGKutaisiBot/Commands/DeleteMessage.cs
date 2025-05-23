using BGKutaisiBot.Types.Logging;
using Telegram.Bot;

namespace BGKutaisiBot.Commands
{
	internal class DeleteMessage
	{
		public static string Description { get => "Удалить сообщение"; }
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string messageId, CancellationToken cancellationToken)
		{
			await botClient.DeleteMessageAsync(chatId, int.Parse(messageId), cancellationToken);
			Logs.Instance.Add($"Сообщение (ID {messageId}) удалено в чате {chatId}");
		}
	}
}