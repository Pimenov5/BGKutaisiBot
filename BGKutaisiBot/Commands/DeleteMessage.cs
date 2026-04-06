using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Удалить сообщение")]
	internal class DeleteMessage
	{
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string messageId, CancellationToken cancellationToken)
		{
			await botClient.DeleteMessage(chatId, int.Parse(messageId), cancellationToken);
			Logs.Add($"Сообщение (ID {messageId}) удалено в чате {chatId}");
		}
	}
}