using Telegram.Bot.Types.ReplyMarkups;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tesera.Models;
using Tesera.Types.Enums;
using BGKutaisiBot.Attributes;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Отправить опрос с играми из коллекции")]
	internal class SendPoll
	{
		static readonly Lazy<HttpClient> _lazyHttpClient = new();

		static private protected string PreparePoll(int teseraCollectionId, out string[] options, out IReplyMarkup? replyMarkup)
		{
			Tesera.TeseraClient teseraClient = new(_lazyHttpClient.Value);
			CustomCollectionInfo collectionInfo = teseraClient.Get(new Tesera.API.Collections.Custom(teseraCollectionId))
				?? throw new NullReferenceException($"Не удалось получить информацию о коллекции с ID #{teseraCollectionId}");
			if (string.IsNullOrEmpty(collectionInfo.Title))
				throw new NullReferenceException($"У коллекции c ID {teseraCollectionId} отсутствует название");
			if (collectionInfo.GamesTotal <= 0)
				throw new InvalidOperationException($"В коллекции \"{collectionInfo.Title}\" отсутствуют игры");

			var collectionGames = teseraClient.Get(new Tesera.API.Collections.Custom.GamesClear(teseraCollectionId, GamesType.All, collectionInfo.GamesTotal))
				?? throw new NullReferenceException($"Не удалось получить список игр в коллекции \"{collectionInfo.Title}\"");

			options = new string[collectionInfo.GamesTotal];
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

			if (i < 2 || i > 10)
				throw new InvalidOperationException($"Количество вариантов ответов из коллекции \"{collectionInfo.Title}\" равно {i}, но это количество не может быть меньше двух или больше десяти");

			Array.Resize(ref options, i);
			replyMarkup = null;
			if (Environment.GetEnvironmentVariable("POLL_COLLECTION_USER_ID") is string collectionUserId && int.TryParse(collectionUserId, out int userId))
				replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Игры из опроса на сайте Tesera.ru") { Url = $"tesera.ru/user/{userId}/lists/{teseraCollectionId}" });

			return collectionInfo.Title;
		}

		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string pollCollectionId, CancellationToken cancellationToken)
		{
			string question = PreparePoll(int.Parse(pollCollectionId), out string[] options, out IReplyMarkup? replyMarkup);

			Message pollMessage = await botClient.SendPollAsync(chatId, question, options, allowsMultipleAnswers: true, replyMarkup: replyMarkup)
				?? throw new NullReferenceException($"Не удалось отправить в чат {chatId} опрос \"{question}\"");
			Logs.Instance.Add($"@{pollMessage.Chat.Username} получил сообщение (ID {pollMessage.MessageId}) с опросом: {question}");
		}
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
		{
			string pollCollectionId = Environment.GetEnvironmentVariable("POLL_COLLECTION_ID") ?? throw new NullReferenceException("В переменных окружения отсутствует идентификатор списка с играми для опроса");
			await RespondAsync(botClient, chatId, pollCollectionId, cancellationToken);
		}
	}
}