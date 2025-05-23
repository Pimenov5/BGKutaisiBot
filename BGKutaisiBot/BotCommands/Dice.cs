using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	[BotCommand("Бросить кубик D6", "определяет случайную цифру (1-6) с помощью шестигранного кубика. Можно указать число бросков, например: /dice 2")]
	internal class Dice : BotCommand
	{
		public static TextMessage? Respond(string[] args) {
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
			throw new RollDiceException(count); 
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