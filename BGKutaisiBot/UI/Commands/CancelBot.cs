using BGKutaisiBot.Types.Logging;

namespace BGKutaisiBot.UI.Commands
{
	internal class CancelBot : Command
	{
		Func<CancellationTokenSource?> _getCancellationTokenSource;
		public CancelBot(Func<CancellationTokenSource?> getCancellationTokenSource) : base("остановить бота")
		{
			_getCancellationTokenSource = getCancellationTokenSource;
			this.Add(0, (string[] args) =>
			{
				CancellationTokenSource cancellationTokenSource = _getCancellationTokenSource.Invoke()
					?? throw new NullReferenceException("Невозможно остановить бота, т.к., возможно, он ещё не был запущен");
				if (cancellationTokenSource.IsCancellationRequested)
					throw new InvalidOperationException("Бот уже остановлен");

				cancellationTokenSource.Cancel();
				cancellationTokenSource.Dispose();
				Logs.Instance.Add("Бот был остановлен", true);
				return Task.CompletedTask;
			});
		}
	}
}