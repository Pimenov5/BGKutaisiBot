using BGKutaisiBot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
using Tesera;
using Tesera.Models;
using Tesera.Types.Enums;
using System.Text;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.Commands
{
	internal class Collection : BotCommand
	{
		enum SortBy { Titles, Players, Playtimes, Ratings }
		const string USER_ALIAS_VARIABLE_NAME_PREFIX = "COLLECTION_USER_ALIAS_";

		static readonly Lazy<HttpClient> _lazyHttpClient = new();
		static TextMessage GetTextMessage(string environmentVariableName, SortBy sortBy)
		{
			string? collectionUri = Environment.GetEnvironmentVariable(environmentVariableName);
			if (string.IsNullOrWhiteSpace(collectionUri))
				return new TextMessage($"В переменных среды отсутствует значение для \"{environmentVariableName}\"");

			Regex regex = new("tesera.ru/user/(\\w+)/games/owns");
			Match? match = regex.IsMatch(collectionUri) ? regex.Match(collectionUri) : null;
			GroupCollection? groupCollection = match?.Groups;
			string? userAlias = groupCollection?.Count == 2 ? groupCollection[1].Value : null;
			if (string.IsNullOrEmpty(userAlias))
				return new TextMessage($"Не удалось выделить алиас пользователя из ссылки {collectionUri}");

			TeseraClient teseraClient = new(_lazyHttpClient.Value);
			var gamesInfo = teseraClient.Get<IEnumerable<CustomCollectionGameInfo>>(new Tesera.API.Collections.Base(CollectionType.Own, userAlias, GamesType.All));
			if (gamesInfo is null)
				return new TextMessage($"Не удалось получить список игр из коллекции по ссылке {collectionUri}");

			List<GameInfo> games = [];
			foreach (CustomCollectionGameInfo item in gamesInfo)
				if (!item.Game.IsAddition && !string.IsNullOrEmpty(item.Game.Alias))
				{
					GameInfoResponse? game = teseraClient.Get<GameInfoResponse>(new Tesera.API.Games(item.Game.Alias));
					if (game is not null)
						games.Add(game.Game);
				}

			if (games.Count == 0)
				return new TextMessage($"Не удалось получить информацию об играх из коллекции по ссылке {collectionUri}");

			games.Sort((GameInfo x, GameInfo y) =>
			{
				switch (sortBy)
				{
					default:
					case SortBy.Titles:
						return string.Compare(x.Title, y.Title);
					case SortBy.Playtimes:
						int result = x.PlaytimeMax.CompareTo(y.PlaytimeMax);
						return result == 0 ? x.PlaytimeMin.CompareTo(y.PlaytimeMin) : result;
					case SortBy.Players:
						result = x.PlayersMax.CompareTo(y.PlayersMax);
						return result == 0 ? x.PlayersMin.CompareTo(y.PlayersMin) : result;
					case SortBy.Ratings:
						return x.N10Rating.CompareTo(y.N10Rating);
				}
			});

			int i = 0;
			regex = new("(\\.|-|\\(|\\)|!|\\+)");
			StringBuilder stringBuilder = new();
			foreach (GameInfo game in games)
				if (!string.IsNullOrEmpty(game.Title))
				{
					string title = regex.Replace(game.Title, (Match match) => $"\\{match.Groups[0].Value}");
					string? playersCount = game.GetPlayersCount();
					if (!string.IsNullOrEmpty(playersCount))
						playersCount = regex.Replace(playersCount, (Match match) => $"\\{match.Groups[0].Value}");

					stringBuilder.AppendLine($"{++i}\\. [{title}](tesera.ru/game/{game.Alias?.Replace("-", "\\-")})");
					stringBuilder.AppendLine($"  {(i > 9 ? "  " : string.Empty)}"
						+ $"{(playersCount is null ? string.Empty : $"  👥{playersCount}")}"
						+ $"{(game.N10Rating == 0 ? string.Empty : $"  ⭐️{game.N10Rating}")}"
						+ $"{(game.PlaytimeMin == 0 ? string.Empty : $"  ⏳{(game.PlaytimeMin == game.PlaytimeMax || game.PlaytimeMax == 0 ? game.PlaytimeMin : $"{game.PlaytimeMin}\\-{game.PlaytimeMax}")}")}");
				}

			string methodNamePrefix = environmentVariableName.Remove(environmentVariableName.IndexOf('_')).ToLower();
			methodNamePrefix = $"{char.ToUpper(methodNamePrefix.First())}{methodNamePrefix.Remove(0, 1)}";

			SortBy[] values = Enum.GetValues<SortBy>();
			List<InlineKeyboardButton> buttons = new() { Capacity = values.Length - 1 };
			for (i = 0; i < values.Length; i++)
				if (values[i] != sortBy)
				{
					string callbackData = BotCommand.GetCallbackData(typeof(Collection), $"Get{methodNamePrefix}{Enum.GetName(values[i])}");
					buttons.Add(new InlineKeyboardButton(values[i] switch { SortBy.Titles => "🔤", SortBy.Players => "👥", SortBy.Playtimes => "⏳", SortBy.Ratings => "⭐️" })
						{ CallbackData = callbackData });
				}

			return new TextMessage(stringBuilder.ToString()) { ParseMode = ParseMode.MarkdownV2, ReplyMarkup = new InlineKeyboardMarkup(buttons), DisableWebPagePreview = true };
		}

		/*
		public static TextMessage GetFirstTitles() => GetTextMessage(FIRST_COLLECTION_VAR_NAME, SortBy.Titles);
		public static TextMessage GetSecondTitles() => GetTextMessage(SECOND_COLLECTION_VAR_NAME, SortBy.Titles);
		public static TextMessage GetFirstPlayers() => GetTextMessage(FIRST_COLLECTION_VAR_NAME, SortBy.Players);
		public static TextMessage GetSecondPlayers() => GetTextMessage(SECOND_COLLECTION_VAR_NAME, SortBy.Players);
		public static TextMessage GetFirstPlaytimes() => GetTextMessage(FIRST_COLLECTION_VAR_NAME, SortBy.Playtimes);
		public static TextMessage GetSecondPlaytimes() => GetTextMessage(SECOND_COLLECTION_VAR_NAME, SortBy.Playtimes);
		public static TextMessage GetFirstRatings() => GetTextMessage(FIRST_COLLECTION_VAR_NAME, SortBy.Ratings);
		public static TextMessage GetSecondRatings() => GetTextMessage(SECOND_COLLECTION_VAR_NAME, SortBy.Ratings);
		*/

		public static string Description { get => "Коллекции настольных игр для игротек"; }
		public override TextMessage Respond(string? messageText, out bool finished)
		{
			finished = true;
			HashSet<string> users = [];
			int i = 1;
			while (true)
			{
				string? value = Environment.GetEnvironmentVariable(USER_ALIAS_VARIABLE_NAME_PREFIX + i++);
				if (string.IsNullOrEmpty(value))
					break;
				else
					users.Add(value);
			}

			if (users.Count == 0)
				throw new CancelException(CancelException.Cancel.Current, "в переменных среды отсутствуют логины пользователей Tesera.ru");



			return new TextMessage($"Настольные игры для игротек хранятся в двух разных коллекциях: первая — [Васи](), вторая — [Саши и Антона]()\\."
				+ $"\nЧью коллекцию вы хотите посмотреть?") { ParseMode = ParseMode.MarkdownV2,
				ReplyMarkup = new InlineKeyboardMarkup([new InlineKeyboardButton("Васи") { CallbackData = BotCommand.GetCallbackData(typeof(Collection), "GetFirstTitles") },
					new InlineKeyboardButton("Саши и Антона") { CallbackData = BotCommand.GetCallbackData(typeof(Collection), "GetSecondTitles") }])
			}; 
		}
	}
}
