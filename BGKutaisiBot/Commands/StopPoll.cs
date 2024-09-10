using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;

namespace BGKutaisiBot.Commands
{
	internal class StopPoll : IAsyncConsoleCommand
	{
		public static string Description { get => "Остановить опрос"; }
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string messageId, CancellationToken cancellationToken)
		{
			await botClient.StopPollAsync(chatId, int.Parse(messageId), cancellationToken: cancellationToken);
			Logs.Instance.Add($"Остановлен опрос в чате ID {chatId} в сообщении ID {messageId}");
		}
	}
}