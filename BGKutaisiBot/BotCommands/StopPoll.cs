using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BGKutaisiBot.BotCommands
{
	internal class StopPoll : Types.BotCommand
	{
		public static string Description => "Остановить опрос";

		public override string[] GetArguments(Message message) => message.ReplyToMessage is Message replyToMessage && replyToMessage.Poll is Poll poll && !poll.IsClosed
			? [replyToMessage.Chat.Id.ToString(), replyToMessage.MessageId.ToString()] : throw new ArgumentException("Команда должна вызываться в ответ на незакрытый опрос");

		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string messageId, CancellationToken cancellationToken)
		{
			await botClient.StopPollAsync(chatId, int.Parse(messageId), cancellationToken: cancellationToken);
			Logs.Instance.Add($"Остановлен опрос в чате ID {chatId} в сообщении ID {messageId}");
		}
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string messageId) => await RespondAsync(botClient, chatId, messageId, default);
		public static async Task RespondAsync(ITelegramBotClient botClient, string[] args)
		{
			if (args.Length != 2)
				throw new ArgumentException("Требуется ID чата и сообщения с опросом");
			await RespondAsync(botClient, args[0], args[1]);
		}
	}
}