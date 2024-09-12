using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Dice : BotCommand, IConsoleCommand
	{
		public static string Description { get => "Бросить кубик D6"; }
		public static string Instruction { get => "определяет случайное число с помощью шестигранного кубика"; }
		public override TextMessage? Respond(string[] args, out bool finished) { throw new RollDiceException(); }
		public static void Respond()
		{
			Console.WriteLine('[' + new Random().Next(1, 6).ToString() + ']');
		}
	}
}