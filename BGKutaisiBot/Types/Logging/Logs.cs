namespace BGKutaisiBot.Types.Logging
{
	internal class Logs
	{
		readonly IList<Log> _logs = [];
		readonly static Lazy<Logs> _lazyInstance = new();

		public static Logs Instance { get { return _lazyInstance.Value; } }
		public void Add(Log log, bool writeConsole = false)
		{
			_logs.Add(log);
			if (writeConsole)
				Console.WriteLine(log);
		}
		public void AddError(Exception e) => this.Add(new ErrorLog(e), true);
		public void Add(string text, bool writeConsole = false) => this.Add(new Log(text), writeConsole);
	}
}