namespace BGKutaisiBot.Types
{
	internal abstract class BotForm : BotCommand
	{
		private bool _isCompleted = false;
		public bool IsCompleted { get { return _isCompleted; } protected set { _isCompleted = value; } }
	}
}