using Telegram.Bot.Types.ReplyMarkups;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tesera.Models;
using Tesera.Types.Enums;

namespace BGKutaisiBot.Commands
{
	internal class SendPoll
	{
		static readonly Lazy<HttpClient> _lazyHttpClient = new();

		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string pollCollectionId, CancellationToken cancellationToken)
		{
			int collectionId = int.Parse(pollCollectionId);
			Tesera.TeseraClient teseraClient = new(_lazyHttpClient.Value);
			CustomCollectionInfo collectionInfo = teseraClient.Get<CustomCollectionInfo>(new Tesera.API.Collections.Custom(collectionId))
				?? throw new NullReferenceException($"Не удалось получить информацию о коллекции с ID #{collectionId}");
			if (collectionInfo.GamesTotal <= 0)
				throw new InvalidOperationException($"В коллекции \"{collectionInfo.Title}\" отсутствуют игры");

			var collectionGames = teseraClient.Get<IEnumerable<CustomCollectionGameInfo>>(new Tesera.API.Collections.Custom.GamesClear(collectionId, GamesType.All, collectionInfo.GamesTotal))
				?? throw new NullReferenceException($"Не удалось получить список игр в коллекции \"{collectionInfo.Title}\"");

			string[] options = new string[collectionInfo.GamesTotal];
			int i = 0;
			foreach (CustomCollectionGameInfo item in collectionGames)
			{
				string comment = string.IsNullOrEmpty(item.Comment) ? string.Empty : item.Comment;
				string? ignoreChar = Environment.GetEnvironmentVariable("POLL_IGNORE_CHAR");
				if (string.IsNullOrEmpty(comment) || ignoreChar is not null && !comment.StartsWith(ignoreChar))
					if (string.IsNullOrEmpty(item.Game.Title))
						throw new NullReferenceException($"Не удалось получить имя игры {item.Game.TeseraId}");
					else
						options[i++] = $"{item.Game.Title}{(string.IsNullOrEmpty(comment) ? "" : $" {comment}")}";
			}

			Array.Resize(ref options, i);
			IReplyMarkup? replyMarkup = null;
			if (Environment.GetEnvironmentVariable("POLL_COLLECTION_USER_ID") is string collectionUserId && int.TryParse(collectionUserId, out int userId))
				replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Игры из опроса на сайте Tesera.ru") { Url = $"tesera.ru/user/{userId}/lists/{collectionId}" });

			Message pollMessage = await botClient.SendPollAsync(chatId, collectionInfo.Title ?? throw new NullReferenceException($"У коллекции с ID {collectionInfo.Id} отсутствует название"),
				options, allowsMultipleAnswers: true, replyMarkup: replyMarkup)
				?? throw new NullReferenceException("Не удалось отправить опрос");
			Logs.Instance.Add($"@{pollMessage.Chat.Username} получил сообщение (ID {pollMessage.MessageId}) с опросом: {collectionInfo.Title}");
		}
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
		{
			string pollCollectionId = Environment.GetEnvironmentVariable("POLL_COLLECTION_ID") ?? throw new NullReferenceException("В переменных окружения отсутствует идентификатор списка с играми для опроса");
			await RespondAsync(botClient, chatId, pollCollectionId, cancellationToken);
		}
	}
}