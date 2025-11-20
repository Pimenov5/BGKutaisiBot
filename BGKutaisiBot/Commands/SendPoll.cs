using Telegram.Bot.Types.ReplyMarkups;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tesera;
using Tesera.Models;
using Tesera.Types.Enums;
using BGKutaisiBot.Attributes;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Отправить опрос с играми из коллекции")]
	internal class SendPoll
	{
		public readonly struct Poll(string question, string[] options, IReplyMarkup? replyMarkup)
		{
			public readonly string Question = question;
			public readonly string[] Options = options;
			public readonly IReplyMarkup? ReplyMarkup = replyMarkup;
		}

		public static async Task<Poll> PreparePollAsync(int teseraCollectionId)
		{
			CustomCollectionInfo collectionInfo = await TeseraClient.Instance.GetAsync(new Tesera.API.Collections.Custom(teseraCollectionId))
				?? throw new NullReferenceException($"Не удалось получить информацию о коллекции с ID #{teseraCollectionId}");
			if (string.IsNullOrEmpty(collectionInfo.Title))
				throw new NullReferenceException($"У коллекции c ID {teseraCollectionId} отсутствует название");
			if (collectionInfo.GamesTotal <= 0)
				throw new InvalidOperationException($"В коллекции \"{collectionInfo.Title}\" отсутствуют игры");

			var collectionGames = await TeseraClient.Instance.GetAsync(new Tesera.API.Collections.Custom.GamesClear(teseraCollectionId, GamesType.All, collectionInfo.GamesTotal))
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

			const uint POLL_OPTIONS_MAX = 12; // poll_answers_max
			if (i < 2 || i > POLL_OPTIONS_MAX)
				throw new InvalidOperationException($"Количество вариантов ответов из коллекции \"{collectionInfo.Title}\" равно {i}, но это количество не может быть меньше двух или больше десяти");

			Array.Resize(ref options, i);
			IReplyMarkup? replyMarkup = null;
			if (Environment.GetEnvironmentVariable("POLL_COLLECTION_USER_ID") is string collectionUserId && int.TryParse(collectionUserId, out int userId))
				replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Игры из опроса на сайте Tesera.ru") { Url = $"tesera.ru/user/{userId}/lists/{teseraCollectionId}" });

			return new(collectionInfo.Title, options, replyMarkup);
		}

		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string pollCollectionId, CancellationToken cancellationToken)
		{
			Poll poll = await PreparePollAsync(int.Parse(pollCollectionId));

			Message pollMessage = await botClient.SendPollAsync(chatId, poll.Question, poll.Options, allowsMultipleAnswers: true, replyMarkup: poll.ReplyMarkup, cancellationToken: cancellationToken)
				?? throw new NullReferenceException($"Не удалось отправить в чат {chatId} опрос \"{poll.Question}\"");
			Logs.Instance.Add($"@{pollMessage.Chat.Username} получил сообщение (ID {pollMessage.MessageId}) с опросом: {poll.Question}");
		}
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
		{
			string pollCollectionId = Environment.GetEnvironmentVariable("POLL_COLLECTION_ID") ?? throw new NullReferenceException("В переменных окружения отсутствует идентификатор списка с играми для опроса");
			await RespondAsync(botClient, chatId, pollCollectionId, cancellationToken);
		}
	}
}