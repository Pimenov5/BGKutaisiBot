using BGKutaisiBot.Types.Logging;
using Telegram.Bot;

namespace BGKutaisiBot.UI.Commands
{
	internal class DeleteMessage : BotCommand
	{
		public DeleteMessage(Func<ITelegramBotClient?> getBotClient) : base("удалить сообщение", getBotClient)
		{
			this.Add(2, (string[] args) => {
				this.BotClient.DeleteMessageAsync(args[0], int.Parse(args[1]));
				Logs.Instance.Add("Сообщение удалено", true);
				return Task.CompletedTask;
			});
		}
	}
}