namespace BGKutaisiBot.Types.Logging
{
	public static class Logs
	{
		readonly static IList<Log> s_logs = [];

		public static void Add(Log log, bool writeConsole)
		{
			s_logs.Add(log);
			if (writeConsole)
				Console.WriteLine(Truncator.Truncate(log.ToString()));
		}
		public static void AddError(Exception e) => Add(new ErrorLog(e), true);
		public static void Add(string text, bool writeConsole) => Add(new Log(text), writeConsole);
		public static void Add(string text) => Add(text, System.Diagnostics.Debugger.IsAttached);
		public static IEnumerable<Log> ToEnumerable() => s_logs;

		public static int Count => s_logs.Count;
		public static Log First => s_logs.First();
		public static Log Last => s_logs.Last();
	}
}