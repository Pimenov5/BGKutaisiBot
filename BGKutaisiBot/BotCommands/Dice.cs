using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Dice : BotCommand
	{
		public static string Description { get => "Бросить кубик D6"; }
		public static string Instruction { get => "определяет случайную цифру (1-6) с помощью шестигранного кубика. Можно указать число бросков, например:\n/dice 2"; }
		public override TextMessage? Respond(string[] args, out bool finished) {
			uint count;
			switch (args.Length)
			{
				case 0:
					count = 1;
					break;
				case 1 when uint.TryParse(args[0], out count):
					break;
				default:
					finished = true;
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