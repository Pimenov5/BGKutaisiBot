namespace BGKutaisiBot.Types.Exceptions
{
	internal class CancelException : Exception
	{
		public enum Cancel { Current, Previous };
		public readonly Cancel Cancelling;
		public readonly string? Reason;
		public CancelException(Cancel cancelling, string? reason = default)
		{
			this.Cancelling = cancelling;
			this.Reason = reason;
		}
	}
}