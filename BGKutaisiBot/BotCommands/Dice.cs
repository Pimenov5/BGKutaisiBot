using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BGKutaisiBot.BotCommands
{
	[BotCommand("Бросить кубик D6", "определяет случайную цифру (1-6) с помощью шестигранного кубика. Можно указать число бросков, например: /dice 2")]
	internal class Dice : Types.BotCommand
	{
		public override string[] GetArguments(Message message)
		{
			string[] result = base.GetArguments(message);
			Array.Resize(ref result, result.Length + 1);
			result[^1] = message.Chat.Id.ToString();
			return result;
		}

		public static async Task<TextMessage?> RespondAsync(ITelegramBotClient botClient, string[] args) {
			if (args.Length == 0)
				throw new ArgumentException("Минимальное количество аргументов команды равно одному: идентификатор чата", nameof(args));
			if (!long.TryParse(args[^1], out long chatId))
				throw new InvalidCastException($"{chatId} не является идентификатором чата");

			Array.Resize(ref args, args.Length - 1);
			uint count;
			switch (args.Length)
			{
				case 0:
					count = 1;
					break;
				case 1 when uint.TryParse(args[0], out count):
					break;
				default:
					return new TextMessage((args.Length == 1 ? $"\"{args[0]}\" не является числом бросков" : "Команда имеет только один параметр"));

			}

			for (int i = 0; i < count; i++)
				await botClient.SendDiceAsync(chatId);

			return null;
		}

		public static void Respond(string strCount)
		{
			if (uint.TryParse(strCount, out uint count))
			{
				for (int i = 0; i < count; i++)
					Console.WriteLine('[' + new Random().Next(1, 6).ToString() + ']');
			}
			else
				Console.WriteLine($"\"{strCount}\" не является числом бросков");
		}
	}
}