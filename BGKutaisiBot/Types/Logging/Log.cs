namespace BGKutaisiBot.Types.Logging
{
	public class Log(string text)
	{
		public readonly DateTime DateTime = DateTime.Now;
		public readonly string Text = text;
		public override string ToString() => $"{this.DateTime.ToShortTimeString()} {this.Text}";
	}

	internal class ErrorLog(Exception ex) : Log(ex.Message)
	{
		public readonly string ExceptionFullName = ex.GetType().FullName ?? "?";
		public override string ToString() => $"[{this.ExceptionFullName}] {base.ToString()}";
	}
}