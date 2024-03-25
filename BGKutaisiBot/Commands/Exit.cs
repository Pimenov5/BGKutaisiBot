namespace BGKutaisiBot.Commands
{
	internal class Exit
	{
		public static void Respond() => throw new Types.Exceptions.ExitException();
	}
}