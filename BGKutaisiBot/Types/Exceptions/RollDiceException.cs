namespace BGKutaisiBot.Types.Exceptions
{
	internal class RollDiceException : Exception
	{
		public readonly uint Count;
		public RollDiceException(uint count) => this.Count = count;
	}
}