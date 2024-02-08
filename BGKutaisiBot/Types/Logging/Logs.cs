namespace BGKutaisiBot.Types.Logging
{
	internal class Logs
	{
		readonly IList<Log> _logs = [];
		readonly static Lazy<Logs> _lazyInstance = new();

		public static Logs Instance { get { return _lazyInstance.Value; } }
		public void Add(Log log, bool writeConsole)
		{
			_logs.Add(log);
			if (writeConsole)
				Console.WriteLine(log);
		}
		public void AddError(Exception e) => this.Add(new ErrorLog(e), true);
		public void Add(string text, bool writeConsole) => this.Add(new Log(text), writeConsole);
		public void Add(string text) => this.Add(text, System.Diagnostics.Debugger.IsAttached);
		public IEnumerable<Log> ToEnumerable() => _logs;

		public int Count => _logs.Count;
		public Log First => _logs.First();
		public Log Last => _logs.Last();
	}
}