using BGKutaisiBot.Attributes;
using BGKutaisiBot.Commands;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BGKutaisiBot.BotCommands
{
	[BotCommand("Опрос по играм из списка", null, (int)ChatAction.Typing, BotCommandScopeType.AllChatAdministrators)]
	internal class GamesPoll : Types.BotCommand
	{
		public override string[] GetArguments(Message message)
		{
			List<string> result = [..base.GetArguments(message)];
			result.Insert(0, message.Chat.Id.ToString());
			return [..result];
		}

		public static async Task RespondAsync(ITelegramBotClient botClient, params string[] args)
		{
			if (args.Length < 1)
				throw new ArgumentOutOfRangeException(nameof(args), "Минимальное количество аргументов команды равно одному: идентификатор чата");
			if (!long.TryParse(args[0], out long chatId))
				throw new InvalidCastException($"{args[0]} не является идентификатором чата");

			if (args.Length == 1)
			{
				Array.Resize(ref args, args.Length + 1);
				args[^1] = Environment.GetEnvironmentVariable("GAMES_POLL_COLLECTION_ID")
					?? throw new NullReferenceException("В переменных окружения не задан идентификатор списка на сайте Tesera.ru");
			}

			if (!int.TryParse(args[1], out int teseraCollectionId))
				throw new InvalidCastException($"{args[1]} не является идентификатором списка на сайте Tesera.ru");

			SendPoll.Poll poll = await SendPoll.PreparePollAsync(teseraCollectionId);

			Message pollMessage = await botClient.SendPollAsync(chatId, poll.Question, poll.Options, 
				isAnonymous: false, type: PollType.Regular, allowsMultipleAnswers: true, replyMarkup: poll.ReplyMarkup);
			Logs.Instance.Add($"@{pollMessage.Chat.Username} получил сообщение (ID {pollMessage.MessageId}) с опросом: {poll.Question}");
		}
	}
}