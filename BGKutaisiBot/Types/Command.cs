namespace BGKutaisiBot.Types
{
	internal abstract class Command
	{
		public static string[] Split(string expression) => expression.Split(expression.Contains('\n') ? "\n" : " ", StringSplitOptions.RemoveEmptyEntries);
	}
}