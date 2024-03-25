using BGKutaisiBot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Globalization;

namespace BGKutaisiBot.Commands
{
	internal class SendNotice
	{
		public static string Description { get => "Отправить уведомление об игротеке"; }
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string arg1, string arg2, CancellationToken cancellationToken)
		{
			const string TEXT_MESSAGE =
				  "🏆  [Настольные игры](t.me/kutaisi\\_offline\\_rus/5077) за 5₾ \\#цена\r\n"
				+ "🗓️  Сегодня \\#{0} c {1} до 00:00\r\n"
				+ "[📍](goo.gl/maps/LfxoBVq7ytk4ZdP97)  Клуб настольных игр [Summer Set](t.me/summersetkutaisi) — [Google Maps](goo.gl/maps/LfxoBVq7ytk4ZdP97)\r\n\r\n"

				+ "Обязательно ставьте 👍, если планируете присоединится\r\n"
				+ "[Ответы на F\\.A\\.Q\\.](t.me/bg\\_kutaisi/{2}) {3}в канале [BGK](t.me/bg\\_kutaisi)";

			const string START_TIME = "19:00";
			TimeOnly startTime = TimeOnly.Parse(START_TIME);
			int pollMessageId = default;
			string[] args = [arg1, arg2];
			for (int i = 0; i < args.Length; i++)
				if (TimeOnly.TryParse(args[i], out TimeOnly parsedTime))
					startTime = startTime.ToString() == START_TIME ? parsedTime
						: throw new ArgumentException($"Два или более аргумента ({startTime}, {parsedTime}) могут быть интерпретированы как время начала");
				else if (int.TryParse(args[i], out int parsedMessageId))
					pollMessageId = pollMessageId == default ? parsedMessageId
						: throw new ArgumentException($"Два или более аргумента ({pollMessageId}, {parsedMessageId}) могут быть интерпретированы как идентификатор сообщения с опросом");

			await new TextMessage(string.Format(TEXT_MESSAGE, DateTime.Now.ToString("dMMMMyyyy", CultureInfo.GetCultureInfo("ru-RU")), startTime,
				Environment.GetEnvironmentVariable("NOTICE_FAQ_MESSAGE_ID") ?? "0", pollMessageId == default ? "" : $"и открытый [опрос по играм](t.me/bg\\_kutaisi/{pollMessageId}) "))
			{ ParseMode = ParseMode.MarkdownV2, CancellationToken = cancellationToken }.SendTextMessageAsync(chatId, botClient);
		}
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string arg1, CancellationToken cancellationToken) => await RespondAsync(botClient, chatId, arg1, string.Empty, cancellationToken);
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken) => await RespondAsync(botClient, chatId, string.Empty, cancellationToken);
	}
}