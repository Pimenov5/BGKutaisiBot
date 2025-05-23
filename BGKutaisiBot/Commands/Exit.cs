using BGKutaisiBot.Attributes;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Завершить и закрыть программу")]
	internal class Exit
	{
		public static void Respond() => throw new Types.Exceptions.ExitException();
	}
}