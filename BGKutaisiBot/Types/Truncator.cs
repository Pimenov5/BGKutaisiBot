namespace BGKutaisiBot.Types
{
	internal static class Truncator
	{
		const int MAX_STRING_LENGTH = 80;
		public static string Truncate(string value, int length = MAX_STRING_LENGTH, bool addEllipsis = true, bool replaceLineBreak = true)
		{
			string result = string.IsNullOrEmpty(value) ? value : value.Length > length ? value.Remove(length) + (addEllipsis ? "..." : string.Empty) : value;
			return replaceLineBreak ? result.Replace("\n", "\\n").Replace("\r", "\\r") : result;
		}
	}
}
