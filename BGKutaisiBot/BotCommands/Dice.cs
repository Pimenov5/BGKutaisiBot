using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Dice : BotCommand
	{
		public static string Description { get => "Бросить кубик D6"; }
		public static string Instruction { get => "определяет случайное число с помощью шестигранного кубика"; }
		public override TextMessage? Respond(string? messageText, out bool finished) { throw new RollDiceException(); }
	}
}