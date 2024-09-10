using BGKutaisiBot.Types;

namespace BGKutaisiBot.Commands
{
	internal class Exit : IConsoleCommand
	{
		public static string Description { get => "Завершить и закрыть программу"; }
		public static void Respond() => throw new Types.Exceptions.ExitException();
	}
}