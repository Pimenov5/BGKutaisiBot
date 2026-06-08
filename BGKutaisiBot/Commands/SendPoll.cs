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
		private static async Task<Poll> CreatePollFromTeseraAsync(int teseraCollectionId)
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
			string? ignoreChar = Environment.GetEnvironmentVariable("POLL_IGNORE_CHAR");
			foreach (CustomCollectionGameInfo item in collectionGames)
			{
				string comment = string.IsNullOrEmpty(item.Comment) ? string.Empty : item.Comment;
				if (string.IsNullOrEmpty(comment) || ignoreChar is not null && !comment.StartsWith(ignoreChar))
					if (string.IsNullOrEmpty(item.Game.Title))
						throw new NullReferenceException($"Не удалось получить имя игры {item.Game.TeseraId}");
					else
						options[i++] = $"{item.Game.Title}{(string.IsNullOrEmpty(comment) ? "" : $" {comment}")}";
			}

			if (i < 2 || i > Poll.OPTIONS_MAX)
				throw new InvalidOperationException($"Количество вариантов ответов из коллекции \"{collectionInfo.Title}\" равно {i}, но это количество не может быть меньше двух или больше десяти");

			Array.Resize(ref options, i);
			ReplyMarkup? replyMarkup = null;
			if (Environment.GetEnvironmentVariable("POLL_COLLECTION_USER_ID") is string collectionUserId && int.TryParse(collectionUserId, out int userId))
				replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Игры из опроса на сайте Tesera.ru") { Url = $"tesera.ru/user/{userId}/lists/{teseraCollectionId}" });

			return new(collectionInfo.Title, options, replyMarkup);
		}

		public readonly struct Poll(string question, string[] options, ReplyMarkup? replyMarkup)
		{
			public const int OPTIONS_MAX = 12; // poll_answers_max

			public readonly string Question = question;
			public readonly InputPollOption[] Options = options.Length >= 2 && options.Length <= OPTIONS_MAX ? [..options] 
				: throw new ArgumentException($"Количество вариантов ответов для опроса должно быть от 2 до {Poll.OPTIONS_MAX}, а не {options.Length}", nameof(options));
			public readonly ReplyMarkup? ReplyMarkup = replyMarkup;
		}

		public static async Task<Poll> PreparePollAsync(int teseraCollectionId, string question = "Во что играем сегодня?")
		{
			try
			{
				Poll poll = await SendPoll.CreatePollFromTeseraAsync(teseraCollectionId);
				return poll;
			}
			catch (Exception e)
			{
				Logs.AddError(e);
			}

			const string SEND_POLL_OPTIONS = "SEND_POLL_OPTIONS";
			if (Environment.GetEnvironmentVariable(SEND_POLL_OPTIONS) is not string optionsStr || optionsStr.Split(';') is not string[] options || options.Length <= 1)
				throw new Exception($"В переменных окружения отсутствует валидное значение {SEND_POLL_OPTIONS}");

			int index = 0;
			string? ignoreChar = Environment.GetEnvironmentVariable("POLL_IGNORE_CHAR");
			foreach (string item in options)
				if (string.IsNullOrEmpty(ignoreChar) || !item.StartsWith(ignoreChar))
					options[index++] = item;

			Array.Resize(ref options, index + 1);
			return new(question, options, null);
		}

		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, string pollCollectionId, CancellationToken cancellationToken)
		{
			Poll poll = await PreparePollAsync(int.Parse(pollCollectionId));

			Message pollMessage = await botClient.SendPoll(chatId, poll.Question, poll.Options, allowsMultipleAnswers: true, replyMarkup: poll.ReplyMarkup, cancellationToken: cancellationToken)
				?? throw new NullReferenceException($"Не удалось отправить в чат {chatId} опрос \"{poll.Question}\"");
			Logs.Add($"@{pollMessage.Chat.Username} получил сообщение (ID {pollMessage.MessageId}) с опросом: {poll.Question}");
		}
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
		{
			string pollCollectionId = Environment.GetEnvironmentVariable("POLL_COLLECTION_ID") ?? throw new NullReferenceException("В переменных окружения отсутствует идентификатор списка с играми для опроса");
			await RespondAsync(botClient, chatId, pollCollectionId, cancellationToken);
		}
	}
}