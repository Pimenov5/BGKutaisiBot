using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tesera.Models;
using Tesera.Types.Enums;

namespace BGKutaisiBot.UI.Commands
{
	internal class SendPoll : BotCommand
	{
		readonly Lazy<HttpClient> _lazyHttpClient = new();

		public SendPoll(Func<ITelegramBotClient?> getBotClient) : base("отправить опрос", getBotClient)
		{
			async Task Function(string[] args)
			{
				int collectionId = int.Parse(args[1]);
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
					if (string.IsNullOrEmpty(comment) || !comment.StartsWith(Configuration.Instance.Poll.IgnoreChar))
						if (string.IsNullOrEmpty(item.Game.Title))
							throw new NullReferenceException($"Не удалось получить имя игры {item.Game.TeseraId}");
						else
							options[i++] = $"{item.Game.Title}{(string.IsNullOrEmpty(comment) ? "" : $" {comment}")}";
				}

				Array.Resize(ref options, i);
				Message pollMessage = await this.BotClient.SendPollAsync(args[0], collectionInfo.Title ?? throw new NullReferenceException($"У коллекции с ID {collectionInfo.Id} отсутствует название"), 
					options, allowsMultipleAnswers: true)
					?? throw new NullReferenceException("Не удалось отправить опрос");
				Logs.Instance.Add($"Опрос (ID {pollMessage.MessageId}) отправлен в @{pollMessage.Chat.Username}", true);
			}

			this.Add(1, (string[] args) => Function(args.Append(Configuration.Instance.Poll.CollectionId.ToString()).ToArray()));
			this.Add(2, Function);
		}
	}
}