namespace BGKutaisiBot.Types.Logging
{
	internal class Log
	{
		public readonly DateTime DateTime;
		public readonly string Text;
		public Log(string text)
		{
			this.DateTime = DateTime.Now;
			this.Text = text;
		}
		public override string ToString() => $"{this.DateTime.ToShortTimeString()} {this.Text}";
	}

	internal class ErrorLog : Log
	{
		public readonly string ExceptionFullName;
		public ErrorLog(Exception e) : base(e.Message) => this.ExceptionFullName = e.GetType().FullName ?? "?";
		public override string ToString() => $"[{this.ExceptionFullName}] {base.ToString()}";
	}
}