using Telegram.Bot;

namespace BGKutaisiBot.UI
{
	internal abstract class BotCommand : Command
	{
		readonly Func<ITelegramBotClient?> _getBotClient;
		private protected ITelegramBotClient? GetBotClient() => _getBotClient();
		private protected ITelegramBotClient BotClient { get => _getBotClient() ?? throw new NullReferenceException("Отсутствует ссылка на бота, возможно, он ещё не был запущен"); }

		public BotCommand(string description, Func<ITelegramBotClient?> getBotClient) : base(description) => _getBotClient = getBotClient;
	}
}