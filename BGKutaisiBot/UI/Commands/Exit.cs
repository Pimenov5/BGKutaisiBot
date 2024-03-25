namespace BGKutaisiBot.UI.Commands
{
	internal class Exit
	{
		public static void Respond() => throw new Types.Exceptions.ExitException();
	}
}